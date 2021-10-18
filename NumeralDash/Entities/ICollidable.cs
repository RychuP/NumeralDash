using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NumeralDash.Entities
{
    interface ICollidable
    {
        public bool CollidesWith(Point p);

        public bool IsCloseTo(ICollidable c);

        public Point Coord { get;  set; }

        public Point[] Coords { get; }

        public int Size { get; }
    }
}
