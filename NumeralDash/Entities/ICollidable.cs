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

        public bool CollidesWith(ICollidable c);

        public Point Coord { set; }

        public Point[] Coords { get; }

        public int Size { get; }

        /// <summary>
        /// Returns the position and all the points directly around it.
        /// </summary>
        /// <returns></returns>
        public Point[] GetExpandedArea();
    }
}
