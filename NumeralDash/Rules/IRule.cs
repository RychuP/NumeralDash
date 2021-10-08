using NumeralDash.Entities;

namespace NumeralDash.Rules
{
    interface IRule
    {
        public string Description { get; }

        public string NumberToFind { get; }

        public Number GetNext(Number? lastNumber);
    }
}
