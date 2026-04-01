using Abp.Domain.Uow;
using Abp.Events.Bus;
using Fintex.Investments;
using Fintex.Investments.Events;
using Fintex.Investments.PaperTrading;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Fintex.Tests.PaperTrading;

public class PaperPositionRiskService_Tests
{
    [Fact]
    public async Task Should_Close_Long_Position_When_Take_Profit_Is_Hit()
    {
        var account = new PaperTradingAccount(1, 42, "Paper", "USD", 1000m);
        account.Id = 100;
        var position = new PaperPosition(
            1,
            42,
            account.Id,
            "BTCUSDT",
            AssetClass.Crypto,
            MarketDataProvider.Binance,
            TradeDirection.Buy,
            2m,
            100m,
            90m,
            110m,
            DateTime.UtcNow.AddMinutes(-5));

        var paperPositionRepository = Substitute.For<IPaperPositionRepository>();
        paperPositionRepository.GetOpenByMarketAsync("BTCUSDT", MarketDataProvider.Binance)
            .Returns(Task.FromResult(new List<PaperPosition> { position }));
        paperPositionRepository.GetOpenPositionsAsync(account.Id)
            .Returns(_ => Task.FromResult(position.Status == PaperPositionStatus.Open
                ? new List<PaperPosition> { position }
                : new List<PaperPosition>()));

        var accountRepository = Substitute.For<IPaperTradingAccountRepository>();
        accountRepository.FirstOrDefaultAsync(account.Id).Returns(Task.FromResult(account));

        var orderRepository = Substitute.For<IPaperOrderRepository>();
        var fillRepository = Substitute.For<IPaperTradeFillRepository>();
        var eventBus = Substitute.For<IEventBus>();
        var currentUnitOfWork = Substitute.For<IActiveUnitOfWork>();
        currentUnitOfWork.DisableFilter(Arg.Any<string[]>()).Returns(Substitute.For<IDisposable>());
        currentUnitOfWork.SaveChangesAsync().Returns(Task.CompletedTask);

        var unitOfWorkManager = Substitute.For<IUnitOfWorkManager>();
        unitOfWorkManager.Current.Returns(currentUnitOfWork);

        var service = new PaperPositionRiskService(
            paperPositionRepository,
            accountRepository,
            orderRepository,
            fillRepository,
            eventBus,
            unitOfWorkManager,
            Substitute.For<ILogger<PaperPositionRiskService>>());

        await service.EvaluateAsync(new MarketDataUpdatedEventData
        {
            Symbol = "BTCUSDT",
            Provider = MarketDataProvider.Binance,
            Price = 111m,
            Bid = 110.8m,
            Ask = 111.2m,
            Timestamp = DateTime.UtcNow
        }, CancellationToken.None);

        position.Status.ShouldBe(PaperPositionStatus.Closed);
        position.Quantity.ShouldBe(0m);
        account.CashBalance.ShouldBe(1021.6m);
        account.Equity.ShouldBe(1021.6m);

        await orderRepository.Received(1).InsertAsync(Arg.Any<PaperOrder>());
        await fillRepository.Received(1).InsertAsync(Arg.Is<PaperTradeFill>(x =>
            x.Symbol == "BTCUSDT" &&
            x.Direction == TradeDirection.Sell &&
            x.RealizedProfitLoss == 21.6m));
        await eventBus.Received(1).TriggerAsync(Arg.Is<TradeExecutedEventData>(x =>
            x.Symbol == "BTCUSDT" &&
            x.Direction == TradeDirection.Sell &&
            x.Status == TradeStatus.Closed &&
            x.RealizedProfitLoss == 21.6m));
    }

    [Fact]
    public async Task Should_Refresh_Open_Position_Without_Closing_When_Risk_Levels_Are_Not_Hit()
    {
        var account = new PaperTradingAccount(1, 42, "Paper", "USD", 1000m);
        account.Id = 100;
        var position = new PaperPosition(
            1,
            42,
            account.Id,
            "BTCUSDT",
            AssetClass.Crypto,
            MarketDataProvider.Binance,
            TradeDirection.Buy,
            2m,
            100m,
            90m,
            120m,
            DateTime.UtcNow.AddMinutes(-5));

        var paperPositionRepository = Substitute.For<IPaperPositionRepository>();
        paperPositionRepository.GetOpenByMarketAsync("BTCUSDT", MarketDataProvider.Binance)
            .Returns(Task.FromResult(new List<PaperPosition> { position }));
        paperPositionRepository.GetOpenPositionsAsync(account.Id)
            .Returns(_ => Task.FromResult(position.Status == PaperPositionStatus.Open
                ? new List<PaperPosition> { position }
                : new List<PaperPosition>()));

        var accountRepository = Substitute.For<IPaperTradingAccountRepository>();
        accountRepository.FirstOrDefaultAsync(account.Id).Returns(Task.FromResult(account));

        var orderRepository = Substitute.For<IPaperOrderRepository>();
        var fillRepository = Substitute.For<IPaperTradeFillRepository>();
        var eventBus = Substitute.For<IEventBus>();
        var currentUnitOfWork = Substitute.For<IActiveUnitOfWork>();
        currentUnitOfWork.DisableFilter(Arg.Any<string[]>()).Returns(Substitute.For<IDisposable>());
        currentUnitOfWork.SaveChangesAsync().Returns(Task.CompletedTask);

        var unitOfWorkManager = Substitute.For<IUnitOfWorkManager>();
        unitOfWorkManager.Current.Returns(currentUnitOfWork);

        var service = new PaperPositionRiskService(
            paperPositionRepository,
            accountRepository,
            orderRepository,
            fillRepository,
            eventBus,
            unitOfWorkManager,
            Substitute.For<ILogger<PaperPositionRiskService>>());

        await service.EvaluateAsync(new MarketDataUpdatedEventData
        {
            Symbol = "BTCUSDT",
            Provider = MarketDataProvider.Binance,
            Price = 105m,
            Bid = 104.8m,
            Ask = 105.2m,
            Timestamp = DateTime.UtcNow
        }, CancellationToken.None);

        position.Status.ShouldBe(PaperPositionStatus.Open);
        position.CurrentMarketPrice.ShouldBe(105m);
        position.UnrealizedProfitLoss.ShouldBe(10m);
        account.CashBalance.ShouldBe(1000m);
        account.Equity.ShouldBe(1010m);

        await orderRepository.DidNotReceive().InsertAsync(Arg.Any<PaperOrder>());
        await fillRepository.DidNotReceive().InsertAsync(Arg.Any<PaperTradeFill>());
        await eventBus.DidNotReceive().TriggerAsync(Arg.Any<TradeExecutedEventData>());
    }
}
