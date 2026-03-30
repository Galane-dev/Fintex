using Abp.Dependency;
using Fintex.Investments;
using Fintex.Investments.MarketData;
using Fintex.Web.Host.MarketData.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Fintex.Web.Host.MarketData.Streaming
{
    /// <summary>
    /// Connects to the OANDA pricing stream and emits normalized forex ticks.
    /// </summary>
    public class OandaMarketDataStreamClient : IMarketDataStreamClient, ITransientDependency
    {
        private readonly IOptions<MarketDataStreamingOptions> _options;
        private readonly HttpClient _httpClient;
        private readonly ILogger<OandaMarketDataStreamClient> _logger;

        public OandaMarketDataStreamClient(
            IOptions<MarketDataStreamingOptions> options,
            HttpClient httpClient,
            ILogger<OandaMarketDataStreamClient> logger)
        {
            _options = options;
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task RunAsync(Func<MarketStreamTick, CancellationToken, Task> onTickAsync, CancellationToken cancellationToken)
        {
            var config = _options.Value.Oanda;
            if (config == null
                || !config.Enabled
                || string.IsNullOrWhiteSpace(config.PricingStreamUrl)
                || string.IsNullOrWhiteSpace(config.AccountId)
                || string.IsNullOrWhiteSpace(config.ApiToken)
                || string.IsNullOrWhiteSpace(config.Instruments))
            {
                return;
            }

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var requestUri = string.Format(
                        "{0}/accounts/{1}/pricing/stream?instruments={2}",
                        config.PricingStreamUrl.TrimEnd('/'),
                        config.AccountId,
                        Uri.EscapeDataString(config.Instruments));

                    using (var request = new HttpRequestMessage(HttpMethod.Get, requestUri))
                    {
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", config.ApiToken);

                        using (var response = await _httpClient.SendAsync(
                                   request,
                                   HttpCompletionOption.ResponseHeadersRead,
                                   cancellationToken))
                        {
                            response.EnsureSuccessStatusCode();
                            using (var stream = await response.Content.ReadAsStreamAsync(cancellationToken))
                            using (var reader = new StreamReader(stream))
                            {
                                while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
                                {
                                    var line = await reader.ReadLineAsync(cancellationToken);
                                    if (string.IsNullOrWhiteSpace(line))
                                    {
                                        continue;
                                    }

                                    using (var document = JsonDocument.Parse(line))
                                    {
                                        if (!document.RootElement.TryGetProperty("type", out var typeElement)
                                            || !string.Equals(typeElement.GetString(), "PRICE", StringComparison.OrdinalIgnoreCase))
                                        {
                                            continue;
                                        }

                                        decimal? bid = TryParseNestedPrice(document.RootElement, "bids");
                                        decimal? ask = TryParseNestedPrice(document.RootElement, "asks");
                                        var price = bid.HasValue && ask.HasValue
                                            ? decimal.Round((bid.Value + ask.Value) / 2m, 8, MidpointRounding.AwayFromZero)
                                            : bid ?? ask ?? 0m;

                                        if (price <= 0m)
                                        {
                                            continue;
                                        }

                                        var tick = new MarketStreamTick
                                        {
                                            Provider = MarketDataProvider.Oanda,
                                            AssetClass = AssetClass.Forex,
                                            Symbol = document.RootElement.GetProperty("instrument").GetString(),
                                            Price = price,
                                            Bid = bid,
                                            Ask = ask,
                                            Timestamp = document.RootElement.TryGetProperty("time", out var timeElement)
                                                ? DateTime.Parse(timeElement.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal)
                                                : DateTime.UtcNow
                                        };

                                        await onTickAsync(tick, cancellationToken);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "OANDA pricing stream disconnected. Reconnecting.");
                    await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                }
            }
        }

        private static decimal? TryParseNestedPrice(JsonElement root, string propertyName)
        {
            if (!root.TryGetProperty(propertyName, out var collection) || collection.ValueKind != JsonValueKind.Array || collection.GetArrayLength() == 0)
            {
                return null;
            }

            var first = collection.EnumerateArray().FirstOrDefault();
            if (first.ValueKind != JsonValueKind.Object || !first.TryGetProperty("price", out var priceElement))
            {
                return null;
            }

            return decimal.Parse(priceElement.GetString(), CultureInfo.InvariantCulture);
        }
    }
}
