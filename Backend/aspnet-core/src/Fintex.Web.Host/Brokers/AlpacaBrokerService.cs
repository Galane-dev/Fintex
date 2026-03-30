using Abp.Dependency;
using Fintex.Investments;
using System;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Collections.Generic;
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

        public async Task<AlpacaPlaceOrderResult> PlaceMarketOrderAsync(AlpacaPlaceOrderRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ApiKey) || string.IsNullOrWhiteSpace(request.ApiSecret))
            {
                return new AlpacaPlaceOrderResult
                {
                    IsSuccess = false,
                    Error = "Alpaca API key and secret are required."
                };
            }

            if (string.IsNullOrWhiteSpace(request.Symbol))
            {
                return new AlpacaPlaceOrderResult
                {
                    IsSuccess = false,
                    Error = "A broker symbol is required."
                };
            }

            if (request.Quantity <= 0m)
            {
                return new AlpacaPlaceOrderResult
                {
                    IsSuccess = false,
                    Error = "Order quantity must be greater than zero."
                };
            }

            var endpoint = request.IsPaperEnvironment ? PaperBaseUrl : LiveBaseUrl;
            string payloadJson;
            if (request.UseBracketExits && request.StopLoss.HasValue && request.TakeProfit.HasValue)
            {
                payloadJson = JsonSerializer.Serialize(new
                {
                    symbol = request.Symbol.Trim().ToUpperInvariant(),
                    qty = request.Quantity.ToString(CultureInfo.InvariantCulture),
                    side = request.Direction == TradeDirection.Buy ? "buy" : "sell",
                    type = "market",
                    time_in_force = "gtc",
                    order_class = "bracket",
                    client_order_id = string.IsNullOrWhiteSpace(request.ClientOrderId) ? null : request.ClientOrderId.Trim(),
                    take_profit = new
                    {
                        limit_price = request.TakeProfit.Value.ToString(CultureInfo.InvariantCulture)
                    },
                    stop_loss = new
                    {
                        stop_price = request.StopLoss.Value.ToString(CultureInfo.InvariantCulture)
                    }
                });
            }
            else
            {
                payloadJson = JsonSerializer.Serialize(new
                {
                    symbol = request.Symbol.Trim().ToUpperInvariant(),
                    qty = request.Quantity.ToString(CultureInfo.InvariantCulture),
                    side = request.Direction == TradeDirection.Buy ? "buy" : "sell",
                    type = "market",
                    time_in_force = "gtc",
                    client_order_id = string.IsNullOrWhiteSpace(request.ClientOrderId) ? null : request.ClientOrderId.Trim()
                });
            }

            try
            {
                using (var client = _httpClientFactory.CreateClient())
                using (var message = new HttpRequestMessage(HttpMethod.Post, endpoint + "/v2/orders"))
                {
                    client.Timeout = TimeSpan.FromSeconds(20);
                    message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    message.Headers.Add("APCA-API-KEY-ID", request.ApiKey.Trim());
                    message.Headers.Add("APCA-API-SECRET-KEY", request.ApiSecret.Trim());
                    message.Content = new StringContent(payloadJson, Encoding.UTF8, "application/json");

                    using (var response = await client.SendAsync(message))
                    {
                        var body = await response.Content.ReadAsStringAsync();

                        if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
                        {
                            var alpacaError = ExtractAlpacaError(body);
                            return new AlpacaPlaceOrderResult
                            {
                                IsSuccess = false,
                                Error = string.IsNullOrWhiteSpace(alpacaError)
                                    ? "Alpaca rejected the order for this account or environment."
                                    : "Alpaca rejected the order: " + alpacaError,
                                Endpoint = endpoint,
                                ResponseJson = body
                            };
                        }

                        if (!response.IsSuccessStatusCode)
                        {
                            var alpacaError = ExtractAlpacaError(body);
                            return new AlpacaPlaceOrderResult
                            {
                                IsSuccess = false,
                                Error = string.IsNullOrWhiteSpace(alpacaError)
                                    ? "Alpaca order placement failed with HTTP " + (int)response.StatusCode + "."
                                    : "Alpaca order placement failed: " + alpacaError,
                                Endpoint = endpoint,
                                ResponseJson = body
                            };
                        }

                        return ParseOrderResult(body, endpoint);
                    }
                }
            }
            catch (TaskCanceledException)
            {
                return new AlpacaPlaceOrderResult
                {
                    IsSuccess = false,
                    Error = "Timed out while placing the Alpaca order.",
                    Endpoint = endpoint,
                    ResponseJson = payloadJson
                };
            }
            catch (Exception exception)
            {
                return new AlpacaPlaceOrderResult
                {
                    IsSuccess = false,
                    Error = "Alpaca order error: " + exception.Message,
                    Endpoint = endpoint,
                    ResponseJson = payloadJson
                };
            }
        }

        public async Task<List<AlpacaPositionSnapshot>> GetOpenPositionsAsync(AlpacaAccountRequest request)
        {
            var endpoint = request.IsPaperEnvironment ? PaperBaseUrl : LiveBaseUrl;
            var body = await SendJsonAsync(request, endpoint + "/v2/positions");

            using (var document = JsonDocument.Parse(body))
            {
                var items = new List<AlpacaPositionSnapshot>();
                foreach (var element in document.RootElement.EnumerateArray())
                {
                    items.Add(new AlpacaPositionSnapshot
                    {
                        Symbol = GetString(element, "symbol"),
                        Side = GetString(element, "side"),
                        Quantity = GetNullableDecimal(element, "qty") ?? 0m,
                        AverageEntryPrice = GetNullableDecimal(element, "avg_entry_price"),
                        CurrentPrice = GetNullableDecimal(element, "current_price"),
                        MarketValue = GetNullableDecimal(element, "market_value"),
                        UnrealizedProfitLoss = GetNullableDecimal(element, "unrealized_pl"),
                        UnrealizedProfitLossPercent = GetNullableDecimal(element, "unrealized_plpc")
                    });
                }

                return items;
            }
        }

        public async Task<List<AlpacaOrderSnapshot>> GetRecentOrdersAsync(AlpacaAccountRequest request)
        {
            var endpoint = request.IsPaperEnvironment ? PaperBaseUrl : LiveBaseUrl;
            var body = await SendJsonAsync(request, endpoint + "/v2/orders?status=all&direction=desc&limit=100");

            using (var document = JsonDocument.Parse(body))
            {
                var items = new List<AlpacaOrderSnapshot>();
                foreach (var element in document.RootElement.EnumerateArray())
                {
                    items.Add(new AlpacaOrderSnapshot
                    {
                        OrderId = GetString(element, "id"),
                        ClientOrderId = GetString(element, "client_order_id"),
                        Symbol = GetString(element, "symbol"),
                        Side = GetString(element, "side"),
                        Status = GetString(element, "status"),
                        Quantity = GetNullableDecimal(element, "qty"),
                        FilledQuantity = GetNullableDecimal(element, "filled_qty"),
                        FilledAveragePrice = GetNullableDecimal(element, "filled_avg_price"),
                        SubmittedAt = GetNullableDateTimeOffset(element, "submitted_at"),
                        FilledAt = GetNullableDateTimeOffset(element, "filled_at"),
                        RawJson = element.GetRawText()
                    });
                }

                return items;
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

        private static AlpacaPlaceOrderResult ParseOrderResult(string body, string endpoint)
        {
            try
            {
                using (var document = JsonDocument.Parse(body))
                {
                    var root = document.RootElement;

                    return new AlpacaPlaceOrderResult
                    {
                        IsSuccess = true,
                        Endpoint = endpoint,
                        OrderId = GetString(root, "id"),
                        ClientOrderId = GetString(root, "client_order_id"),
                        Symbol = GetString(root, "symbol"),
                        Status = GetString(root, "status"),
                        SubmittedQuantity = GetNullableDecimal(root, "qty"),
                        FilledQuantity = GetNullableDecimal(root, "filled_qty"),
                        FilledAveragePrice = GetNullableDecimal(root, "filled_avg_price"),
                        SubmittedAt = GetNullableDateTimeOffset(root, "submitted_at"),
                        FilledAt = GetNullableDateTimeOffset(root, "filled_at"),
                        ResponseJson = body
                    };
                }
            }
            catch (Exception exception)
            {
                return new AlpacaPlaceOrderResult
                {
                    IsSuccess = false,
                    Endpoint = endpoint,
                    Error = "Alpaca returned an unreadable order response: " + exception.Message,
                    ResponseJson = body
                };
            }
        }

        private static string ExtractAlpacaError(string body)
        {
            if (string.IsNullOrWhiteSpace(body))
            {
                return null;
            }

            try
            {
                using (var document = JsonDocument.Parse(body))
                {
                    var root = document.RootElement;
                    var message = GetString(root, "message");
                    if (!string.IsNullOrWhiteSpace(message))
                    {
                        return message;
                    }

                    var error = GetString(root, "error");
                    if (!string.IsNullOrWhiteSpace(error))
                    {
                        return error;
                    }
                }
            }
            catch
            {
                // Ignore parse issues and fall back to generic HTTP errors.
            }

            return null;
        }

        private async Task<string> SendJsonAsync(AlpacaAccountRequest request, string url)
        {
            if (string.IsNullOrWhiteSpace(request.ApiKey) || string.IsNullOrWhiteSpace(request.ApiSecret))
            {
                throw new InvalidOperationException("Alpaca API key and secret are required.");
            }

            using (var client = _httpClientFactory.CreateClient())
            using (var message = new HttpRequestMessage(HttpMethod.Get, url))
            {
                client.Timeout = TimeSpan.FromSeconds(20);
                message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                message.Headers.Add("APCA-API-KEY-ID", request.ApiKey.Trim());
                message.Headers.Add("APCA-API-SECRET-KEY", request.ApiSecret.Trim());

                using (var response = await client.SendAsync(message))
                {
                    var body = await response.Content.ReadAsStringAsync();
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new InvalidOperationException(
                            "Alpaca request failed with HTTP " + (int)response.StatusCode + ".");
                    }

                    return body;
                }
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

        private static DateTime? GetNullableDateTimeOffset(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out var property))
            {
                return null;
            }

            if (property.ValueKind == JsonValueKind.String &&
                DateTimeOffset.TryParse(property.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var value))
            {
                return value.UtcDateTime;
            }

            return null;
        }
    }
}
