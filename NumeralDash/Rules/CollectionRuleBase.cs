using NumeralDash.Entities;
using System.Collections.Generic;

namespace NumeralDash.Rules;

class CollectionRuleBase : ICollectionRule
{
    #region Statics
    static readonly Type[] s_collectionRules = 
    {
        //typeof(UpAndDownOrder),
        //typeof(RandomOrder),
        typeof(ReverseOrder),
        typeof(SequentialOrder),
    };

    public static ICollectionRule GetRandomRule(int numberCount)
    {
        var index = Program.GetRandomIndex(s_collectionRules.Length);
        object? o = Activator.CreateInstance(s_collectionRules[index], numberCount);
        if (o is ICollectionRule r) return r;
        else throw new InvalidOperationException("Could not create a new rule.");
    }

    public static ICollectionRule GetNextRule(int level, int numberCount)
    {
        return (level % 2) switch
        {
            0 => new SequentialOrder(numberCount),
            _ => new ReverseOrder(numberCount)
        };
    }
    #endregion Statics

    #region Fields

    #endregion Fields

    #region Constructors
    public CollectionRuleBase(int count, string title, Color color)
    {
        if (count < 1)
            throw new ArgumentException("Minimum amount of numbers to generate is 1.");

        Numbers = new Queue<Number>(count);
        Title = title;
        Color = color;
    }
    #endregion Constructors

    #region Properties
    public Number NextNumber
    {
        get
        {
            if (Numbers.Count > 0)
                return Numbers.Peek();
            else
                return Number.Empty;
        }
    }

    public Queue<Number> Numbers { get; init; }

    public Color Color { get; init; }

    public string Title { get; init; }
    #endregion Properties

    #region Methods
    public void Dequeue()
    {
        if (Numbers.Count == 0) return;
        Numbers.Dequeue();
    }
    #endregion Methods
}