using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Events.Bus;
using Fintex.Authorization.Users;
using Fintex.Investments;
using Fintex.Investments.Notifications;
using NSubstitute;
using Shouldly;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Fintex.Tests.Notifications;

public class NotificationDispatchService_Tests
{
    [Fact]
    public async Task Should_Send_Branded_Email_And_Trigger_InApp_Event()
    {
        var notificationRepository = Substitute.For<INotificationItemRepository>();
        var userRepository = Substitute.For<IRepository<User, long>>();
        var emailSender = Substitute.For<INotificationEmailSender>();
        var eventBus = Substitute.For<IEventBus>();
        var unitOfWorkManager = Substitute.For<IUnitOfWorkManager>();
        var currentUnitOfWork = Substitute.For<IActiveUnitOfWork>();
        var filterHandle = Substitute.For<IDisposable>();

        unitOfWorkManager.Current.Returns(currentUnitOfWork);
        currentUnitOfWork.DisableFilter(Arg.Any<string[]>()).Returns(filterHandle);
        currentUnitOfWork.SaveChangesAsync().Returns(Task.CompletedTask);

        userRepository.FirstOrDefaultAsync(42).Returns(new User
        {
            Id = 42,
            Name = "Fintex",
            Surname = "User",
            EmailAddress = "user@fintex.dev"
        });

        string sentHtml = null;
        emailSender
            .SendAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Do<string>(html => sentHtml = html))
            .Returns(Task.CompletedTask);

        var service = new NotificationDispatchService(
            notificationRepository,
            userRepository,
            emailSender,
            eventBus,
            unitOfWorkManager);

        var occurredAt = new DateTime(2026, 4, 2, 10, 30, 0, DateTimeKind.Utc);
        var notification = await service.DispatchAsync(new NotificationDispatchRequest
        {
            TenantId = 1,
            UserId = 42,
            Type = NotificationType.TradeOpportunity,
            Severity = NotificationSeverity.Warning,
            Title = "<Potential setup>",
            Message = "Price crossed your threshold.",
            Symbol = "BTCUSDT",
            Provider = MarketDataProvider.Binance,
            ReferencePrice = 67850.32m,
            TargetPrice = 67900m,
            ConfidenceScore = 84.7m,
            Verdict = MarketVerdict.Buy,
            TriggerKey = "test:dispatch",
            NotifyEmail = true,
            NotifyInApp = true,
            ContextJson = "{}",
            OccurredAt = occurredAt
        });

        await notificationRepository.Received(1).InsertAsync(Arg.Any<NotificationItem>());
        await emailSender.Received(1).SendAsync(
            "Fintex",
            "user@fintex.dev",
            "<Potential setup>",
            Arg.Any<string>());
        await eventBus.Received(1).TriggerAsync(Arg.Is<NotificationCreatedEventData>(item =>
            item.UserId == 42 &&
            item.Symbol == "BTCUSDT"));

        sentHtml.ShouldNotBeNullOrWhiteSpace();
        sentHtml.ShouldContain("Fintex Alerts");
        sentHtml.ShouldContain("&lt;Potential setup&gt;");
        sentHtml.ShouldContain("April 2, 2026");
        sentHtml.ShouldContain("UTC");

        notification.EmailSent.ShouldBeTrue();
        notification.InAppDelivered.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_Skip_Email_When_User_Has_No_Email_Address()
    {
        var notificationRepository = Substitute.For<INotificationItemRepository>();
        var userRepository = Substitute.For<IRepository<User, long>>();
        var emailSender = Substitute.For<INotificationEmailSender>();
        var eventBus = Substitute.For<IEventBus>();
        var unitOfWorkManager = Substitute.For<IUnitOfWorkManager>();
        var currentUnitOfWork = Substitute.For<IActiveUnitOfWork>();
        var filterHandle = Substitute.For<IDisposable>();

        unitOfWorkManager.Current.Returns(currentUnitOfWork);
        currentUnitOfWork.DisableFilter(Arg.Any<string[]>()).Returns(filterHandle);
        currentUnitOfWork.SaveChangesAsync().Returns(Task.CompletedTask);

        userRepository.FirstOrDefaultAsync(99).Returns(new User
        {
            Id = 99,
            Name = "No",
            Surname = "Email",
            EmailAddress = "   "
        });

        var service = new NotificationDispatchService(
            notificationRepository,
            userRepository,
            emailSender,
            eventBus,
            unitOfWorkManager);

        var notification = await service.DispatchAsync(new NotificationDispatchRequest
        {
            TenantId = 1,
            UserId = 99,
            Type = NotificationType.TradeAutomation,
            Severity = NotificationSeverity.Info,
            Title = "Automation update",
            Message = "Rule evaluated.",
            Symbol = "BTCUSDT",
            Provider = MarketDataProvider.Binance,
            TriggerKey = "test:no-email",
            NotifyEmail = true,
            NotifyInApp = false,
            OccurredAt = DateTime.UtcNow
        });

        await emailSender.DidNotReceive().SendAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>());

        notification.EmailSent.ShouldBeFalse();
    }
}
