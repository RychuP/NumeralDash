using NumeralDash.Entities;

namespace NumeralDash.Rules
{
    interface ICollectionRule
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
