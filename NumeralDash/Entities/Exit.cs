using System.Linq;
using SadConsole.Entities;

namespace NumeralDash.Entities;

class Exit : Entity, ICollidable
{
    readonly Point[] _coords;

    public Exit() : base(Color.LightGray, Color.Black, 130, (int) Layer.Items) // old glyph 240
    {
        _coords = new Point[1] { Position };
        Name = "Exit";
    }

    public bool CollidesWith(Point p) => Position == p;

    public bool IsCloseTo(ICollidable c) => Position.GetDirectionPoints().Any(p => c.CollidesWith(p));

    public int Size => 1;

    public Point Coord
    {
        get => Position;
        set
        {
            Position = value;
            _coords[0] = Position;
        }
    }

    public Point[] Coords => _coords;
}
