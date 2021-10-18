using NumeralDash.Entities;
using SadConsole;
using SadRogue.Primitives;

namespace NumeralDash.Rules
{
    class RandomOrder : RuleBase, IRule
    {
        public string Description => "Random order";

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
                NextNumber = RemainingNumbers[index];
                RemainingNumbers.RemoveAt(index);
                OnNextNumberChanged(RemainingNumbers.Count + 1);
            }
            else if (RemainingNumbers.Count == 1)
            {
                NextNumber = RemainingNumbers[0];
                RemainingNumbers.RemoveAt(0);
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
