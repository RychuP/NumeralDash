using NumeralDash.Entities;

namespace NumeralDash.Rules;

class ReverseOrder : CollectionRuleBase
{
    public ReverseOrder(int count) : base(count, "Reverse Order", Color.Turquoise)
    {
        for (int i = count; i >= 1; i--)
            Numbers.Enqueue(new Number(i));
    }
}