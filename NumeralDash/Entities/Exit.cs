using System;
using NumeralDash.Rules;
using SadConsole;
using SadConsole.Entities;
using SadRogue.Primitives;

namespace NumeralDash.Entities
{
    class Exit : Entity
    {
        IRule _rule;

        public Exit(IRule rule) : base(Color.White, Color.Black, 240, (int) Layer.Items)
        {
            _rule = rule;
        }

        public bool AllowsPassage()
        {
            return false;
        }
    }
}
