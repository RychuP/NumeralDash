using System.Linq;

namespace NumeralDash.Consoles.SpecialScreens;

class StartScreen : SpecialScreen
{
    const int MaxStarPosSearchAttempts = 100;
    readonly Star[] _stars = new Star[10];
    readonly TimeSpan _speed = TimeSpan.FromMilliseconds(100);
    readonly Rectangle _starsArea;
    byte _alphaChangeSpeed = 1;
    TimeSpan _delta = TimeSpan.Zero;

    public StartScreen(int width, int height) : base(width, height, "numeral", "dash")
    {
        Print(0, "Collect all numbers scattered around the map in the given order");
        Print(2, "and leave before the time runs out.");
        Print(4, $"{Orange("Controls")}: {Green("Arrow Keys")} move, " +
            $"{Green("F5")} toggle full screen, {Esc} pause or exit,");
        Print(6, $"{Green("Left Shift")} auto move stopping at numbers and the exit,");
        Print(8, $"{Green("Left Ctrl")} auto move ignoring numbers and stopping at road intersections.");
        Print(10, $"Press {Enter} to start.");
        IsVisible = true;

        _starsArea = Surface.Area.Expand(-1, -1);

        // populate stars
        for (int i = 0; i < _stars.Length; i++)
            _stars[i] = Star.Empty;

        for (int i = 0; i < _stars.Length; i++)
            _stars[i] = CreateStar();
    }

    Star CreateStar(int alpha = 0)
    {
        string text = Game.Instance.Random.Next(10, 255).ToString();
        alpha = alpha == 0 ? Game.Instance.Random.Next(byte.MaxValue - 1) : alpha;
        Color color = Program.GetRandBrightColor().SetAlpha((byte)alpha);

        // find an area for the star that is within the Surface and not colliding with any other text already printed
        Rectangle starArea = new(0, 0, text.Length + Star.HorizontalMargin * 2, Star.VerticalMargin * 2 + 1);

        for (int i = 0; i <= MaxStarPosSearchAttempts; i++)
        {
            starArea = starArea.WithPosition(_starsArea.RandomPosition());
            if (_starsArea.Contains(starArea)) {
                if (AreaIsEmpty(starArea)) {
                    if (!_stars.Any(s => s.Overlaps(starArea)))
                        break;
                }
            }
                    
            if (i == MaxStarPosSearchAttempts)
                return Star.Empty;
        }

        return new Star(text, color, starArea);
    }

    void ReplaceStar(Star star)
    {
        int i = Array.IndexOf(_stars, star);
        _stars[i] = CreateStar();
    }

    bool AreaIsEmpty(Rectangle starArea)
    {
        if (starArea.Positions().Any(p => Surface.GetGlyph(p.X, p.Y) != 0))
            return false;
        return true;
    }

    void PrintStar(Star star)
    {
        ColoredString text = new(star.Text, star.Color, Color.Transparent);
        Surface.Print(star.Position, text);
    }

    void EraseStar(Star star)
    {
        string text = new((char)0, star.Text.Length);
        Surface.Print(star.Position, text);
    }

    public override void Update(TimeSpan delta)
    {

        foreach (var star in _stars)
        {
            if (star == Star.Empty)
            {
                ReplaceStar(star);
                continue;
            }

            byte alpha = star.Color.A;

            // star is diminished
            if (alpha <= 0)
            {
                EraseStar(star);
                ReplaceStar(star);
                continue;
            }

            // star reached its full brightness
            else if (alpha >= byte.MaxValue)
            {
                star.AlphaIsGoingDown = true;
                star.Color = star.Color.SetAlpha(byte.MaxValue - 1);
            }

            // star is changing its brightness
            else
            {
                alpha = star.AlphaIsGoingDown ? 
                    (byte)(alpha - _alphaChangeSpeed) : (byte)(alpha + _alphaChangeSpeed);
                star.Color = star.Color.SetAlpha(alpha);
            }

            // print current state of the star
            PrintStar(star);
        }

        base.Update(delta);
    }
}

// flickering numbers appearing on the start screen
class Star
{
    public static readonly Star Empty = new Star(string.Empty, Color.Transparent, Rectangle.Empty);

    public const int HorizontalMargin = 2;
    public const int VerticalMargin = 1;
    
    public string Text { get; init; }
    public Color Color { get; set; }
    public Point Position { get; init; }
    public Rectangle Area { get; init; }
    public bool AlphaIsGoingDown { get; set; } = false;

    public Star(string text, Color color, Rectangle area)
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