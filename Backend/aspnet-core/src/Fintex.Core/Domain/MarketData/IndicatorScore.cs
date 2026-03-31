using System;

namespace Fintex.Investments
{
    /// <summary>
    /// Value object describing the score emitted by a single indicator.
    /// </summary>
    public sealed class IndicatorScore : IEquatable<IndicatorScore>
    {
        public string Name { get; }

        public decimal Value { get; }

        public decimal Score { get; }

        public IndicatorSignal Signal { get; }

        public IndicatorScore(string name, decimal value, decimal score, IndicatorSignal signal)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Indicator name is required.", nameof(name));
            }

            Name = name.Trim();
            Value = value;
            Score = score;
            Signal = signal;
        }

        public bool Equals(IndicatorScore other)
        {
            return other != null
                   && string.Equals(Name, other.Name, StringComparison.Ordinal)
                   && Value == other.Value
                   && Score == other.Score
                   && Signal == other.Signal;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as IndicatorScore);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Name.GetHashCode();
                hashCode = (hashCode * 397) ^ Value.GetHashCode();
                hashCode = (hashCode * 397) ^ Score.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)Signal;
                return hashCode;
            }
        }
    }
}
