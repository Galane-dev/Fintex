using Abp.Domain.Uow;
using Fintex.Investments;
using Fintex.Investments.Notifications;
using NSubstitute;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Fintex.Tests.Notifications;

public class NotificationEvaluationService_Tests
{
    [Fact]
    public async Task Should_Return_Immediately_For_Invalid_Snapshot()
    {
        var ruleRepository = Substitute.For<INotificationAlertRuleRepository>();
        var dispatchService = Substitute.For<INotificationDispatchService>();
        var unitOfWorkManager = Substitute.For<IUnitOfWorkManager>();

        var service = new NotificationEvaluationService(ruleRepository, dispatchService, unitOfWorkManager);

        await service.EvaluateAsync(new NotificationMarketSnapshot
        {
            Symbol = string.Empty,
            Provider = MarketDataProvider.Binance,
            Price = 0m
        }, CancellationToken.None);

        await ruleRepository.DidNotReceive().GetActivePriceAlertsAsync(Arg.Any<string>(), Arg.Any<MarketDataProvider>());
        await dispatchService.DidNotReceive().DispatchAsync(Arg.Any<NotificationDispatchRequest>());
    }

    [Fact]
    public async Task Should_Update_Last_Observed_Price_When_Alert_Has_Not_Crossed()
    {
        var rule = new NotificationAlertRule(
            tenantId: 1,
            userId: 7,
            name: "BTC above",
            symbol: "BTCUSDT",
            provider: MarketDataProvider.Binance,
            createdPrice: 100m,
            targetPrice: 110m,
            notifyInApp: true,
            notifyEmail: false,
            notes: null);

        var ruleRepository = Substitute.For<INotificationAlertRuleRepository>();
        var dispatchService = Substitute.For<INotificationDispatchService>();
        var unitOfWorkManager = Substitute.For<IUnitOfWorkManager>();
        var currentUnitOfWork = Substitute.For<IActiveUnitOfWork>();
        var filterHandle = Substitute.For<IDisposable>();

        unitOfWorkManager.Current.Returns(currentUnitOfWork);
        currentUnitOfWork.DisableFilter(Arg.Any<string[]>()).Returns(filterHandle);
        ruleRepository
            .GetActivePriceAlertsAsync("BTCUSDT", MarketDataProvider.Binance)
            .Returns(Task.FromResult(new List<NotificationAlertRule> { rule }));

        var service = new NotificationEvaluationService(ruleRepository, dispatchService, unitOfWorkManager);

        await service.EvaluateAsync(new NotificationMarketSnapshot
        {
            Symbol = "BTCUSDT",
            Provider = MarketDataProvider.Binance,
            Price = 105m
        }, CancellationToken.None);

        rule.LastObservedPrice.ShouldBe(105m);
        await ruleRepository.Received(1).UpdateAsync(rule);
        await dispatchService.DidNotReceive().DispatchAsync(Arg.Any<NotificationDispatchRequest>());
    }

    [Fact]
    public async Task Should_Dispatch_And_Deactivate_Rule_When_Target_Is_Crossed()
    {
        var rule = new NotificationAlertRule(
            tenantId: 1,
            userId: 7,
            name: "BTC above",
            symbol: "BTCUSDT",
            provider: MarketDataProvider.Binance,
            createdPrice: 100m,
            targetPrice: 110m,
            notifyInApp: true,
            notifyEmail: true,
            notes: null);
        rule.Id = 50;

        var ruleRepository = Substitute.For<INotificationAlertRuleRepository>();
        var dispatchService = Substitute.For<INotificationDispatchService>();
        var unitOfWorkManager = Substitute.For<IUnitOfWorkManager>();
        var currentUnitOfWork = Substitute.For<IActiveUnitOfWork>();
        var filterHandle = Substitute.For<IDisposable>();

        unitOfWorkManager.Current.Returns(currentUnitOfWork);
        currentUnitOfWork.DisableFilter(Arg.Any<string[]>()).Returns(filterHandle);
        ruleRepository
            .GetActivePriceAlertsAsync("BTCUSDT", MarketDataProvider.Binance)
            .Returns(Task.FromResult(new List<NotificationAlertRule> { rule }));

        var dispatchedNotification = new NotificationItem(
            tenantId: 1,
            userId: 7,
            type: NotificationType.PriceTarget,
            severity: NotificationSeverity.Warning,
            title: "Alert",
            message: "Triggered",
            symbol: "BTCUSDT",
            provider: MarketDataProvider.Binance,
            referencePrice: 111m,
            targetPrice: 110m,
            confidenceScore: 70m,
            verdict: MarketVerdict.Buy,
            triggerKey: "price:50",
            emailRequested: true,
            contextJson: "{}",
            occurredAt: DateTime.UtcNow);
        dispatchedNotification.Id = 999;

        dispatchService
            .DispatchAsync(Arg.Any<NotificationDispatchRequest>())
            .Returns(Task.FromResult(dispatchedNotification));

        var service = new NotificationEvaluationService(ruleRepository, dispatchService, unitOfWorkManager);

        await service.EvaluateAsync(new NotificationMarketSnapshot
        {
            Symbol = "BTCUSDT",
            Provider = MarketDataProvider.Binance,
            Price = 111m,
            Bid = 110.9m,
            Ask = 111.1m,
            ConfidenceScore = 88m,
            Verdict = MarketVerdict.Buy
        }, CancellationToken.None);

        await dispatchService.Received(1).DispatchAsync(Arg.Is<NotificationDispatchRequest>(request =>
            request.UserId == 7 &&
            request.Symbol == "BTCUSDT" &&
            request.Type == NotificationType.PriceTarget &&
            request.NotifyEmail &&
            request.NotifyInApp));

        rule.IsActive.ShouldBeFalse();
        rule.LastNotificationId.ShouldBe(999);
        await ruleRepository.Received(1).UpdateAsync(rule);
    }
}
