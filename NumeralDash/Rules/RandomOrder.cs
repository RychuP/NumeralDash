using NumeralDash.Entities;

namespace NumeralDash.Rules
{
    class RandomOrder : RuleBase
    {
        public override string Description => base.Description + "Numbers will be selected in a random order.";

        public RandomOrder(int count) : base(count)
        {
            // populate remaining numbers
            for (int i = 1; i < NumberCount; i++)
            {
                RemainingNumbers.Add(new Number(i));
            }

            SetNextNumber();
        }

        protected override void SetNextNumber()
        {
            if (RemainingNumbers.Count > 1)
            {
                int index = Program.GetRandomIndex(RemainingNumbers.Count);
                NextNumber = RemainingNumbers[index];
                RemainingNumbers.RemoveAt(index);
            }
            else if (RemainingNumbers.Count == 1)
            {
                NextNumber = RemainingNumbers[0];
                RemainingNumbers.RemoveAt(0);
            }
            else NextNumber = Number.Finished;
        }
    }
}
