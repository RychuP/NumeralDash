using NumeralDash.Entities;

namespace NumeralDash.Rules
{
    class SequentialOrder : RuleBase
    {
        public override string Description => base.Description + "Collect all numbers in a sequntial order starting with 1.";

        public SequentialOrder(int count) : base(count)
        {
            // populate remaining numbers
            for (int i = 1; i <= NumberCount; i++)
            {
                RemainingNumbers.Add(new Number(i));
            }

            // save a copy of all the numbers before removing the NextNumber
            Numbers = RemainingNumbers.ToArray();

            // set the number to find
            SetNextNumber();
        }

        protected override void SetNextNumber()
        {
            if (RemainingNumbers.Count >= 1)
            {
                NextNumber = RemainingNumbers[0];
                RemainingNumbers.RemoveAt(0);
            }
            else NextNumber = Number.Finished;
        }
    }
}
