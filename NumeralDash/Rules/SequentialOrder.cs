using NumeralDash.Entities;
using SadRogue.Primitives;

namespace NumeralDash.Rules
{
    class SequentialOrder : CollectionRuleBase, ICollectionRule
    {
        public string Description => "Sequential Order";

        public SequentialOrder(int count) : base(count)
        {
            Color = Color.Yellow;

            // populate sequntial numbers
            for (int i = 1; i <= NumberCount; i++)
            {
                var n = new Number(i);
                RemainingNumbers.Add(n);
                Numbers[i - 1] = n;
            }

            // set the number to find
            SetNextNumber();
        }
    }
}
