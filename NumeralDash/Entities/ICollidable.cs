namespace NumeralDash.Entities;

interface ICollidable
{
    public bool CollidesWith(Point p);

    public bool IsCloseTo(ICollidable c);

    public Point Coord { get;  set; }

    public Point[] Coords { get; }

    public int Size { get; }
}
