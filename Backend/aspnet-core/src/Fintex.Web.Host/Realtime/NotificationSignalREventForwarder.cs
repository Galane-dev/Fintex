using Abp.Dependency;
using Abp.Events.Bus.Handlers;
using Fintex.Investments.Notifications;
using Fintex.Web.Host.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Fintex.Web.Host.Realtime
{
    /// <summary>
    /// Pushes persisted notification events to the signed-in user's SignalR group.
    /// </summary>
    public class NotificationSignalREventForwarder :
        IAsyncEventHandler<NotificationCreatedEventData>,
        ITransientDependency
    {
        private readonly IHubContext<MarketDataHub> _hubContext;

        public NotificationSignalREventForwarder(IHubContext<MarketDataHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task HandleEventAsync(NotificationCreatedEventData eventData)
        {
            var payload = new
            {
                eventData.NotificationId,
                eventData.UserId,
                eventData.Title,
                eventData.Message,
                eventData.Symbol,
                eventData.Severity,
                eventData.Type,
                eventData.ConfidenceScore,
                eventData.OccurredAt
            };

            await _hubContext.Clients
                .Group(MarketDataHub.BuildUserGroup(eventData.UserId.ToString()))
                .SendAsync("notificationCreated", payload);
        }
    }
}
