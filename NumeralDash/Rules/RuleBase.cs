using System;
using NumeralDash.Entities;
using System.Collections.Generic;
using SadRogue.Primitives;

namespace NumeralDash.Rules
{
    class RuleBase
    {
        static Type[] rules = 
        {
            typeof(UpAndDownOrder),
            typeof(ReverseOrder),
            typeof(SequentialOrder),
            typeof(RandomOrder),
        };

        public static IRule GetRandomRule(int numberCount)
        {
            var index = Program.GetRandomIndex(rules.Length);
            object? o = Activator.CreateInstance(rules[index], numberCount);
            if (o is IRule r) return r;
            else throw new InvalidOperationException("Could not create a new rule.");
        }

        #region Storage

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

        #endregion

        public RuleBase(int count)
        {
            if (count < 1)
            {
                throw new ArgumentException("Minimum amount of numbers to generate is 1.");
            }

            NumberCount = count;
            RemainingNumbers = new();
            Numbers = new Number[NumberCount];
        }

        public virtual void SetNextNumber()
        {
            if (RemainingNumbers.Count >= 1)
            {
                SetNextAndRemove(0);
                OnNextNumberChanged(RemainingNumbers.Count + 1);
            }
            else
            {
                NextNumber = Number.Finished;
                OnNextNumberChanged(0);
            }
        }

        protected void SetNextAndRemove(int index)
        {
            NextNumber = RemainingNumbers[index];
            RemainingNumbers.RemoveAt(index);
        }

        #region Events

        protected void OnNextNumberChanged(int numbersRemaining)
        {
            NextNumberChanged?.Invoke(NextNumber, numbersRemaining);
        }

        public event Action<Number, int>? NextNumberChanged;

        #endregion
    }
}
