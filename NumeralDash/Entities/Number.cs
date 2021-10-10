using System;
using SadConsole;
using SadRogue.Primitives;
using SadConsole.Entities;

namespace NumeralDash.Entities
{
    class Number : Entity
    {
        public static Number Finished = new Number(-1);
        public static Number Empty = new Number(0);

        int _value;

        public Number(int value) : base(Color.White, Color.Black, 48 + value, (int) Layer.Items)
        {
            _value = value;

            // make sure the background is not transparent
            Appearance.Background = Appearance.Background.FillAlpha();

            // find a color that meets the Minimum Brightness criteria
            do Appearance.Foreground = Program.GetRandomColor();
            while (Appearance.Foreground.GetBrightness() < Program.MinimumColorBrightness);
        }

        /// <summary>
        /// How many tiles this number occupies.
        /// </summary>
        public int Size     
        {
            get => _value.ToString().Length;
        }

        public Color Color => Appearance.Foreground;

        public override bool Equals(object? obj)
        {
            if (obj != null && obj is Number n) return _value == n._value;
            else return false;
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public override string ToString() => _value == Empty ? "" : _value.ToString();

        /// <summary>
        /// Returns the underlying int value.
        /// </summary>
        public int ToInt32() => _value;

        public static bool operator ==(Number a, Number b) => a is not null && b is not null && a.Equals(b);

        public static bool operator !=(Number a, Number b) => a is not null && b is not null && !a.Equals(b);

        public static bool operator ==(Number a, int b) => a is not null && a._value == b;
        
        public static bool operator !=(Number a, int b) => a is not null && a._value != b;

        public static bool operator ==(int a, Number b) => b is not null && a == b._value;

        public static bool operator !=(int a, Number b) => b is not null && a != b._value;

        public static int operator +(Number a, int b) => a._value + b;

        public static int operator +(int a, Number b) => a + b._value;

        public static Number operator +(Number a, Number b) => new Number(a._value + b._value);
    }
}
