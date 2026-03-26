using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;

namespace Fintex.Investments
{
    /// <summary>
    /// Raw execution event captured from an external broker stream for later analytics and auditing.
    /// </summary>
    public class ExternalBrokerExecutionEvent : CreationAuditedAggregateRoot<long>, IMayHaveTenant
    {
        public const int MaxEventTypeLength = 64;
        public const int MaxExecutionIdLength = 128;
        public const int MaxOrderIdLength = 128;
        public const int MaxClientOrderIdLength = 128;
        public const int MaxSymbolLength = 32;
        public const int MaxStatusLength = 64;
        public const int MaxPayloadLength = 8000;

        protected ExternalBrokerExecutionEvent()
        {
        }

        public ExternalBrokerExecutionEvent(
            int? tenantId,
            long userId,
            long externalBrokerConnectionId,
            ExternalBrokerProvider brokerProvider,
            ExternalBrokerPlatform brokerPlatform,
            string brokerEnvironment,
            string eventType,
            string executionId,
            string brokerOrderId,
            string brokerClientOrderId,
            string brokerSymbol,
            string normalizedSymbol,
            string brokerOrderStatus,
            TradeDirection? direction,
            AssetClass assetClass,
            decimal? quantity,
            decimal? filledQuantity,
            decimal? eventQuantity,
            decimal? price,
            decimal? filledAveragePrice,
            decimal? positionQuantity,
            DateTime? occurredAt,
            string rawPayloadJson)
        {
            TenantId = tenantId;
            UserId = userId;
            ExternalBrokerConnectionId = externalBrokerConnectionId;
            BrokerProvider = brokerProvider;
            BrokerPlatform = brokerPlatform;
            BrokerEnvironment = LimitRequired(brokerEnvironment, ExternalBrokerConnection.MaxServerLength, "Broker environment is required.");
            EventType = LimitRequired(eventType, MaxEventTypeLength, "Event type is required.");
            ExecutionId = LimitOptional(executionId, MaxExecutionIdLength);
            BrokerOrderId = LimitOptional(brokerOrderId, MaxOrderIdLength);
            BrokerClientOrderId = LimitOptional(brokerClientOrderId, MaxClientOrderIdLength);
            BrokerSymbol = LimitOptional(brokerSymbol, MaxSymbolLength);
            NormalizedSymbol = LimitOptional(normalizedSymbol, MaxSymbolLength);
            BrokerOrderStatus = LimitOptional(brokerOrderStatus, MaxStatusLength);
            Direction = direction;
            AssetClass = assetClass;
            Quantity = quantity;
            FilledQuantity = filledQuantity;
            EventQuantity = eventQuantity;
            Price = price;
            FilledAveragePrice = filledAveragePrice;
            PositionQuantity = positionQuantity;
            OccurredAt = occurredAt;
            RawPayloadJson = LimitOptional(rawPayloadJson, MaxPayloadLength);
        }

        public int? TenantId { get; set; }

        public long UserId { get; protected set; }

        public long ExternalBrokerConnectionId { get; protected set; }

        public ExternalBrokerProvider BrokerProvider { get; protected set; }

        public ExternalBrokerPlatform BrokerPlatform { get; protected set; }

        public string BrokerEnvironment { get; protected set; }

        public string EventType { get; protected set; }

        public string ExecutionId { get; protected set; }

        public string BrokerOrderId { get; protected set; }

        public string BrokerClientOrderId { get; protected set; }

        public string BrokerSymbol { get; protected set; }

        public string NormalizedSymbol { get; protected set; }

        public string BrokerOrderStatus { get; protected set; }

        public TradeDirection? Direction { get; protected set; }

        public AssetClass AssetClass { get; protected set; }

        public decimal? Quantity { get; protected set; }

        public decimal? FilledQuantity { get; protected set; }

        public decimal? EventQuantity { get; protected set; }

        public decimal? Price { get; protected set; }

        public decimal? FilledAveragePrice { get; protected set; }

        public decimal? PositionQuantity { get; protected set; }

        public DateTime? OccurredAt { get; protected set; }

        public string RawPayloadJson { get; protected set; }

        private static string LimitRequired(string value, int maxLength, string error)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException(error, nameof(value));
            }

            var trimmed = value.Trim();
            return trimmed.Length <= maxLength ? trimmed : trimmed.Substring(0, maxLength);
        }

        private static string LimitOptional(string value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var trimmed = value.Trim();
            return trimmed.Length <= maxLength ? trimmed : trimmed.Substring(0, maxLength);
        }
    }
}
