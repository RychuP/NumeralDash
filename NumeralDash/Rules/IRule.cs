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

        public event Action<Number> NextNumberChanged;
    }
}
