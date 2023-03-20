using NumeralDash.Entities;
using SadConsole;
using SadRogue.Primitives;

namespace NumeralDash.Rules
{
    class RandomOrder : CollectionRuleBase, ICollectionRule
    {
        public string Description => "Random Order";

        public RandomOrder(int count) : base(count)
        {
            Color = Color.LightSalmon;

            // generate numbers
            for (int i = 1; i <= NumberCount; i++)
            {
                var n = new Number(i);
                RemainingNumbers.Add(n);
                Numbers[i - 1] = n;
            }

            // set the number to find
            SetNextNumber();
        }

        public override void SetNextNumber()
        {
            if (RemainingNumbers.Count > 1)
            {
                int index = Program.GetRandomIndex(RemainingNumbers.Count);
                SetNextAndRemove(index);
                OnNextNumberChanged(RemainingNumbers.Count + 1);
            }
            else if (RemainingNumbers.Count == 1)
            {
                SetNextAndRemove(0);
                OnNextNumberChanged(1);
            }
            else
            {
                NextNumber = Number.Finished;
                OnNextNumberChanged(0);
            }
        }
    }
}
