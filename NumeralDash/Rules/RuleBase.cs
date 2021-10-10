using System;
using NumeralDash.Entities;
using System.Collections.Generic;
using SadRogue.Primitives;

namespace NumeralDash.Rules
{
    class RuleBase
    {
        /// <summary>
        /// A list of remaing numbers to be collected.
        /// </summary>
        protected List<Number> RemainingNumbers { get; init; }

        /// <summary>
        /// Amount of numbers to be generated.
        /// </summary>
        protected int NumberCount { get; set; }

        /// <summary>
        /// Next number to be collected.
        /// </summary>
        public Number NextNumber { get; protected set; } = Number.Empty;

        /// <summary>
        /// Initial list of all numbers to collect from the map.
        /// </summary>
        public Number[] Numbers { get; init; }

        /// <summary>
        /// Foreground color.
        /// </summary>
        public Color Color { get; protected set; }

        public RuleBase(int count)
        {
            if (count < 1 || count > 100)
            {
                throw new ArgumentException("Amount of the numbers to generate is outside of the limits allowed.");
            }

            NumberCount = count;
            RemainingNumbers = new();
            Numbers = new Number[NumberCount];
        }

        protected void OnNextNumberChanged()
        {
            NextNumberChanged?.Invoke(NextNumber);
        }

        public event Action<Number>? NextNumberChanged;
    }
}
