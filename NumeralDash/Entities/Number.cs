using System;
using SadConsole;
using SadRogue.Primitives;
using SadConsole.Entities;

namespace NumeralDash.Entities
{
    class Number : Entity
    {
        int _value;

        public Number(int value) : base(Color.White, Color.Black, 48 + value, zIndex: 1)
        {
            _value = value;

            // make sure the background is not transparent
            Appearance.Background = Appearance.Background.FillAlpha();

            // find a color that meets the Minimum Brightness criteria
            do Appearance.Foreground = Program.GetRandomColor();
            while (Appearance.Foreground.GetBrightness() < Program.MinimumColorBrightness);
        }

        /// <summary>
        /// How many tiles this number occupies
        /// </summary>
        public int Size
        {
            get => _value.ToString().Length;
        }

        /// <summary>
        /// Returns the next number that need to follow this one.
        /// </summary>
        public int Next => _value + 1;

        public override bool Equals(object obj)
        {
            if (obj != null && obj is Number n) return _value == n._value;
            else return false;
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public override string ToString()
        {
            return _value.ToString();
        }

        public static bool operator ==(Number a, Number b) => a is not null && b is not null && a.Equals(b);

        public static bool operator !=(Number a, Number b) => a is not null && b is not null && !a.Equals(b);

        public static bool operator ==(Number a, int b) => a is not null && a._value == b;

        public static bool operator !=(Number a, int b) => a is not null && a._value != b;
    }
}
