﻿using System;
using System.Linq;
using SadConsole;
using SadRogue.Primitives;
using SadConsole.Entities;
using System.Collections.Generic;

namespace NumeralDash.Entities
{
    class Number : Entity, ICollidable
    {
        // instances of special numbers
        public static Number Finished = new(-1);
        public static Number Empty = new(0);

        readonly int _value;
        readonly Point[] _coords;
        readonly Point[] _expandedArea;

        /// <summary>
        /// Additional one digit numbers that together will form a representation of the string value.
        /// </summary>
        public NumberExtension[] Extensions { get; init; }

        /// <summary>
        /// Constructor for the main number object.
        /// </summary>
        /// <param name="value"></param>
        public Number(int value) : base(Color.White, Color.Black, value.ToString()[0], (int) Layer.Items)
        {
            _value = value;
            _coords = new Point[Size];

            int areaPositions = Size == 1 ? 9 : 9 + (Size - 1) * 3;
            _expandedArea = new Point[areaPositions];

            // make sure the background is not transparent
            Appearance.Background = Appearance.Background.FillAlpha();

            // random color for regular numbers
            if (!IsSpecial())
            {
                // find a color that meets the Minimum Brightness criteria
                do Appearance.Foreground = Program.GetRandomColor();
                while (Appearance.Foreground.GetBrightness() < Program.MinimumColorBrightness);
            }
            // set color for special numbers
            else
            {
                Appearance.Foreground = (_value == 0) ? Color.Red : Color.Green;
            }

            // check if the value occupies more than one cell and if so, create extensions
            if (Size > 1)
            {
                Extensions = new NumberExtension[Size - 1];

                for (var i = 0; i < Size - 1; i++)
                {
                    Extensions[i] = new NumberExtension(_value.ToString()[i + 1], Appearance.Foreground, this);
                }

                Coord = Position;
            }
            else
            {
                Extensions = Array.Empty<NumberExtension>();
            }
        }

        public Point Coord
        {
            set
            {
                // set position
                Position = value;
                _coords[0] = Position;

                // calculate new area positions
                var areaPositions = Position.GetDirectionPoints().ToList();

                // set position for the extensions
                if (Size > 1)
                {
                    Point p = Position;
                    for (var i = 0; i < Size - 1; i++)
                    {
                        p += Direction.Right;
                        _coords[i + 1] = p;
                        Extensions[i].Position = p;
                    }

                    // calculate new area positions for extensions
                    foreach(var e in Extensions)
                    {
                        // get all positions around the point including the point
                        var tempPositions = e.Position.GetDirectionPoints().ToList();

                        areaPositions.AddRange(tempPositions);
                    }

                    // remove duplicate border positions
                    var uniquePoints =
                        from point in areaPositions
                        group point by point into Group
                        select Group.Key;

                    if (uniquePoints.Count() != _expandedArea.Length)
                    {
                        throw new IndexOutOfRangeException("Border position counts do not match.");
                    }

                    // copy area points to the field array
                    int j = 0;
                    foreach (var point in uniquePoints)
                    {
                        _expandedArea[j++] = point;
                    }
                }
            }
        }

        public Point[] Coords => _coords;

        public Point[] GetExpandedArea() => _expandedArea.ToArray();

        public bool CollidesWith(Point p)
        {
            if (Size > 1) return Position == p || Extensions.Any(n => n.Position == p);
            else return Position == p;
        }

        public bool CollidesWith(ICollidable c)
        {
            foreach (var point in Coords)
            {
                if (c.Coords.Any(otherPoint => otherPoint == point)) return true;
            }
            return false;
        }

        /// <summary>
        /// How many tiles this number occupies.
        /// </summary>
        public int Size     
        {
            get => !IsSpecial() ? _value.ToString().Length : 0;
        }

        public void MarkAsVisible(bool visible = true)
        {
            IsVisible = visible;
            if (Size > 1)
            {
                foreach (var e in Extensions)
                {
                    e.IsVisible = visible;
                }
            }
        }

        public void MarkAsInvisible()
        {
            MarkAsVisible(false);
        }

        bool IsSpecial() => _value <= 0;

        public Color Color
        {
            get => Appearance.Foreground;
            set
            {
                Appearance.Foreground = value;
            }
        }

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
