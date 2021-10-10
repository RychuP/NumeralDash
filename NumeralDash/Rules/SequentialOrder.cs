using NumeralDash.Entities;
using SadRogue.Primitives;

namespace NumeralDash.Rules
{
    class SequentialOrder : RuleBase, IRule
    {
        public string Description => "Sequential order.";

        public SequentialOrder(int count) : base(count)
        {
            Color = Color.Yellow;

            // populate remaining numbers
            for (int i = 1; i <= NumberCount; i++)
            {
                var n = new Number(i);
                RemainingNumbers.Add(n);
                Numbers[i - 1] = n;
            }

            // set the number to find
            SetNextNumber();
        }

        public void SetNextNumber()
        {
            if (RemainingNumbers.Count >= 1)
            {
                NextNumber = RemainingNumbers[0];
                RemainingNumbers.RemoveAt(0);
            }
            else NextNumber = Number.Finished;

            OnNextNumberChanged();
        }
    }
}
