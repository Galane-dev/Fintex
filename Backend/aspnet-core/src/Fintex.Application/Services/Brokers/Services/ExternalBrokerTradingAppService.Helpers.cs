using Fintex.Investments.MarketData.Dto;
using System;

namespace Fintex.Investments.Brokers
{
    public partial class ExternalBrokerTradingAppService
    {
        private static string BuildExposureKey(string symbol, TradeDirection direction)
        {
            return $"{symbol?.Trim()?.ToUpperInvariant()}::{direction}";
        }

        private static string MapToAlpacaSymbol(string symbol)
        {
            var normalized = (symbol ?? string.Empty).Trim().ToUpperInvariant();
            if (normalized.EndsWith("USDT", StringComparison.Ordinal))
            {
                return normalized.Substring(0, normalized.Length - 4) + "/USD";
            }

            if (normalized.EndsWith("USD", StringComparison.Ordinal))
            {
                return normalized.Substring(0, normalized.Length - 3) + "/USD";
            }

            throw new Abp.UI.UserFriendlyException("Only USD-quoted crypto symbols are supported for Alpaca routing right now.");
        }

        private static string GetAlternateMarketSymbol(string symbol, MarketDataProvider provider)
        {
            if (provider != MarketDataProvider.Binance || string.IsNullOrWhiteSpace(symbol))
            {
                return null;
            }

            var normalized = symbol.Trim().ToUpperInvariant().Replace("/", string.Empty, StringComparison.Ordinal);
            if (normalized.EndsWith("USD", StringComparison.Ordinal) &&
                !normalized.EndsWith("USDT", StringComparison.Ordinal))
            {
                return normalized.Substring(0, normalized.Length - 3) + "USDT";
            }

            return null;
        }

        private static string MapFromAlpacaSymbol(string symbol)
        {
            var normalized = (symbol ?? string.Empty).Trim().ToUpperInvariant();
            if (normalized.EndsWith("/USD", StringComparison.Ordinal))
            {
                return normalized.Replace("/USD", "USDT", StringComparison.Ordinal);
            }

            if (normalized.EndsWith("USD", StringComparison.Ordinal))
            {
                return normalized.Substring(0, normalized.Length - 3) + "USDT";
            }

            return normalized.Replace("/", string.Empty, StringComparison.Ordinal);
        }

        private static TradeDirection MapAlpacaSide(string side)
        {
            return string.Equals(side, "sell", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(side, "short", StringComparison.OrdinalIgnoreCase)
                ? TradeDirection.Sell
                : TradeDirection.Buy;
        }

        private static string BuildClientOrderId(long userId)
        {
            return $"fintex-{userId}-{DateTime.UtcNow:yyyyMMddHHmmssfff}";
        }
    }
}
