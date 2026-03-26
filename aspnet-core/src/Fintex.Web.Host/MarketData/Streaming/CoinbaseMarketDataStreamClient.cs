using Abp.Dependency;
using Fintex.Investments;
using Fintex.Investments.MarketData;
using Fintex.Web.Host.MarketData.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Globalization;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Fintex.Web.Host.MarketData.Streaming
{
    /// <summary>
    /// Connects to Coinbase Exchange WebSockets and emits normalized ticker updates.
    /// </summary>
    public class CoinbaseMarketDataStreamClient : IMarketDataStreamClient, ITransientDependency
    {
        private readonly IOptions<MarketDataStreamingOptions> _options;
        private readonly ILogger<CoinbaseMarketDataStreamClient> _logger;

        public CoinbaseMarketDataStreamClient(IOptions<MarketDataStreamingOptions> options, ILogger<CoinbaseMarketDataStreamClient> logger)
        {
            _options = options;
            _logger = logger;
        }

        public async Task RunAsync(Func<MarketStreamTick, CancellationToken, Task> onTickAsync, CancellationToken cancellationToken)
        {
            var config = _options.Value.Coinbase;
            if (config == null || !config.Enabled || string.IsNullOrWhiteSpace(config.ProductIds))
            {
                return;
            }

            var streamUrl = string.IsNullOrWhiteSpace(config.StreamUrl)
                ? "wss://ws-feed.exchange.coinbase.com"
                : config.StreamUrl;

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    using (var socket = new ClientWebSocket())
                    {
                        await socket.ConnectAsync(new Uri(streamUrl), cancellationToken);
                        await SendSubscribeMessageAsync(socket, config.ProductIds, cancellationToken);
                        await ReceiveLoopAsync(socket, onTickAsync, cancellationToken);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Coinbase stream disconnected. Reconnecting.");
                    await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                }
            }
        }

        private static async Task SendSubscribeMessageAsync(ClientWebSocket socket, string productIds, CancellationToken cancellationToken)
        {
            var products = productIds.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            var subscribePayload = JsonSerializer.Serialize(new
            {
                type = "subscribe",
                product_ids = products,
                channels = new[] { "ticker" }
            });

            var buffer = Encoding.UTF8.GetBytes(subscribePayload);
            await socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, cancellationToken);
        }

        private static async Task ReceiveLoopAsync(ClientWebSocket socket, Func<MarketStreamTick, CancellationToken, Task> onTickAsync, CancellationToken cancellationToken)
        {
            var buffer = new byte[16 * 1024];

            while (socket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
            {
                var builder = new StringBuilder();
                WebSocketReceiveResult result;

                do
                {
                    result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        return;
                    }

                    builder.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
                }
                while (!result.EndOfMessage);

                using (var document = JsonDocument.Parse(builder.ToString()))
                {
                    if (!document.RootElement.TryGetProperty("type", out var typeElement)
                        || !string.Equals(typeElement.GetString(), "ticker", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var tick = new MarketStreamTick
                    {
                        Provider = MarketDataProvider.Coinbase,
                        AssetClass = AssetClass.Crypto,
                        Symbol = document.RootElement.GetProperty("product_id").GetString(),
                        Price = ParseDecimal(document.RootElement, "price"),
                        Bid = ParseNullableDecimal(document.RootElement, "best_bid"),
                        Ask = ParseNullableDecimal(document.RootElement, "best_ask"),
                        Volume = ParseNullableDecimal(document.RootElement, "volume_24h"),
                        Timestamp = document.RootElement.TryGetProperty("time", out var timeElement)
                            ? DateTime.Parse(timeElement.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal)
                            : DateTime.UtcNow
                    };

                    await onTickAsync(tick, cancellationToken);
                }
            }
        }

        private static decimal ParseDecimal(JsonElement element, string propertyName)
        {
            return decimal.Parse(element.GetProperty(propertyName).GetString(), CultureInfo.InvariantCulture);
        }

        private static decimal? ParseNullableDecimal(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out var property) || string.IsNullOrWhiteSpace(property.GetString()))
            {
                return null;
            }

            return decimal.Parse(property.GetString(), CultureInfo.InvariantCulture);
        }
    }
}
