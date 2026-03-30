namespace Fintex.Web.Host.MarketData.Configuration
{
    /// <summary>
    /// Root configuration for crypto, forex, and AI connectivity.
    /// </summary>
    public class MarketDataStreamingOptions
    {
        public BinanceStreamOptions Binance { get; set; } = new BinanceStreamOptions();

        public CoinbaseStreamOptions Coinbase { get; set; } = new CoinbaseStreamOptions();

        public OandaStreamOptions Oanda { get; set; } = new OandaStreamOptions();
    }

    /// <summary>
    /// Configuration for Binance ticker streaming.
    /// </summary>
    public class BinanceStreamOptions
    {
        public bool Enabled { get; set; }

        public string StreamUrl { get; set; }

        public string Symbols { get; set; }
    }

    /// <summary>
    /// Configuration for Coinbase ticker streaming.
    /// </summary>
    public class CoinbaseStreamOptions
    {
        public bool Enabled { get; set; }

        public string StreamUrl { get; set; }

        public string ProductIds { get; set; }
    }

    /// <summary>
    /// Configuration for OANDA forex pricing streams.
    /// </summary>
    public class OandaStreamOptions
    {
        public bool Enabled { get; set; }

        public string PricingStreamUrl { get; set; }

        public string AccountId { get; set; }

        public string ApiToken { get; set; }

        public string Instruments { get; set; }
    }
}
