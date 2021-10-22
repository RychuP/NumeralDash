using NumeralDash.Entities;
using SadRogue.Primitives;

namespace NumeralDash.Rules
{
    class UpAndDownOrder : RuleBase, IRule
    {
        public string Description => "Up & Down Order";
        bool _lastNumberWasHigh = true;

        public UpAndDownOrder(int count) : base(count)
        {
            Color = Color.DarkOrange;

            // populate reversed numbers
            for (int i = NumberCount; i >= 1; i--)
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
                if (_lastNumberWasHigh)
                {
                    SetNextAndRemove(0);
                    _lastNumberWasHigh = false;
                }
                else
                {
                    SetNextAndRemove(RemainingNumbers.Count - 1);
                    _lastNumberWasHigh = true;
                }
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
