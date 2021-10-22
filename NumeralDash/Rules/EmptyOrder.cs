using NumeralDash.Entities;
using SadConsole;
using SadRogue.Primitives;

namespace NumeralDash.Rules
{
    /// <summary>
    /// Empty order in case of a map generation failure.
    /// </summary>
    class EmptyOrder : RuleBase, IRule
    {
        public string Description => "Empty Order.";

        public EmptyOrder() : base(10) { }
    }
}
