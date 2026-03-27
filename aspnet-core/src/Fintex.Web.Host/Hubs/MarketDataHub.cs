using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Fintex.Web.Host.Hubs
{
    /// <summary>
    /// SignalR hub used by clients to subscribe to live symbols and trade risk updates.
    /// </summary>
    [Authorize]
    public class MarketDataHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            foreach (var userId in ResolveUserGroupIds())
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, BuildUserGroup(userId));
            }

            await base.OnConnectedAsync();
        }

        public Task SubscribeSymbol(string symbol)
        {
            return Groups.AddToGroupAsync(Context.ConnectionId, BuildSymbolGroup(symbol));
        }

        public Task UnsubscribeSymbol(string symbol)
        {
            return Groups.RemoveFromGroupAsync(Context.ConnectionId, BuildSymbolGroup(symbol));
        }

        public Task SubscribeTrade(long tradeId)
        {
            return Groups.AddToGroupAsync(Context.ConnectionId, BuildTradeGroup(tradeId));
        }

        public Task UnsubscribeTrade(long tradeId)
        {
            return Groups.RemoveFromGroupAsync(Context.ConnectionId, BuildTradeGroup(tradeId));
        }

        public static string BuildSymbolGroup(string symbol)
        {
            return "symbol:" + (symbol ?? string.Empty).Trim().ToUpperInvariant();
        }

        public static string BuildTradeGroup(long tradeId)
        {
            return "trade:" + tradeId;
        }

        public static string BuildUserGroup(string userId)
        {
            return "user:" + userId;
        }

        private IEnumerable<string> ResolveUserGroupIds()
        {
            var candidateIds = new[]
            {
                Context.UserIdentifier,
                Context.User?.FindFirstValue(ClaimTypes.NameIdentifier),
                Context.User?.FindFirstValue("sub"),
            };

            return candidateIds
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct();
        }
    }
}
