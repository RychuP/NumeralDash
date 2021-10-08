using NumeralDash.Entities;
using System.Collections.Generic;

namespace NumeralDash.Rules
{
    class RuleBase : IRule
    {
        public virtual string Description => "Rule: ";

        public virtual string NumberToFind => $"Next number to collect: {NextNumber}";

        /// <summary>
        /// A list of remaing numbers to be collected.
        /// </summary>
        protected List<Number> RemainingNumbers { get; init; }

        /// <summary>
        /// Returns all numbers as an array.
        /// </summary>
        public Number[] Numbers => RemainingNumbers.ToArray();

        /// <summary>
        /// Next number that need to be collected.
        /// </summary>
        protected Number NextNumber = Number.Empty;

        /// <summary>
        /// Amount of numbers to be generated.
        /// </summary>
        protected int NumberCount;

        public RuleBase(int count)
        {
            // limit the count of numbers to generate
            NumberCount = (count < 10) ? 10 : (count > 100) ? 100 : count;

            RemainingNumbers = new();
        }

        protected virtual void SetNextNumber() { }

        public Number GetNext(Number? lastDeposit)
        {
            if (lastDeposit is null) return NextNumber;
            else if (NextNumber == Number.Finished) return Number.Finished;
            else if (lastDeposit != NextNumber) return NextNumber;
            else
            {
                SetNextNumber();
                return NextNumber;
            }
        }
    }
}
