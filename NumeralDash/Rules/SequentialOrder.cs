using NumeralDash.Entities;

namespace NumeralDash.Rules;

class SequentialOrder : CollectionRuleBase
{
    public SequentialOrder(int count) : base(count, "Sequential Order", Color.Yellow)
    {
        for (int i = 1; i <= count; i++)
            Numbers.Enqueue(new Number(i));
    }
}