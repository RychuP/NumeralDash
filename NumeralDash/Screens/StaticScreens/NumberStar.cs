namespace NumeralDash.Screens.StaticScreens;

// a number randomly appearing on static screens 
class NumberStar
{
    public static readonly NumberStar Empty = new(string.Empty, Color.Transparent, Rectangle.Empty);

    public const int HorizontalMargin = 2;
    public const int VerticalMargin = 1;

    public string Text { get; init; }
    public Color Color { get; set; }
    public Point Position { get; init; }
    public Rectangle Area { get; init; }
    public bool AlphaIsGoingDown { get; set; } = false;

    public NumberStar(string text, Color color, Rectangle area)
    {
        (Text, Color, Area) = (text, color, area);
        Position = area.Position + (HorizontalMargin, VerticalMargin);
    }

    public bool Overlaps(Rectangle area)
    {
        var expandedArea = Area.Expand(3, 1);
        bool result = expandedArea.Intersects(area);
        return result;
    }
}