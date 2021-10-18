using System.Linq;
using NumeralDash.Entities;
using SadRogue.Primitives;

namespace NumeralDash.Rules
{
    class ReverseOrder : RuleBase, IRule
    {
        public string Description => "Reverse order";

        public ReverseOrder(int count) : base(count)
        {
            Color = Color.Turquoise;

            // populate remaining numbers
            for (int i = NumberCount; i >= 1; i--)
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
