using System;
using NumeralDash.World;
using SadConsole;
using SadRogue.Primitives;

namespace NumeralDash.Other
{
    public static class Extensions
    {
        public static Direction GetRandomDirection(this Direction d, Direction[] directions)
        {
            int index = Game.Instance.Random.Next(0, directions.Length - 1);
            return directions[index];
        }

    }
}
