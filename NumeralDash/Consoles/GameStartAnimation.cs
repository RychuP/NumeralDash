using NumeralDash.Consoles.SpecialScreens;
using SadConsole.Instructions;

namespace NumeralDash.Consoles;

class GameStartAnimation : ScreenSurface
{
    readonly Color _backgroundColor = Color.Black;

    public GameStartAnimation(int width, int height) : base(width, height)
    {
        Surface.DefaultForeground = Color.LightBlue;
        Font = Fonts.C64;
        IsVisible = false;
    }

    public void Play(Dungeon dungeon, StartScreen startScreen)
    {
        Surface.Clear();
        var instructions = new GameStartAnimationInstructions(dungeon, this, startScreen);
        SadComponents.Add(instructions);
    }

    public void DrawRectangle(Rectangle rectangle)
    {
        // clear everything with black color
        Surface.DefaultBackground = _backgroundColor;
        Surface.Clear();

        // erase the center of the mask with transparent color
        Surface.DefaultBackground = Color.Transparent;
        Surface.Clear(rectangle);

        // draw new smaller rectangle
        Surface.DrawRectangle(rectangle, Surface.DefaultForeground, _backgroundColor);
    }

    public void OnFinished()
    {
        Finished?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler? Finished;
}

class GameStartAnimationInstructions : InstructionSet
{
    public GameStartAnimationInstructions(Dungeon dungeon, GameStartAnimation host, StartScreen startScreen)
    {
        RemoveOnFinished = true;

        // create animation rectangles
        Rectangle rectangle = host.Surface.Area;

        // calculate animation steps
        int halfHeight = host.Surface.Area.Height / 2;
        double horizontalRadius = (double)host.Surface.Area.Width / 2;
        int verticalRadius = halfHeight;
        double horizontalStep = (double)horizontalRadius / verticalRadius;

        // create animation instructions
        TimeSpan speed = TimeSpan.FromMilliseconds(100);
        TimeSpan delta = speed;

        Sounds.Level.Play();

        this.Code((o, t) =>
        {
            delta += t;
            if (delta >= speed)
            {
                delta = TimeSpan.Zero;

                // draw rectangle
                host.DrawRectangle(rectangle);

                // recalculate sizes
                horizontalRadius -= horizontalStep;
                horizontalRadius = horizontalRadius < 1 ? 1 : horizontalRadius;
                verticalRadius -= 1;

                // create a smaller rectangle
                if (verticalRadius > 0)
                    rectangle = new(host.Surface.Area.Center, (int)horizontalRadius, verticalRadius);
                else
                    return true;
            }
            return false;
        })
        .Code(() =>
        {
            delta = speed;
            dungeon.ChangeLevel();
            startScreen.IsVisible = false;
            dungeon.IsVisible = true;

            Sounds.Level.Play();
        })
        .Code((o, t) =>
        {
            delta += t;
            if (delta >= speed)
            {
                delta = TimeSpan.Zero;

                // draw rectangle
                host.DrawRectangle(rectangle);

                // recalculate sizes
                horizontalRadius += horizontalStep;
                horizontalRadius = horizontalRadius > host.Surface.Width ? host.Surface.Width : horizontalRadius;
                verticalRadius += 1;

                // create a larger rectangle
                if (verticalRadius <= halfHeight)
                    rectangle = new(host.Surface.Area.Center, (int)horizontalRadius, verticalRadius);
                else
                    return true;
            }
            return false;
        })
        .Code(() =>
        {
            host.Surface.Clear();
        });
    }

    public override void OnRemoved(IScreenObject host)
    {
        if (host is GameStartAnimation parent)
            parent.OnFinished();
        base.OnRemoved(host);
    }
}