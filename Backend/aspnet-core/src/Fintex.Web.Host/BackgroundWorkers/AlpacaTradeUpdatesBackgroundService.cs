using Abp.Domain.Uow;
using Abp.Runtime.Security;
using Fintex.Investments;
using Fintex.Investments.Brokers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Fintex.Web.Host.BackgroundWorkers
{
    /// <summary>
    /// Maintains Alpaca trade update websocket subscriptions for active broker connections.
    /// </summary>
    public class AlpacaTradeUpdatesBackgroundService : BackgroundService
    {
        private const string PaperStreamUrl = "wss://paper-api.alpaca.markets/stream";
        private const string LiveStreamUrl = "wss://api.alpaca.markets/stream";

        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<AlpacaTradeUpdatesBackgroundService> _logger;
        private readonly ConcurrentDictionary<long, CancellationTokenSource> _connectionWorkers = new ConcurrentDictionary<long, CancellationTokenSource>();

        public AlpacaTradeUpdatesBackgroundService(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<AlpacaTradeUpdatesBackgroundService> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var unitOfWorkManager = scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
                        var repository = scope.ServiceProvider.GetRequiredService<IExternalBrokerConnectionRepository>();

                        using (var unitOfWork = unitOfWorkManager.Begin())
                        {
                            var activeConnections = await repository.GetActiveConnectionsAsync(ExternalBrokerProvider.Alpaca);
                            var activeIds = new HashSet<long>();

                            foreach (var connection in activeConnections)
                            {
                                activeIds.Add(connection.Id);
                                if (_connectionWorkers.ContainsKey(connection.Id))
                                {
                                    continue;
                                }

                                var cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                                if (_connectionWorkers.TryAdd(connection.Id, cts))
                                {
                                    _ = Task.Run(() => RunConnectionLoopAsync(connection.Id, cts.Token), cts.Token);
                                }
                            }

                            foreach (var item in _connectionWorkers)
                            {
                                if (!activeIds.Contains(item.Key))
                                {
                                    if (_connectionWorkers.TryRemove(item.Key, out var staleCts))
                                    {
                                        staleCts.Cancel();
                                        staleCts.Dispose();
                                    }
                                }
                            }

                            await unitOfWork.CompleteAsync();
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Failed to refresh Alpaca trade update subscriptions.");
                }

                await Task.Delay(TimeSpan.FromSeconds(20), stoppingToken);
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            foreach (var item in _connectionWorkers)
            {
                item.Value.Cancel();
                item.Value.Dispose();
            }

            _connectionWorkers.Clear();
            return base.StopAsync(cancellationToken);
        }

        private async Task RunConnectionLoopAsync(long connectionId, CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        using (var scope = _serviceScopeFactory.CreateScope())
                        {
                            var unitOfWorkManager = scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
                            var repository = scope.ServiceProvider.GetRequiredService<IExternalBrokerConnectionRepository>();

                            ExternalBrokerConnection connection;
                            using (var unitOfWork = unitOfWorkManager.Begin())
                            {
                                connection = await repository.FirstOrDefaultAsync(connectionId);
                                await unitOfWork.CompleteAsync();
                            }

                            if (connection == null || !connection.IsActive || connection.Status != ExternalBrokerConnectionStatus.Connected)
                            {
                                return;
                            }

                            await ListenToConnectionAsync(connection, cancellationToken);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        return;
                    }
                    catch (Exception exception)
                    {
                        _logger.LogWarning(exception, "Alpaca trade update stream disconnected for connection {ConnectionId}. Reconnecting.", connectionId);
                    }

                    await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                }
            }
            finally
            {
                if (_connectionWorkers.TryRemove(connectionId, out var cts))
                {
                    cts.Dispose();
                }
            }
        }

        private async Task ListenToConnectionAsync(ExternalBrokerConnection connection, CancellationToken cancellationToken)
        {
            var streamUrl = connection.Server != null &&
                            connection.Server.Contains("paper-api.alpaca.markets", StringComparison.OrdinalIgnoreCase)
                ? PaperStreamUrl
                : LiveStreamUrl;

            using (var socket = new ClientWebSocket())
            {
                await socket.ConnectAsync(new Uri(streamUrl), cancellationToken);
                _logger.LogInformation("Connected Alpaca trade update stream for broker connection {ConnectionId}.", connection.Id);

                await SendJsonAsync(socket, new
                {
                    action = "auth",
                    key = connection.AccountLogin,
                    secret = SimpleStringCipher.Instance.Decrypt(connection.EncryptedPassword)
                }, cancellationToken);

                await SendJsonAsync(socket, new
                {
                    action = "listen",
                    data = new
                    {
                        streams = new[] { "trade_updates" }
                    }
                }, cancellationToken);

                while (socket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
                {
                    var payload = await ReceiveMessageAsync(socket, cancellationToken);
                    if (string.IsNullOrWhiteSpace(payload))
                    {
                        continue;
                    }

                    var update = TryParseTradeUpdate(payload);
                    if (update == null)
                    {
                        continue;
                    }

                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var unitOfWorkManager = scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
                        var ingestionService = scope.ServiceProvider.GetRequiredService<IAlpacaTradeUpdateIngestionService>();

                        using (var unitOfWork = unitOfWorkManager.Begin())
                        {
                            await ingestionService.CaptureAsync(connection.Id, update);
                            await unitOfWork.CompleteAsync();
                        }
                    }
                }
            }
        }

        private static async Task SendJsonAsync(ClientWebSocket socket, object payload, CancellationToken cancellationToken)
        {
            var json = JsonSerializer.Serialize(payload);
            var bytes = Encoding.UTF8.GetBytes(json);
            await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, cancellationToken);
        }

        private static async Task<string> ReceiveMessageAsync(ClientWebSocket socket, CancellationToken cancellationToken)
        {
            var buffer = new byte[16 * 1024];
            using (var stream = new System.IO.MemoryStream())
            {
                WebSocketReceiveResult result;
                do
                {
                    result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        return null;
                    }

                    stream.Write(buffer, 0, result.Count);
                }
                while (!result.EndOfMessage);

                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }

        private static AlpacaTradeUpdateMessage TryParseTradeUpdate(string payload)
        {
            using (var document = JsonDocument.Parse(payload))
            {
                var root = document.RootElement;
                var stream = GetString(root, "stream");
                if (!string.Equals(stream, "trade_updates", StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                if (!root.TryGetProperty("data", out var data))
                {
                    return null;
                }

                var order = data.TryGetProperty("order", out var orderElement) ? orderElement : default;
                return new AlpacaTradeUpdateMessage
                {
                    EventType = GetString(data, "event"),
                    ExecutionId = GetString(data, "execution_id"),
                    OrderId = order.ValueKind != JsonValueKind.Undefined ? GetString(order, "id") : null,
                    ClientOrderId = order.ValueKind != JsonValueKind.Undefined ? GetString(order, "client_order_id") : null,
                    Symbol = order.ValueKind != JsonValueKind.Undefined ? GetString(order, "symbol") : null,
                    Side = order.ValueKind != JsonValueKind.Undefined ? GetString(order, "side") : null,
                    OrderStatus = order.ValueKind != JsonValueKind.Undefined ? GetString(order, "status") : null,
                    OrderQuantity = order.ValueKind != JsonValueKind.Undefined ? GetNullableDecimal(order, "qty") : null,
                    FilledQuantity = order.ValueKind != JsonValueKind.Undefined ? GetNullableDecimal(order, "filled_qty") : null,
                    FilledAveragePrice = order.ValueKind != JsonValueKind.Undefined ? GetNullableDecimal(order, "filled_avg_price") : null,
                    EventQuantity = GetNullableDecimal(data, "qty"),
                    Price = GetNullableDecimal(data, "price"),
                    PositionQuantity = GetNullableDecimal(data, "position_qty"),
                    OccurredAt = GetNullableDateTimeOffset(data, "timestamp"),
                    RawPayloadJson = payload
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
