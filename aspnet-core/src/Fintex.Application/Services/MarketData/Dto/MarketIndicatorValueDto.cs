using Abp.Application.Services.Dto;
using System;

namespace Fintex.Investments.MarketData.Dto
{
    /// <summary>
    /// Represents a single requested market indicator value.
    /// </summary>
    public class MarketIndicatorValueDto : EntityDto<long>
    {
        public string Symbol { get; set; }

        public MarketDataProvider Provider { get; set; }

        public MarketIndicatorType Indicator { get; set; }

        public decimal? Value { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
