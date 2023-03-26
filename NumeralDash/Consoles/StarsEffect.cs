using NumeralDash.Consoles.SpecialScreens;
using System.Linq;

namespace NumeralDash.Consoles;

class StarsEffect : ScreenSurface
{
    const int MaxStarPosSearchAttempts = 100;
    const byte AlphaChangeSpeed = 1;
    readonly Star[] _stars = new Star[10];
    readonly Rectangle _starsArea;

    public StarsEffect(int width, int height) : base(width, height)
    {
        _starsArea = Surface.Area.Expand(-1, -1);
        for (int i = 0; i < _stars.Length; i++)
            _stars[i] = Star.Empty;
    }

    Star CreateStar(int alpha = 0)
    {
        string text = Game.Instance.Random.Next(10, 255).ToString();
        alpha = alpha == 0 ? Game.Instance.Random.Next(byte.MaxValue - 1) : alpha;
        Color color = Program.GetRandBrightColor().SetAlpha((byte)alpha);

        Rectangle starArea = new(0, 0, text.Length + Star.HorizontalMargin * 2, Star.VerticalMargin * 2 + 1);

        for (int i = 0; i <= MaxStarPosSearchAttempts; i++)
        {
            // find an area for the star that is not colliding with any other text already printed
            starArea = starArea.WithPosition(_starsArea.RandomPosition());
            if (_starsArea.Contains(starArea))
            {
                if (AreaIsEmpty(starArea))
                {
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
        if (starArea.Positions().Any(p => (Parent as ScreenSurface)?.Surface.GetGlyph(p.X, p.Y) != 0))
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

    public override void Render(TimeSpan delta)
    {
        base.Render(delta);

        if (Parent is not SpecialScreen || !IsVisible) return;

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
                    (byte)(alpha - AlphaChangeSpeed) : (byte)(alpha + AlphaChangeSpeed);
                star.Color = star.Color.SetAlpha(alpha);
            }

            // print current state of the star
            PrintStar(star);
        }
    }

    protected override void OnParentChanged(IScreenObject oldParent, IScreenObject newParent)
    {
        if (Parent is SpecialScreen screen)
            screen.VisibleChanged += SpecialScreen_OnIsVisibleChanged;

        base.OnParentChanged(oldParent, newParent);
    }

    protected override void OnVisibleChanged()
    {
        if (!IsVisible)
        {
            for (int i = 0; i < _stars.Length; i++)
            {
                var star = _stars[i];
                if (star != Star.Empty)
                {
                    EraseStar(star);
                    _stars[i] = Star.Empty;
                }
            }
        }
        base.OnVisibleChanged();
    }

    void SpecialScreen_OnIsVisibleChanged(object? o, EventArgs e)
    {
        if (o is SpecialScreen screen)
            IsVisible = screen.IsVisible;
    }
}

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