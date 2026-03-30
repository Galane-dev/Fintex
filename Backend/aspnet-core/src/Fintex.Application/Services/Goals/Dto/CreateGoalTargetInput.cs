using System;
using System.ComponentModel.DataAnnotations;

namespace Fintex.Investments.Goals.Dto
{
    public class CreateGoalTargetInput
    {
        [MaxLength(GoalTarget.MaxNameLength)]
        public string Name { get; set; }

        public GoalAccountType AccountType { get; set; }

        public long? ExternalConnectionId { get; set; }

        public GoalTargetType TargetType { get; set; }

        public decimal? TargetPercent { get; set; }

        public decimal? TargetAmount { get; set; }

        public DateTime DeadlineUtc { get; set; }

        [Range(typeof(decimal), "0.01", "100")]
        public decimal MaxAcceptableRisk { get; set; } = 45m;

        [Range(typeof(decimal), "0.01", "100")]
        public decimal MaxDrawdownPercent { get; set; } = 2.5m;

        [Range(typeof(decimal), "0.01", "100")]
        public decimal MaxPositionSizePercent { get; set; } = 20m;

        public GoalTradingSession TradingSession { get; set; } = GoalTradingSession.AnyTime;

        public bool AllowOvernightPositions { get; set; } = true;
    }
}
