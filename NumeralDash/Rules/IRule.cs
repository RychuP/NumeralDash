using NumeralDash.Entities;
using SadRogue.Primitives;
using System;

namespace NumeralDash.Rules
{
    interface IRule
    {
        public Color Color { get; }

        public string Description { get; }

        public Number NextNumber { get; }

        public void SetNextNumber();

        public Number[] Numbers { get; }

        // Number is next number, int is numbers remaining
        public event Action<Number, int> NextNumberChanged;
    }
}
