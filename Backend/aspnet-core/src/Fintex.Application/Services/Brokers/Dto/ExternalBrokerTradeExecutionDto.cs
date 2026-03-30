using Fintex.Investments.Trading.Dto;

namespace Fintex.Investments.Brokers.Dto
{
    /// <summary>
    /// Result returned after executing a live market order through an external broker.
    /// </summary>
    public class ExternalBrokerTradeExecutionDto
    {
        public long ConnectionId { get; set; }

        public string BrokerName { get; set; }

        public string BrokerEnvironment { get; set; }

        public string BrokerSymbol { get; set; }

        public string BrokerOrderId { get; set; }

        public string BrokerOrderStatus { get; set; }

        public decimal? FilledAveragePrice { get; set; }

        public TradeDto Trade { get; set; }

        public string Headline { get; set; }

        public string Summary { get; set; }
    }
}
