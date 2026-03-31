using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.Events.Bus;
using Fintex.Investments.Events;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Fintex.Investments.PaperTrading
{
    /// <summary>
    /// Keeps paper positions in sync with live market updates and auto-executes stop-loss / take-profit exits.
    /// </summary>
    public class PaperPositionRiskService : IPaperPositionRiskService, ITransientDependency
    {
        private readonly IPaperPositionRepository _paperPositionRepository;
        private readonly IPaperTradingAccountRepository _paperTradingAccountRepository;
        private readonly IPaperOrderRepository _paperOrderRepository;
        private readonly IPaperTradeFillRepository _paperTradeFillRepository;
        private readonly IEventBus _eventBus;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly ILogger<PaperPositionRiskService> _logger;

        public PaperPositionRiskService(
            IPaperPositionRepository paperPositionRepository,
            IPaperTradingAccountRepository paperTradingAccountRepository,
            IPaperOrderRepository paperOrderRepository,
            IPaperTradeFillRepository paperTradeFillRepository,
            IEventBus eventBus,
            IUnitOfWorkManager unitOfWorkManager,
            ILogger<PaperPositionRiskService> logger)
        {
            _paperPositionRepository = paperPositionRepository;
            _paperTradingAccountRepository = paperTradingAccountRepository;
            _paperOrderRepository = paperOrderRepository;
            _paperTradeFillRepository = paperTradeFillRepository;
            _eventBus = eventBus;
            _unitOfWorkManager = unitOfWorkManager;
            _logger = logger;
        }

        public async Task EvaluateAsync(MarketDataUpdatedEventData eventData, CancellationToken cancellationToken)
        {
            if (eventData == null || string.IsNullOrWhiteSpace(eventData.Symbol) || eventData.Price <= 0m)
            {
                return;
            }

            if (_unitOfWorkManager.Current == null)
            {
                using (var uow = _unitOfWorkManager.Begin())
                {
                    await EvaluateWithinCurrentUnitOfWorkAsync(eventData, cancellationToken);
                    await uow.CompleteAsync();
                }

                return;
            }

            await EvaluateWithinCurrentUnitOfWorkAsync(eventData, cancellationToken);
        }

        private async Task EvaluateWithinCurrentUnitOfWorkAsync(MarketDataUpdatedEventData eventData, CancellationToken cancellationToken)
        {
            using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                var positions = await GetOpenPositionsForMarketAsync(eventData.Symbol, eventData.Provider);
                if (positions.Count == 0)
                {
                    _logger.LogDebug(
                        "No open paper positions matched live update for {Symbol} from {Provider}.",
                        eventData.Symbol,
                        eventData.Provider);
                    return;
                }

                var occurredAt = eventData.Timestamp == default ? DateTime.UtcNow : eventData.Timestamp;
                var accountCache = new Dictionary<long, PaperTradingAccount>();
                var touchedAccountIds = new HashSet<long>();

                foreach (var position in positions)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    position.RefreshMarketPrice(eventData.Price, occurredAt);
                    touchedAccountIds.Add(position.AccountId);

                    var trigger = position.GetTriggeredRiskExit(eventData.Price);
                    if (trigger == PaperPositionRiskTrigger.None)
                    {
                        continue;
                    }

                    var account = await GetAccountAsync(position.AccountId, accountCache);
                    if (account == null || !account.IsActive)
                    {
                        continue;
                    }

                    var closingDirection = position.Direction == TradeDirection.Buy
                        ? TradeDirection.Sell
                        : TradeDirection.Buy;
                    var fillPrice = ResolveExitPrice(position, eventData.Price, eventData.Bid, eventData.Ask);
                    var quantity = position.Quantity;

                    var order = new PaperOrder(
                        position.TenantId,
                        position.UserId,
                        account.Id,
                        position.Symbol,
                        position.AssetClass,
                        position.Provider,
                        closingDirection,
                        PaperOrderType.Market,
                        quantity,
                        fillPrice,
                        position.StopLoss,
                        position.TakeProfit,
                        BuildExitNotes(trigger),
                        occurredAt);

                    await _paperOrderRepository.InsertAsync(order);
                    await _unitOfWorkManager.Current.SaveChangesAsync();

                    var realizedProfitLoss = position.Reduce(quantity, fillPrice, occurredAt);
                    account.ApplyRealizedProfitLoss(realizedProfitLoss, occurredAt);
                    order.MarkFilled(fillPrice, occurredAt, position.Id);

                    await _paperTradeFillRepository.InsertAsync(new PaperTradeFill(
                        position.TenantId,
                        position.UserId,
                        account.Id,
                        order.Id,
                        position.Id,
                        position.Symbol,
                        position.AssetClass,
                        position.Provider,
                        closingDirection,
                        quantity,
                        fillPrice,
                        realizedProfitLoss,
                        occurredAt));

                    await _unitOfWorkManager.Current.SaveChangesAsync();
                    await _eventBus.TriggerAsync(new TradeExecutedEventData
                    {
                        TenantId = position.TenantId,
                        TradeId = order.Id,
                        UserId = position.UserId,
                        Symbol = position.Symbol,
                        Provider = position.Provider,
                        Direction = closingDirection,
                        Quantity = quantity,
                        ExecutionPrice = fillPrice,
                        Source = trigger == PaperPositionRiskTrigger.StopLoss ? "Paper stop loss" : "Paper take profit",
                        Status = TradeStatus.Closed,
                        RealizedProfitLoss = realizedProfitLoss,
                        OccurredAt = occurredAt
                    });

                    _logger.LogInformation(
                        "Auto-closed paper position {PositionId} for user {UserId} on {Trigger} at {Price}.",
                        position.Id,
                        position.UserId,
                        trigger,
                        fillPrice);
                }

                foreach (var accountId in touchedAccountIds)
                {
                    var account = await GetAccountAsync(accountId, accountCache);
                    if (account == null)
                    {
                        continue;
                    }

                    var openPositions = await _paperPositionRepository.GetOpenPositionsAsync(accountId);
                    account.ApplyMarkToMarket(openPositions.Sum(x => x.UnrealizedProfitLoss), occurredAt);
                }

                await _unitOfWorkManager.Current.SaveChangesAsync();
            }
        }

        private async Task<List<PaperPosition>> GetOpenPositionsForMarketAsync(string symbol, MarketDataProvider provider)
        {
            var positions = await _paperPositionRepository.GetOpenByMarketAsync(symbol, provider) ?? new List<PaperPosition>();
            var alternateSymbol = GetAlternateMarketSymbol(symbol, provider);
            if (string.IsNullOrWhiteSpace(alternateSymbol))
            {
                return positions;
            }

            var alternatePositions = await _paperPositionRepository.GetOpenByMarketAsync(alternateSymbol, provider) ?? new List<PaperPosition>();
            if (alternatePositions.Count == 0)
            {
                return positions;
            }

            return positions
                .Concat(alternatePositions)
                .GroupBy(x => x.Id)
                .Select(x => x.First())
                .ToList();
        }

        private async Task<PaperTradingAccount> GetAccountAsync(
            long accountId,
            Dictionary<long, PaperTradingAccount> accountCache)
        {
            if (accountCache.TryGetValue(accountId, out var account))
            {
                return account;
            }

            account = await _paperTradingAccountRepository.FirstOrDefaultAsync(accountId);
            accountCache[accountId] = account;
            return account;
        }

        private static decimal ResolveExitPrice(
            PaperPosition position,
            decimal marketPrice,
            decimal? bid,
            decimal? ask)
        {
            if (position.Direction == TradeDirection.Buy)
            {
                return bid ?? marketPrice;
            }

            return ask ?? marketPrice;
        }

        private static string BuildExitNotes(PaperPositionRiskTrigger trigger)
        {
            return trigger == PaperPositionRiskTrigger.StopLoss
                ? "Auto-closed by the paper stop-loss engine."
                : "Auto-closed by the paper take-profit engine.";
        }

        private static string GetAlternateMarketSymbol(string symbol, MarketDataProvider provider)
        {
            if (provider != MarketDataProvider.Binance || string.IsNullOrWhiteSpace(symbol))
            {
                return null;
            }

            var normalized = symbol.Trim().ToUpperInvariant().Replace("/", string.Empty, StringComparison.Ordinal);
            if (normalized.EndsWith("USDT", StringComparison.Ordinal))
            {
                return normalized.Substring(0, normalized.Length - 4) + "USD";
            }

            if (normalized.EndsWith("USD", StringComparison.Ordinal))
            {
                return normalized.Substring(0, normalized.Length - 3) + "USDT";
            }

            return null;
        }
    }
}
