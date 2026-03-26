using Abp.Dependency;
using Fintex.Investments;
using System;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace Fintex.Web.Host.Brokers
{
    /// <summary>
    /// Validates Alpaca API credentials by probing the account endpoint.
    /// </summary>
    public class AlpacaBrokerService : IAlpacaBrokerService, ITransientDependency
    {
        private const string LiveBaseUrl = "https://api.alpaca.markets";
        private const string PaperBaseUrl = "https://paper-api.alpaca.markets";

        private readonly IHttpClientFactory _httpClientFactory;

        public AlpacaBrokerService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<AlpacaConnectionProbeResult> ProbeConnectionAsync(AlpacaConnectionProbeRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ApiKey) || string.IsNullOrWhiteSpace(request.ApiSecret))
            {
                return new AlpacaConnectionProbeResult
                {
                    IsSuccess = false,
                    Error = "Alpaca API key and secret are required."
                };
            }

            var endpoint = request.IsPaperEnvironment ? PaperBaseUrl : LiveBaseUrl;

            try
            {
                using (var client = _httpClientFactory.CreateClient())
                using (var message = new HttpRequestMessage(HttpMethod.Get, endpoint + "/v2/account"))
                {
                    client.Timeout = TimeSpan.FromSeconds(20);
                    message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    message.Headers.Add("APCA-API-KEY-ID", request.ApiKey.Trim());
                    message.Headers.Add("APCA-API-SECRET-KEY", request.ApiSecret.Trim());

                    using (var response = await client.SendAsync(message))
                    {
                        var body = await response.Content.ReadAsStringAsync();

                        if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
                        {
                            return new AlpacaConnectionProbeResult
                            {
                                IsSuccess = false,
                                Error = "Alpaca rejected the API key or secret for this environment."
                            };
                        }

                        if (!response.IsSuccessStatusCode)
                        {
                            return new AlpacaConnectionProbeResult
                            {
                                IsSuccess = false,
                                Error = "Alpaca account validation failed with HTTP " + (int)response.StatusCode + "."
                            };
                        }

                        return ParseSuccessResult(body, endpoint, request.IsPaperEnvironment);
                    }
                }
            }
            catch (TaskCanceledException)
            {
                return new AlpacaConnectionProbeResult
                {
                    IsSuccess = false,
                    Error = "Timed out while validating the Alpaca account."
                };
            }
            catch (Exception exception)
            {
                return new AlpacaConnectionProbeResult
                {
                    IsSuccess = false,
                    Error = "Alpaca connection error: " + exception.Message
                };
            }
        }

        private static AlpacaConnectionProbeResult ParseSuccessResult(string body, string endpoint, bool isPaperEnvironment)
        {
            try
            {
                using (var document = JsonDocument.Parse(body))
                {
                    var root = document.RootElement;
                    var accountNumber = GetString(root, "account_number");
                    var status = GetString(root, "status");
                    var currency = GetString(root, "currency");
                    var equity = GetNullableDecimal(root, "equity") ?? GetNullableDecimal(root, "portfolio_value");
                    var cash = GetNullableDecimal(root, "cash");
                    var multiplier = GetNullableInt(root, "multiplier");

                    return new AlpacaConnectionProbeResult
                    {
                        IsSuccess = true,
                        AccountNumber = string.IsNullOrWhiteSpace(accountNumber) ? "Alpaca account" : accountNumber,
                        AccountStatus = status,
                        Currency = currency,
                        Company = isPaperEnvironment ? "Alpaca Paper" : "Alpaca Live",
                        Multiplier = multiplier,
                        Cash = cash,
                        Equity = equity,
                        Endpoint = endpoint
                    };
                }
            }
            catch (Exception exception)
            {
                return new AlpacaConnectionProbeResult
                {
                    IsSuccess = false,
                    Error = "Alpaca returned an unreadable account response: " + exception.Message
                };
            }
        }

        private static string GetString(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out var property) || property.ValueKind == JsonValueKind.Null)
            {
                return null;
            }

            return property.ToString();
        }

        private static int? GetNullableInt(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out var property))
            {
                return null;
            }

            if (property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var numericValue))
            {
                return numericValue;
            }

            if (property.ValueKind == JsonValueKind.String &&
                int.TryParse(property.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var stringValue))
            {
                return stringValue;
            }

            return null;
        }

        private static decimal? GetNullableDecimal(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out var property))
            {
                return null;
            }

            if (property.ValueKind == JsonValueKind.Number && property.TryGetDecimal(out var decimalValue))
            {
                return decimalValue;
            }

            if (property.ValueKind == JsonValueKind.String &&
                decimal.TryParse(property.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var stringValue))
            {
                return stringValue;
            }

            return null;
        }
    }
}
