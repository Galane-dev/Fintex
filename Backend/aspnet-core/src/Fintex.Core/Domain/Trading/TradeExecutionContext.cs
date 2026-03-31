using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;

namespace Fintex.Investments
{
    /// <summary>
    /// Stores the execution-time broker, market, and user context used later for behavioral analysis.
    /// </summary>
    public class TradeExecutionContext : CreationAuditedAggregateRoot<long>, IMayHaveTenant
    {
        public const int MaxBrokerSymbolLength = 32;
        public const int MaxEnvironmentLength = 16;
        public const int MaxOrderIdLength = 128;
        public const int MaxClientOrderIdLength = 128;
        public const int MaxStatusLength = 64;
        public const int MaxVerdictLength = 16;
        public const int MaxStructureLabelLength = 128;
        public const int MaxSummaryLength = 4000;

        protected TradeExecutionContext()
        {
        }

        public TradeExecutionContext(
            int? tenantId,
            long tradeId,
            long userId,
            long externalBrokerConnectionId,
            ExternalBrokerProvider brokerProvider,
            ExternalBrokerPlatform brokerPlatform,
            string brokerEnvironment,
            string brokerSymbol,
            TradeDirection direction,
            AssetClass assetClass,
            MarketDataProvider marketDataProvider,
            decimal quantity,
            decimal referencePrice,
            decimal? bid,
            decimal? ask,
            decimal? stopLoss,
            decimal? takeProfit,
            string notes)
        {
            TenantId = tenantId;
            TradeId = tradeId;
            UserId = userId;
            ExternalBrokerConnectionId = externalBrokerConnectionId;
            BrokerProvider = brokerProvider;
            BrokerPlatform = brokerPlatform;
            BrokerEnvironment = LimitRequired(brokerEnvironment, MaxEnvironmentLength, "Broker environment is required.");
            BrokerSymbol = LimitRequired(brokerSymbol, MaxBrokerSymbolLength, "Broker symbol is required.");
            Direction = direction;
            AssetClass = assetClass;
            MarketDataProvider = marketDataProvider;
            Quantity = quantity;
            ReferencePrice = referencePrice;
            Bid = bid;
            Ask = ask;
            StopLoss = stopLoss;
            TakeProfit = takeProfit;
            Notes = LimitOptional(notes, MaxSummaryLength);
        }

        public int? TenantId { get; set; }

        public long TradeId { get; protected set; }

        public long UserId { get; protected set; }

        public long ExternalBrokerConnectionId { get; protected set; }

        public ExternalBrokerProvider BrokerProvider { get; protected set; }

        public ExternalBrokerPlatform BrokerPlatform { get; protected set; }

        public string BrokerEnvironment { get; protected set; }

        public string BrokerSymbol { get; protected set; }

        public TradeDirection Direction { get; protected set; }

        public AssetClass AssetClass { get; protected set; }

        public MarketDataProvider MarketDataProvider { get; protected set; }

        public decimal Quantity { get; protected set; }

        public decimal ReferencePrice { get; protected set; }

        public decimal? Bid { get; protected set; }

        public decimal? Ask { get; protected set; }

        public decimal? Spread { get; protected set; }

        public decimal? SpreadPercent { get; protected set; }

        public decimal? StopLoss { get; protected set; }

        public decimal? TakeProfit { get; protected set; }

        public string MarketVerdict { get; protected set; }

        public decimal? TrendScore { get; protected set; }

        public decimal? ConfidenceScore { get; protected set; }

        public decimal? TimeframeAlignmentScore { get; protected set; }

        public decimal? StructureScore { get; protected set; }

        public string StructureLabel { get; protected set; }

        public decimal? Sma { get; protected set; }

        public decimal? Ema { get; protected set; }

        public decimal? Rsi { get; protected set; }

        public decimal? Macd { get; protected set; }

        public decimal? MacdSignal { get; protected set; }

        public decimal? MacdHistogram { get; protected set; }

        public decimal? Momentum { get; protected set; }

        public decimal? RateOfChange { get; protected set; }

        public decimal? Atr { get; protected set; }

        public decimal? AtrPercent { get; protected set; }

        public decimal? Adx { get; protected set; }

        public decimal? UserRiskTolerance { get; protected set; }

        public decimal? UserBehavioralRiskScore { get; protected set; }

        public string UserBehavioralSummary { get; protected set; }

        public string BrokerOrderId { get; protected set; }

        public string BrokerClientOrderId { get; protected set; }

        public string BrokerOrderStatus { get; protected set; }

        public decimal? BrokerSubmittedQuantity { get; protected set; }

        public decimal? BrokerFilledQuantity { get; protected set; }

        public decimal? BrokerFilledAveragePrice { get; protected set; }

        public DateTime? BrokerSubmittedAt { get; protected set; }

        public DateTime? BrokerFilledAt { get; protected set; }

        public string Notes { get; protected set; }

        public string DecisionSummary { get; protected set; }

        public string RequestPayloadJson { get; protected set; }

        public string BrokerResponseJson { get; protected set; }

        public void ApplyMarketContext(MarketDataPoint latestPoint, decimal? spread, decimal? spreadPercent)
        {
            if (latestPoint == null)
            {
                return;
            }

            Spread = spread;
            SpreadPercent = spreadPercent;
            Sma = latestPoint.Sma;
            Ema = latestPoint.Ema;
            Rsi = latestPoint.Rsi;
            Macd = latestPoint.Macd;
            MacdSignal = latestPoint.MacdSignal;
            MacdHistogram = latestPoint.MacdHistogram;
            Momentum = latestPoint.Momentum;
            RateOfChange = latestPoint.RateOfChange;
        }

        public void ApplyVerdictContext(
            MarketVerdict verdict,
            decimal? trendScore,
            decimal? confidenceScore,
            decimal? timeframeAlignmentScore,
            decimal? structureScore,
            string structureLabel,
            decimal? atr,
            decimal? atrPercent,
            decimal? adx,
            string decisionSummary)
        {
            MarketVerdict = LimitOptional(verdict.ToString(), MaxVerdictLength);
            TrendScore = trendScore;
            ConfidenceScore = confidenceScore;
            TimeframeAlignmentScore = timeframeAlignmentScore;
            StructureScore = structureScore;
            StructureLabel = LimitOptional(structureLabel, MaxStructureLabelLength);
            Atr = atr;
            AtrPercent = atrPercent;
            Adx = adx;
            DecisionSummary = LimitOptional(decisionSummary, MaxSummaryLength);
        }

        public void ApplyUserContext(decimal? riskTolerance, decimal? behavioralRiskScore, string behavioralSummary)
        {
            UserRiskTolerance = riskTolerance;
            UserBehavioralRiskScore = behavioralRiskScore;
            UserBehavioralSummary = LimitOptional(behavioralSummary, MaxSummaryLength);
        }

        public void ApplyBrokerExecution(
            string brokerOrderId,
            string brokerClientOrderId,
            string brokerOrderStatus,
            decimal? brokerSubmittedQuantity,
            decimal? brokerFilledQuantity,
            decimal? brokerFilledAveragePrice,
            DateTime? brokerSubmittedAt,
            DateTime? brokerFilledAt,
            string requestPayloadJson,
            string brokerResponseJson)
        {
            BrokerOrderId = LimitOptional(brokerOrderId, MaxOrderIdLength);
            BrokerClientOrderId = LimitOptional(brokerClientOrderId, MaxClientOrderIdLength);
            BrokerOrderStatus = LimitOptional(brokerOrderStatus, MaxStatusLength);
            BrokerSubmittedQuantity = brokerSubmittedQuantity;
            BrokerFilledQuantity = brokerFilledQuantity;
            BrokerFilledAveragePrice = brokerFilledAveragePrice;
            BrokerSubmittedAt = brokerSubmittedAt;
            BrokerFilledAt = brokerFilledAt;
            RequestPayloadJson = LimitOptional(requestPayloadJson, MaxSummaryLength);
            BrokerResponseJson = LimitOptional(brokerResponseJson, MaxSummaryLength);
        }

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
