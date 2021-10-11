using System;
using System.Linq;
using NumeralDash.Rules;
using SadConsole;
using SadConsole.Entities;
using SadRogue.Primitives;

namespace NumeralDash.Entities
{
    class Exit : Entity, ICollidable
    {
        readonly Point[] _coords;

        public Exit() : base(Color.White, Color.Black, 240, (int) Layer.Items)
        {
            _coords = new Point[1] { Position };
            Name = "Exit";
        }

        public bool AllowsPassage()
        {
            return false;
        }

        public bool CollidesWith(Point p) => Position == p;

        public bool CollidesWith(ICollidable c) => c.Coords.Any(p => p == Position);

        public int Size => 1;

        public Point Coord
        {
            set
            {
                Position = value;
                _coords[0] = Position;
            }
        }

        public Point[] Coords => _coords;

        public Point[] GetExpandedArea() => Position.GetDirectionPoints();
    }
}
