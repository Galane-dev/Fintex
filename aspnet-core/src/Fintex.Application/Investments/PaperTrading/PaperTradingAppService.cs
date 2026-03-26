using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Uow;
using Abp.Runtime.Session;
using Abp.UI;
using Fintex.Investments.MarketData;
using Fintex.Investments.MarketData.Dto;
using Fintex.Investments.PaperTrading.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fintex.Investments.PaperTrading
{
    /// <summary>
    /// Internal paper trading execution service built on top of live market prices.
    /// </summary>
    [AbpAuthorize]
    public class PaperTradingAppService : FintexAppServiceBase, IPaperTradingAppService
    {
        private readonly IPaperTradingAccountRepository _paperTradingAccountRepository;
        private readonly IPaperOrderRepository _paperOrderRepository;
        private readonly IPaperPositionRepository _paperPositionRepository;
        private readonly IPaperTradeFillRepository _paperTradeFillRepository;
        private readonly IMarketDataPointRepository _marketDataPointRepository;
        private readonly IMarketDataAppService _marketDataAppService;

        public PaperTradingAppService(
            IPaperTradingAccountRepository paperTradingAccountRepository,
            IPaperOrderRepository paperOrderRepository,
            IPaperPositionRepository paperPositionRepository,
            IPaperTradeFillRepository paperTradeFillRepository,
            IMarketDataPointRepository marketDataPointRepository,
            IMarketDataAppService marketDataAppService)
        {
            _paperTradingAccountRepository = paperTradingAccountRepository;
            _paperOrderRepository = paperOrderRepository;
            _paperPositionRepository = paperPositionRepository;
            _paperTradeFillRepository = paperTradeFillRepository;
            _marketDataPointRepository = marketDataPointRepository;
            _marketDataAppService = marketDataAppService;
        }

        public async Task<PaperTradingAccountDto> CreateMyAccountAsync(CreatePaperTradingAccountInput input)
        {
            var userId = AbpSession.GetUserId();
            var existing = await _paperTradingAccountRepository.GetActiveForUserAsync(userId);
            if (existing != null)
            {
                throw new UserFriendlyException("You already have an active paper trading account.");
            }

            var account = new PaperTradingAccount(
                AbpSession.TenantId,
                userId,
                input.Name,
                input.BaseCurrency,
                input.StartingBalance);

            await _paperTradingAccountRepository.InsertAsync(account);
            await CurrentUnitOfWork.SaveChangesAsync();

            return ObjectMapper.Map<PaperTradingAccountDto>(account);
        }

        public async Task<PaperTradingSnapshotDto> GetMySnapshotAsync()
        {
            var userId = AbpSession.GetUserId();
            var account = await _paperTradingAccountRepository.GetActiveForUserAsync(userId);
            if (account == null)
            {
                return new PaperTradingSnapshotDto();
            }

            var positions = await _paperPositionRepository.GetOpenPositionsAsync(account.Id);

            await MarkAccountToMarketAsync(account, positions, DateTime.UtcNow);
            await CurrentUnitOfWork.SaveChangesAsync();

            var orders = await _paperOrderRepository.GetUserOrdersAsync(userId, account.Id);
            var fills = await _paperTradeFillRepository.GetAccountFillsAsync(account.Id);

            return new PaperTradingSnapshotDto
            {
                Account = ObjectMapper.Map<PaperTradingAccountDto>(account),
                Positions = ObjectMapper.Map<List<PaperPositionDto>>(positions),
                RecentOrders = ObjectMapper.Map<List<PaperOrderDto>>(orders.Take(20).ToList()),
                RecentFills = ObjectMapper.Map<List<PaperTradeFillDto>>(fills.Take(20).ToList())
            };
        }

        public async Task<ListResultDto<PaperOrderDto>> GetMyOrdersAsync()
        {
            var userId = AbpSession.GetUserId();
            var account = await _paperTradingAccountRepository.GetActiveForUserAsync(userId);
            if (account == null)
            {
                return new ListResultDto<PaperOrderDto>(new List<PaperOrderDto>());
            }

            var orders = await _paperOrderRepository.GetUserOrdersAsync(userId, account.Id);
            return new ListResultDto<PaperOrderDto>(ObjectMapper.Map<List<PaperOrderDto>>(orders));
        }

        public async Task<ListResultDto<PaperPositionDto>> GetMyPositionsAsync()
        {
            var userId = AbpSession.GetUserId();
            var account = await _paperTradingAccountRepository.GetActiveForUserAsync(userId);
            if (account == null)
            {
                return new ListResultDto<PaperPositionDto>(new List<PaperPositionDto>());
            }

            var positions = await _paperPositionRepository.GetOpenPositionsAsync(account.Id);

            await MarkAccountToMarketAsync(account, positions, DateTime.UtcNow);
            await CurrentUnitOfWork.SaveChangesAsync();

            return new ListResultDto<PaperPositionDto>(ObjectMapper.Map<List<PaperPositionDto>>(positions));
        }

        public async Task<ListResultDto<PaperTradeFillDto>> GetMyFillsAsync()
        {
            var userId = AbpSession.GetUserId();
            var account = await _paperTradingAccountRepository.GetActiveForUserAsync(userId);
            if (account == null)
            {
                return new ListResultDto<PaperTradeFillDto>(new List<PaperTradeFillDto>());
            }

            var fills = await _paperTradeFillRepository.GetAccountFillsAsync(account.Id);
            return new ListResultDto<PaperTradeFillDto>(ObjectMapper.Map<List<PaperTradeFillDto>>(fills));
        }

        public async Task<PaperTradeRecommendationDto> GetRecommendationAsync(GetPaperTradeRecommendationInput input)
        {
            var userId = AbpSession.GetUserId();
            var account = await _paperTradingAccountRepository.GetActiveForUserAsync(userId);
            var marketContext = await GetMarketContextAsync(input.Symbol, input.Provider);
            var suggestedPlan = BuildSuggestedTradePlan(
                marketContext.LatestPoint.Price,
                marketContext.RealtimeVerdict?.Atr,
                marketContext.RealtimeVerdict?.Verdict == MarketVerdict.Sell ? TradeDirection.Sell : TradeDirection.Buy);

            if (marketContext.RealtimeVerdict == null ||
                marketContext.RealtimeVerdict.Verdict == MarketVerdict.Hold ||
                !marketContext.RealtimeVerdict.ConfidenceScore.HasValue ||
                marketContext.RealtimeVerdict.ConfidenceScore.Value < 45m ||
                !marketContext.RealtimeVerdict.TrendScore.HasValue ||
                Math.Abs(marketContext.RealtimeVerdict.TrendScore.Value) < 15m)
            {
                return new PaperTradeRecommendationDto
                {
                    RecommendedAction = MarketVerdict.Hold,
                    RiskScore = 82m,
                    RiskLevel = PaperTradeRiskLevel.High,
                    Headline = "Best move is to wait for a cleaner setup.",
                    Summary = "The market-only read is not aligned strongly enough yet, so forcing a paper trade here would be closer to a gamble than a structured setup.",
                    ReferencePrice = marketContext.LatestPoint.Price,
                    Spread = marketContext.Spread,
                    SpreadPercent = marketContext.SpreadPercent,
                    SuggestedStopLoss = null,
                    SuggestedTakeProfit = null,
                    ConfidenceScore = marketContext.RealtimeVerdict?.ConfidenceScore,
                    TrendScore = marketContext.RealtimeVerdict?.TrendScore,
                    Reasons = BuildHoldReasons(marketContext.RealtimeVerdict),
                    Suggestions = new List<string>
                    {
                        "Wait for 5m, 15m, and 1h alignment to strengthen before entering.",
                        "Look for ADX to firm up and for structure to break more decisively in one direction.",
                        "Only size in once stop loss and take profit levels are defined before the click."
                    }
                };
            }

            var recommendedDirection = marketContext.RealtimeVerdict.Verdict == MarketVerdict.Buy
                ? TradeDirection.Buy
                : TradeDirection.Sell;

            var assessment = BuildTradeAssessment(
                account,
                marketContext,
                recommendedDirection,
                input.Quantity,
                input.StopLoss,
                input.TakeProfit);

            return new PaperTradeRecommendationDto
            {
                RecommendedAction = marketContext.RealtimeVerdict.Verdict,
                RiskScore = assessment.RiskScore,
                RiskLevel = assessment.RiskLevel,
                Headline = marketContext.RealtimeVerdict.Verdict == MarketVerdict.Buy
                    ? "Current edge favors a buy setup."
                    : "Current edge favors a sell setup.",
                Summary = assessment.RiskLevel == PaperTradeRiskLevel.High
                    ? "The market leans in one direction, but your current trade plan still needs work before it becomes disciplined."
                    : "This is the cleaner side of the market right now, provided you keep the setup disciplined.",
                ReferencePrice = marketContext.LatestPoint.Price,
                Spread = marketContext.Spread,
                SpreadPercent = marketContext.SpreadPercent,
                SuggestedStopLoss = assessment.SuggestedStopLoss ?? suggestedPlan.StopLoss,
                SuggestedTakeProfit = assessment.SuggestedTakeProfit ?? suggestedPlan.TakeProfit,
                ConfidenceScore = assessment.ConfidenceScore,
                TrendScore = assessment.TrendScore,
                Reasons = assessment.Reasons,
                Suggestions = assessment.Suggestions
            };
        }

        public async Task<PaperTradeExecutionResultDto> PlaceMarketOrderAsync(PlacePaperOrderInput input)
        {
            var userId = AbpSession.GetUserId();
            var account = await GetMyAccountOrThrowAsync(userId);
            var marketContext = await GetMarketContextAsync(input.Symbol, input.Provider);
            var assessment = BuildTradeAssessment(
                account,
                marketContext,
                input.Direction,
                input.Quantity,
                input.StopLoss,
                input.TakeProfit);

            if (assessment.ShouldBlock)
            {
                return new PaperTradeExecutionResultDto
                {
                    WasExecuted = false,
                    Assessment = assessment,
                    Order = null
                };
            }

            var occurredAt = DateTime.UtcNow;
            var fillPrice = marketContext.LatestPoint.Price;
            var effectiveStopLoss = input.StopLoss ?? assessment.SuggestedStopLoss;
            var effectiveTakeProfit = input.TakeProfit ?? assessment.SuggestedTakeProfit;

            var order = new PaperOrder(
                AbpSession.TenantId,
                userId,
                account.Id,
                input.Symbol,
                input.AssetClass,
                input.Provider,
                input.Direction,
                PaperOrderType.Market,
                input.Quantity,
                fillPrice,
                effectiveStopLoss,
                effectiveTakeProfit,
                input.Notes,
                occurredAt);

            await _paperOrderRepository.InsertAsync(order);
            await CurrentUnitOfWork.SaveChangesAsync();

            var position = await _paperPositionRepository.GetOpenBySymbolAsync(account.Id, input.Symbol, input.Provider);
            var realizedProfitLoss = 0m;
            long? positionId;

            if (position == null)
            {
                position = new PaperPosition(
                    AbpSession.TenantId,
                    userId,
                    account.Id,
                    input.Symbol,
                    input.AssetClass,
                    input.Provider,
                    input.Direction,
                    input.Quantity,
                    fillPrice,
                    effectiveStopLoss,
                    effectiveTakeProfit,
                    occurredAt);

                await _paperPositionRepository.InsertAsync(position);
                await CurrentUnitOfWork.SaveChangesAsync();
                positionId = position.Id;
            }
            else if (position.Direction == input.Direction)
            {
                position.Add(input.Quantity, fillPrice, occurredAt);
                position.ApplyTradePlan(effectiveStopLoss, effectiveTakeProfit, occurredAt);
                positionId = position.Id;
            }
            else
            {
                var closingQuantity = decimal.Min(position.Quantity, input.Quantity);
                realizedProfitLoss = position.Reduce(closingQuantity, fillPrice, occurredAt);
                account.ApplyRealizedProfitLoss(realizedProfitLoss, occurredAt);

                var remainingQuantity = input.Quantity - closingQuantity;
                if (remainingQuantity > 0m)
                {
                    position = new PaperPosition(
                        AbpSession.TenantId,
                        userId,
                        account.Id,
                        input.Symbol,
                        input.AssetClass,
                        input.Provider,
                        input.Direction,
                        remainingQuantity,
                        fillPrice,
                        effectiveStopLoss,
                        effectiveTakeProfit,
                        occurredAt);

                    await _paperPositionRepository.InsertAsync(position);
                    await CurrentUnitOfWork.SaveChangesAsync();
                }

                positionId = position.Id;
            }

            order.MarkFilled(fillPrice, occurredAt, positionId);
            await _paperTradeFillRepository.InsertAsync(new PaperTradeFill(
                AbpSession.TenantId,
                userId,
                account.Id,
                order.Id,
                positionId,
                input.Symbol,
                input.AssetClass,
                input.Provider,
                input.Direction,
                input.Quantity,
                fillPrice,
                realizedProfitLoss,
                occurredAt));

            var openPositions = await _paperPositionRepository.GetOpenPositionsAsync(account.Id);
            await MarkAccountToMarketAsync(account, openPositions, occurredAt);
            await CurrentUnitOfWork.SaveChangesAsync();

            return new PaperTradeExecutionResultDto
            {
                WasExecuted = true,
                Assessment = assessment,
                Order = ObjectMapper.Map<PaperOrderDto>(order)
            };
        }

        public async Task<PaperOrderDto> ClosePositionAsync(ClosePaperPositionInput input)
        {
            var userId = AbpSession.GetUserId();
            var account = await GetMyAccountOrThrowAsync(userId);
            var position = await _paperPositionRepository.FirstOrDefaultAsync(input.PositionId);

            if (position == null || position.UserId != userId || position.AccountId != account.Id)
            {
                throw new UserFriendlyException("Position not found.");
            }

            if (position.Status != PaperPositionStatus.Open)
            {
                throw new UserFriendlyException("Only open positions can be closed.");
            }

            var quantity = input.Quantity ?? position.Quantity;
            if (quantity <= 0m || quantity > position.Quantity)
            {
                throw new UserFriendlyException("Close quantity must be greater than zero and not exceed the open position size.");
            }

            var occurredAt = DateTime.UtcNow;
            var fillPrice = input.ExitPrice ?? await ResolveLatestPriceAsync(position.Symbol, position.Provider);
            var closingDirection = position.Direction == TradeDirection.Buy
                ? TradeDirection.Sell
                : TradeDirection.Buy;

            var order = new PaperOrder(
                AbpSession.TenantId,
                userId,
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
                "Position close",
                occurredAt);

            await _paperOrderRepository.InsertAsync(order);
            await CurrentUnitOfWork.SaveChangesAsync();

            var realizedProfitLoss = position.Reduce(quantity, fillPrice, occurredAt);
            account.ApplyRealizedProfitLoss(realizedProfitLoss, occurredAt);

            order.MarkFilled(fillPrice, occurredAt, position.Id);
            await _paperTradeFillRepository.InsertAsync(new PaperTradeFill(
                AbpSession.TenantId,
                userId,
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

            var openPositions = await _paperPositionRepository.GetOpenPositionsAsync(account.Id);
            await MarkAccountToMarketAsync(account, openPositions, occurredAt);
            await CurrentUnitOfWork.SaveChangesAsync();

            return ObjectMapper.Map<PaperOrderDto>(order);
        }

        private async Task<PaperTradingAccount> GetMyAccountOrThrowAsync(long userId)
        {
            var account = await _paperTradingAccountRepository.GetActiveForUserAsync(userId);
            if (account == null)
            {
                throw new UserFriendlyException("Create a paper trading account before placing simulated trades.");
            }

            return account;
        }

        private async Task MarkAccountToMarketAsync(
            PaperTradingAccount account,
            List<PaperPosition> positions,
            DateTime occurredAt)
        {
            decimal totalUnrealized = 0m;

            foreach (var position in positions)
            {
                var marketPrice = await ResolveLatestPriceAsync(position.Symbol, position.Provider);
                position.RefreshMarketPrice(marketPrice, occurredAt);
                totalUnrealized += position.UnrealizedProfitLoss;
            }

            account.ApplyMarkToMarket(totalUnrealized, occurredAt);
        }

        private async Task<PaperTradeMarketContext> GetMarketContextAsync(string symbol, MarketDataProvider provider)
        {
            var latestPoint = await GetLatestPointAsync(symbol, provider);
            if (latestPoint == null)
            {
                throw new UserFriendlyException(
                    $"No live market price is available yet for {symbol?.Trim()?.ToUpperInvariant()} from {provider}.");
            }

            var realtimeVerdict = await _marketDataAppService.GetRealtimeVerdictAsync(new GetMarketDataHistoryInput
            {
                Symbol = symbol,
                Provider = provider,
                Take = 80
            });
            realtimeVerdict ??= BuildFallbackVerdict(latestPoint);

            decimal? spread = null;
            decimal? spreadPercent = null;
            if (latestPoint.Bid.HasValue &&
                latestPoint.Ask.HasValue &&
                latestPoint.Ask.Value >= latestPoint.Bid.Value)
            {
                spread = decimal.Round(latestPoint.Ask.Value - latestPoint.Bid.Value, 4, MidpointRounding.AwayFromZero);
                spreadPercent = latestPoint.Price > 0m
                    ? decimal.Round((spread.Value / latestPoint.Price) * 100m, 4, MidpointRounding.AwayFromZero)
                    : (decimal?)null;
            }

            return new PaperTradeMarketContext
            {
                LatestPoint = latestPoint,
                RealtimeVerdict = realtimeVerdict,
                Spread = spread,
                SpreadPercent = spreadPercent
            };
        }

        private PaperTradeAssessmentDto BuildTradeAssessment(
            PaperTradingAccount account,
            PaperTradeMarketContext marketContext,
            TradeDirection direction,
            decimal? quantity,
            decimal? stopLoss,
            decimal? takeProfit)
        {
            var reasons = new List<string>();
            var suggestions = new List<string>();
            var latestPrice = marketContext.LatestPoint.Price;
            var verdict = marketContext.RealtimeVerdict;
            var suggestedPlan = BuildSuggestedTradePlan(latestPrice, verdict?.Atr, direction);
            var effectiveQuantity = quantity.GetValueOrDefault();
            var normalizedDirection = direction == TradeDirection.Buy ? 1m : -1m;
            var riskScore = 34m;

            if (verdict == null)
            {
                riskScore += 32m;
                AddUnique(reasons, "The market verdict is still loading, so the setup cannot be confirmed yet.");
                AddUnique(suggestions, "Wait for the realtime verdict stack to finish loading before entering.");
            }
            else
            {
                var confidence = verdict.ConfidenceScore ?? 0m;
                var trend = verdict.TrendScore ?? 0m;
                var alignment = verdict.TimeframeAlignmentScore ?? 0m;
                var structureScore = (verdict.StructureScore ?? 0m) / 100m;

                if (confidence < 45m)
                {
                    riskScore += 20m;
                    AddUnique(reasons, "Confidence is still weak, so the market-only read does not have much conviction yet.");
                    AddUnique(suggestions, "Wait for confidence to build before pressing the trade.");
                }
                else if (confidence < 60m)
                {
                    riskScore += 8m;
                    AddUnique(reasons, "Confidence is only moderate, so the setup still needs discipline.");
                }
                else
                {
                    riskScore -= 6m;
                    AddUnique(reasons, "Confidence is healthy enough to support a structured attempt.");
                }

                if (Math.Abs(trend) < 15m)
                {
                    riskScore += 12m;
                    AddUnique(reasons, "Trend score is still shallow, so there is not much directional edge yet.");
                }
                else if ((trend * normalizedDirection) < 0m)
                {
                    riskScore += 30m;
                    AddUnique(reasons, $"This {direction.ToString().ToLowerInvariant()} goes against the current {verdict.Verdict.ToString().ToLowerInvariant()} bias.");
                    AddUnique(suggestions, $"Only {direction.ToString().ToLowerInvariant()} once trend and alignment turn in your favor.");
                }
                else
                {
                    riskScore -= 12m;
                    AddUnique(reasons, $"This {direction.ToString().ToLowerInvariant()} is aligned with the current {verdict.Verdict.ToString().ToLowerInvariant()} bias.");
                }

                if (Math.Abs(alignment) < 10m)
                {
                    riskScore += 9m;
                    AddUnique(reasons, "Higher timeframes are not aligned strongly enough yet.");
                }
                else if ((alignment * normalizedDirection) < 0m)
                {
                    riskScore += 16m;
                    AddUnique(reasons, "5m, 15m, and 1h confirmation are leaning against this trade.");
                }
                else
                {
                    riskScore -= 6m;
                    AddUnique(reasons, "Multi-timeframe confirmation supports the side you want to take.");
                }

                if (verdict.Adx.HasValue)
                {
                    if (verdict.Adx.Value < 15m)
                    {
                        riskScore += 12m;
                        AddUnique(reasons, "ADX is weak, which means the move may not have trend strength behind it.");
                    }
                    else if (verdict.Adx.Value < 25m)
                    {
                        riskScore += 5m;
                        AddUnique(reasons, "ADX is only middling, so trend continuation is not fully convincing.");
                    }
                    else
                    {
                        riskScore -= 4m;
                        AddUnique(reasons, "ADX confirms that the market has usable directional strength.");
                    }
                }

                if (verdict.AtrPercent.HasValue)
                {
                    if (verdict.AtrPercent.Value >= 0.85m)
                    {
                        riskScore += 12m;
                        AddUnique(reasons, "ATR is elevated, so price can whip through weak setups quickly.");
                        AddUnique(suggestions, "Use tighter confirmation and avoid oversized exposure while ATR stays hot.");
                    }
                    else if (verdict.AtrPercent.Value >= 0.60m)
                    {
                        riskScore += 6m;
                        AddUnique(reasons, "ATR is above calm conditions, so execution needs more care.");
                    }
                    else
                    {
                        riskScore -= 2m;
                    }
                }

                if (marketContext.SpreadPercent.HasValue && marketContext.SpreadPercent.Value >= 0.05m)
                {
                    riskScore += 7m;
                    AddUnique(reasons, "Spread is wider than ideal, which raises execution friction.");
                }

                if (verdict.Rsi.HasValue)
                {
                    if (direction == TradeDirection.Buy)
                    {
                        if (verdict.Rsi.Value >= 72m)
                        {
                            riskScore += 16m;
                            AddUnique(reasons, "RSI is already stretched high, so chasing the buy increases reversal risk.");
                        }
                        else if (verdict.Rsi.Value <= 35m)
                        {
                            riskScore -= 5m;
                            AddUnique(reasons, "RSI is compressed enough to support a mean-reversion style buy.");
                        }
                    }
                    else
                    {
                        if (verdict.Rsi.Value <= 28m)
                        {
                            riskScore += 16m;
                            AddUnique(reasons, "RSI is already deeply compressed, so pressing a sell here risks chasing the move late.");
                        }
                        else if (verdict.Rsi.Value >= 65m)
                        {
                            riskScore -= 5m;
                            AddUnique(reasons, "RSI is elevated enough to support a fade or bearish reversal idea.");
                        }
                    }
                }

                if (verdict.MacdHistogram.HasValue)
                {
                    if ((verdict.MacdHistogram.Value * normalizedDirection) < 0m)
                    {
                        riskScore += 10m;
                        AddUnique(reasons, "MACD histogram is pushing against this trade direction.");
                    }
                    else
                    {
                        riskScore -= 4m;
                    }
                }

                if ((structureScore * normalizedDirection) < -0.15m)
                {
                    riskScore += 12m;
                    AddUnique(reasons, $"Market structure currently looks better for the opposite side: {verdict.StructureLabel?.ToLowerInvariant()}.");
                }
                else if ((structureScore * normalizedDirection) > 0.15m)
                {
                    riskScore -= 6m;
                    AddUnique(reasons, $"Structure supports this direction: {verdict.StructureLabel?.ToLowerInvariant()}.");
                }
            }

            if (!stopLoss.HasValue)
            {
                riskScore += 18m;
                AddUnique(reasons, "There is no stop loss on the ticket yet.");
                AddUnique(suggestions, $"Use a stop loss near {suggestedPlan.StopLoss:N2} so the downside stays defined.");
            }
            else if (!IsStopLossValid(direction, latestPrice, stopLoss.Value))
            {
                riskScore += 25m;
                AddUnique(reasons, "The stop loss is on the wrong side of the entry, so the plan is structurally invalid.");
                AddUnique(suggestions, $"Move the stop loss to the correct side of the entry, around {suggestedPlan.StopLoss:N2}.");
            }

            if (!takeProfit.HasValue)
            {
                riskScore += 8m;
                AddUnique(reasons, "There is no take profit on the ticket yet.");
                AddUnique(suggestions, $"Set a take profit near {suggestedPlan.TakeProfit:N2} so the trade has a defined target.");
            }
            else if (!IsTakeProfitValid(direction, latestPrice, takeProfit.Value))
            {
                riskScore += 18m;
                AddUnique(reasons, "The take profit is on the wrong side of the entry.");
                AddUnique(suggestions, $"Move the take profit to the correct side of the entry, around {suggestedPlan.TakeProfit:N2}.");
            }

            var effectiveStopLoss = stopLoss ?? suggestedPlan.StopLoss;
            var effectiveTakeProfit = takeProfit ?? suggestedPlan.TakeProfit;
            var rewardRiskRatio = CalculateRewardRiskRatio(direction, latestPrice, effectiveStopLoss, effectiveTakeProfit);

            if (rewardRiskRatio.HasValue)
            {
                if (rewardRiskRatio.Value < 1m)
                {
                    riskScore += 15m;
                    AddUnique(reasons, "Reward-to-risk is below 1:1, which makes the setup poor even if the direction is right.");
                }
                else if (rewardRiskRatio.Value < 1.5m)
                {
                    riskScore += 7m;
                    AddUnique(reasons, "Reward-to-risk is only modest, so the setup needs strong accuracy to pay well.");
                }
                else if (rewardRiskRatio.Value >= 2m)
                {
                    riskScore -= 6m;
                    AddUnique(reasons, "Reward-to-risk is healthy for a disciplined paper trade.");
                }
            }

            if (account != null && effectiveQuantity > 0m && account.Equity > 0m)
            {
                var notional = effectiveQuantity * latestPrice;
                var exposurePercent = (notional / account.Equity) * 100m;

                if (exposurePercent >= 80m)
                {
                    riskScore += 22m;
                    AddUnique(reasons, "The position size is oversized relative to current equity.");
                    AddUnique(suggestions, "Reduce size so one idea does not dominate the whole paper account.");
                }
                else if (exposurePercent >= 40m)
                {
                    riskScore += 10m;
                    AddUnique(reasons, "The position size is aggressive relative to current equity.");
                }
                else if (exposurePercent <= 15m)
                {
                    riskScore -= 3m;
                }
            }

            riskScore = decimal.Round(Clamp(riskScore, 0m, 100m), 2, MidpointRounding.AwayFromZero);

            var riskLevel = riskScore >= 72m
                ? PaperTradeRiskLevel.High
                : riskScore >= 45m
                    ? PaperTradeRiskLevel.Medium
                    : PaperTradeRiskLevel.Low;

            var shouldBlock = riskLevel == PaperTradeRiskLevel.High;

            return new PaperTradeAssessmentDto
            {
                Direction = direction,
                RiskScore = riskScore,
                RiskLevel = riskLevel,
                ShouldBlock = shouldBlock,
                Headline = shouldBlock
                    ? "Trade blocked: the setup is too risky right now."
                    : riskLevel == PaperTradeRiskLevel.Medium
                        ? "Trade placed, but this one still looks like a gamble."
                        : "Trade placed: this is a cleaner, more disciplined setup.",
                Summary = shouldBlock
                    ? "The current plan breaks too many quality checks, so the simulator stopped the trade before execution."
                    : riskLevel == PaperTradeRiskLevel.Medium
                        ? "The trade was allowed, but the setup still needs better structure or risk definition if you want better odds."
                        : "The market bias, structure, and risk plan are aligned well enough to support the trade. Keep repeating setups like this.",
                ReferencePrice = latestPrice,
                Spread = marketContext.Spread,
                SpreadPercent = marketContext.SpreadPercent,
                SuggestedStopLoss = suggestedPlan.StopLoss,
                SuggestedTakeProfit = suggestedPlan.TakeProfit,
                SuggestedRewardRiskRatio = rewardRiskRatio ?? suggestedPlan.RewardRiskRatio,
                ConfidenceScore = verdict?.ConfidenceScore,
                TrendScore = verdict?.TrendScore,
                TimeframeAlignmentScore = verdict?.TimeframeAlignmentScore,
                StructureLabel = verdict?.StructureLabel,
                MarketVerdict = verdict?.Verdict ?? MarketVerdict.Hold,
                Reasons = reasons.Take(5).ToList(),
                Suggestions = suggestions.Take(5).ToList()
            };
        }

        private static SuggestedTradePlan BuildSuggestedTradePlan(
            decimal entryPrice,
            decimal? atr,
            TradeDirection direction)
        {
            var volatilityDistance = atr.HasValue && atr.Value > 0m
                ? Math.Max(atr.Value * 0.90m, entryPrice * 0.0025m)
                : entryPrice * 0.0035m;
            var rewardDistance = volatilityDistance * 1.80m;

            var stopLoss = direction == TradeDirection.Buy
                ? entryPrice - volatilityDistance
                : entryPrice + volatilityDistance;
            var takeProfit = direction == TradeDirection.Buy
                ? entryPrice + rewardDistance
                : entryPrice - rewardDistance;

            return new SuggestedTradePlan
            {
                StopLoss = decimal.Round(stopLoss, 2, MidpointRounding.AwayFromZero),
                TakeProfit = decimal.Round(takeProfit, 2, MidpointRounding.AwayFromZero),
                RewardRiskRatio = decimal.Round(rewardDistance / Math.Max(volatilityDistance, 0.00000001m), 2, MidpointRounding.AwayFromZero)
            };
        }

        private static decimal? CalculateRewardRiskRatio(
            TradeDirection direction,
            decimal entryPrice,
            decimal? stopLoss,
            decimal? takeProfit)
        {
            if (!stopLoss.HasValue || !takeProfit.HasValue)
            {
                return null;
            }

            decimal riskDistance;
            decimal rewardDistance;

            if (direction == TradeDirection.Buy)
            {
                riskDistance = entryPrice - stopLoss.Value;
                rewardDistance = takeProfit.Value - entryPrice;
            }
            else
            {
                riskDistance = stopLoss.Value - entryPrice;
                rewardDistance = entryPrice - takeProfit.Value;
            }

            if (riskDistance <= 0m || rewardDistance <= 0m)
            {
                return null;
            }

            return decimal.Round(rewardDistance / riskDistance, 2, MidpointRounding.AwayFromZero);
        }

        private static bool IsStopLossValid(TradeDirection direction, decimal entryPrice, decimal stopLoss)
        {
            return direction == TradeDirection.Buy
                ? stopLoss < entryPrice
                : stopLoss > entryPrice;
        }

        private static MarketVerdictDto BuildFallbackVerdict(MarketDataPoint latestPoint)
        {
            if (latestPoint == null)
            {
                return null;
            }

            var hasDirectionalSignal =
                latestPoint.TrendScore.HasValue ||
                latestPoint.ConfidenceScore.HasValue ||
                latestPoint.Verdict != MarketVerdict.Hold;

            if (!hasDirectionalSignal)
            {
                return null;
            }

            return new MarketVerdictDto
            {
                MarketDataPointId = latestPoint.Id,
                Symbol = latestPoint.Symbol,
                Provider = latestPoint.Provider,
                Price = latestPoint.Price,
                TrendScore = latestPoint.TrendScore,
                ConfidenceScore = latestPoint.ConfidenceScore,
                Verdict = latestPoint.Verdict,
                Timestamp = latestPoint.Timestamp,
                Sma = latestPoint.Sma,
                Ema = latestPoint.Ema,
                Rsi = latestPoint.Rsi,
                Macd = latestPoint.Macd,
                MacdSignal = latestPoint.MacdSignal,
                MacdHistogram = latestPoint.MacdHistogram,
                Momentum = latestPoint.Momentum,
                RateOfChange = latestPoint.RateOfChange,
                Atr = null,
                AtrPercent = null,
                Adx = null,
                StructureScore = null,
                StructureLabel = "Waiting",
                TimeframeAlignmentScore = null,
                IndicatorScores = new List<IndicatorScoreDto>(),
                TimeframeSignals = new List<MarketVerdictTimeframeDto>()
            };
        }

        private static bool IsTakeProfitValid(TradeDirection direction, decimal entryPrice, decimal takeProfit)
        {
            return direction == TradeDirection.Buy
                ? takeProfit > entryPrice
                : takeProfit < entryPrice;
        }

        private static List<string> BuildHoldReasons(MarketVerdictDto verdict)
        {
            var reasons = new List<string>();

            if (verdict == null)
            {
                reasons.Add("The realtime verdict stack is still loading.");
                return reasons;
            }

            if (verdict.ConfidenceScore.HasValue && verdict.ConfidenceScore.Value < 45m)
            {
                reasons.Add("Confidence is too low to justify forcing a trade.");
            }

            if (verdict.TrendScore.HasValue && Math.Abs(verdict.TrendScore.Value) < 15m)
            {
                reasons.Add("Trend score is still shallow, so the edge is weak.");
            }

            if (verdict.TimeframeAlignmentScore.HasValue && Math.Abs(verdict.TimeframeAlignmentScore.Value) < 10m)
            {
                reasons.Add("Higher timeframes are not aligned strongly enough.");
            }

            if (reasons.Count == 0)
            {
                reasons.Add("The current market-only read still favors patience over action.");
            }

            return reasons;
        }

        private static void AddUnique(ICollection<string> collection, string value)
        {
            if (string.IsNullOrWhiteSpace(value) || collection.Contains(value))
            {
                return;
            }

            collection.Add(value);
        }

        private async Task<MarketDataPoint> GetLatestPointAsync(string symbol, MarketDataProvider provider)
        {
            MarketDataPoint latestPoint;

            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                latestPoint = await _marketDataPointRepository.GetLatestAsync(symbol, provider);
                if (latestPoint == null)
                {
                    latestPoint = await _marketDataPointRepository.GetLatestBySymbolAsync(symbol);
                }
            }

            return latestPoint;
        }

        private async Task<decimal> ResolveLatestPriceAsync(string symbol, MarketDataProvider provider)
        {
            var latestPoint = await GetLatestPointAsync(symbol, provider);
            if (latestPoint == null)
            {
                throw new UserFriendlyException(
                    $"No live market price is available yet for {symbol?.Trim()?.ToUpperInvariant()} from {provider}.");
            }

            return latestPoint.Price;
        }

        private static decimal Clamp(decimal value, decimal min, decimal max)
        {
            if (value < min)
            {
                return min;
            }

            if (value > max)
            {
                return max;
            }

            return value;
        }

        private sealed class PaperTradeMarketContext
        {
            public MarketDataPoint LatestPoint { get; set; }

            public MarketVerdictDto RealtimeVerdict { get; set; }

            public decimal? Spread { get; set; }

            public decimal? SpreadPercent { get; set; }
        }

        private sealed class SuggestedTradePlan
        {
            public decimal StopLoss { get; set; }

            public decimal TakeProfit { get; set; }

            public decimal RewardRiskRatio { get; set; }
        }
    }
}
