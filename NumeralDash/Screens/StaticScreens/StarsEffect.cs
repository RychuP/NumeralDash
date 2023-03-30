using System.Linq;

namespace NumeralDash.Screens.StaticScreens;

class StarsEffect : ScreenSurface
{
    const int MaxStarPosSearchAttempts = 100;
    const byte AlphaChangeSpeed = 1;
    readonly NumberStar[] _stars = new NumberStar[10];

    public StarsEffect(int width, int height) : base(width, height)
    {
        for (int i = 0; i < _stars.Length; i++)
            _stars[i] = NumberStar.Empty;
    }

    NumberStar CreateStar(int alpha = 0)
    {
        string text = Game.Instance.Random.Next(10, 255).ToString();
        alpha = alpha == 0 ? Game.Instance.Random.Next(byte.MaxValue - 1) : alpha;
        Color color = Program.GetRandBrightColor().SetAlpha((byte)alpha);

        Rectangle starArea = new(0, 0, text.Length + NumberStar.HorizontalMargin * 2, NumberStar.VerticalMargin * 2 + 1);

        for (int i = 0; i <= MaxStarPosSearchAttempts; i++)
        {
            // find an area for the star that is not colliding with any other text already printed
            starArea = starArea.WithPosition(Surface.Area.RandomPosition());
            if (Surface.Area.Contains(starArea))
            {
                if (AreaIsEmpty(starArea))
                {
                    if (!_stars.Any(s => s.Overlaps(starArea)))
                        break;
                }
            }

            if (i == MaxStarPosSearchAttempts)
                return NumberStar.Empty;
        }

        return new NumberStar(text, color, starArea);
    }

    void ReplaceStar(NumberStar star)
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

    void PrintStar(NumberStar star)
    {
        ColoredString text = new(star.Text, star.Color, Color.Transparent);
        Surface.Print(star.Position, text);
    }

    void EraseStar(NumberStar star)
    {
        string text = new((char)0, star.Text.Length);
        Surface.Print(star.Position, text);
    }

    public override void Render(TimeSpan delta)
    {
        base.Render(delta);

        if (Parent is not StaticScreen || !IsVisible) return;

        foreach (var star in _stars)
        {
            if (star == NumberStar.Empty)
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

    void StaticScreen_OnIsVisibleChanged(object? o, EventArgs e)
    {
        if (o is StaticScreen screen)
            IsVisible = screen.IsVisible;
    }

    protected override void OnParentChanged(IScreenObject oldParent, IScreenObject newParent)
    {
        if (newParent is StaticScreen screen)
            screen.VisibleChanged += StaticScreen_OnIsVisibleChanged;
        base.OnParentChanged(oldParent, newParent);
    }

    protected override void OnVisibleChanged()
    {
        if (!IsVisible)
        {
            for (int i = 0; i < _stars.Length; i++)
            {
                var star = _stars[i];
                if (star != NumberStar.Empty)
                {
                    EraseStar(star);
                    _stars[i] = NumberStar.Empty;
                }
            }
        }
        base.OnVisibleChanged();
    }
}