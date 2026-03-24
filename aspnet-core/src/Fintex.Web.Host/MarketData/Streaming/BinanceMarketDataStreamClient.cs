using Abp.Dependency;
using Fintex.Investments;
using Fintex.Investments.MarketData;
using Fintex.Web.Host.MarketData.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Globalization;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Fintex.Web.Host.MarketData.Streaming
{
    /// <summary>
    /// Connects to Binance Spot WebSockets and emits normalized ticker updates.
    /// </summary>
    public class BinanceMarketDataStreamClient : IMarketDataStreamClient, ITransientDependency
    {
        private readonly IOptions<MarketDataStreamingOptions> _options;
        private readonly ILogger<BinanceMarketDataStreamClient> _logger;

        public BinanceMarketDataStreamClient(IOptions<MarketDataStreamingOptions> options, ILogger<BinanceMarketDataStreamClient> logger)
        {
            _options = options;
            _logger = logger;
        }

        public async Task RunAsync(Func<MarketStreamTick, CancellationToken, Task> onTickAsync, CancellationToken cancellationToken)
        {
            var config = _options.Value.Binance;
            if (config == null || !config.Enabled || string.IsNullOrWhiteSpace(config.Symbols))
            {
                return;
            }

            var streamUrl = BuildStreamUrl(config);
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    using (var socket = new ClientWebSocket())
                    {
                        await socket.ConnectAsync(new Uri(streamUrl), cancellationToken);
                        await ReceiveLoopAsync(socket, onTickAsync, cancellationToken);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Binance stream disconnected. Reconnecting.");
                    await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                }
            }
        }

        private static string BuildStreamUrl(BinanceStreamOptions config)
        {
            if (!string.IsNullOrWhiteSpace(config.StreamUrl) && config.StreamUrl.Contains("@ticker"))
            {
                return config.StreamUrl;
            }

            var streams = config.Symbols
                .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim().ToLowerInvariant() + "@ticker");

            var prefix = string.IsNullOrWhiteSpace(config.StreamUrl)
                ? "wss://stream.binance.com:9443/stream?streams="
                : config.StreamUrl;

            return prefix + string.Join("/", streams);
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

                var payload = builder.ToString();
                using (var document = JsonDocument.Parse(payload))
                {
                    if (!document.RootElement.TryGetProperty("data", out var data))
                    {
                        continue;
                    }

                    var tick = new MarketStreamTick
                    {
                        Provider = MarketDataProvider.Binance,
                        AssetClass = AssetClass.Crypto,
                        Symbol = data.GetProperty("s").GetString(),
                        Price = ParseDecimal(data, "c"),
                        Bid = ParseNullableDecimal(data, "b"),
                        Ask = ParseNullableDecimal(data, "a"),
                        Volume = ParseNullableDecimal(data, "v"),
                        Open24Hours = ParseNullableDecimal(data, "o"),
                        High24Hours = ParseNullableDecimal(data, "h"),
                        Low24Hours = ParseNullableDecimal(data, "l"),
                        Timestamp = data.TryGetProperty("E", out var eventTime)
                            ? DateTimeOffset.FromUnixTimeMilliseconds(eventTime.GetInt64()).UtcDateTime
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
