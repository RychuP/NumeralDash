using SadConsole.Instructions;

namespace NumeralDash.Consoles;

class Transition : ScreenSurface
{
    readonly Color _backgroundColor = Color.Black;

    public Transition(Dungeon dungeon) : base(dungeon.Surface.View.Width, dungeon.Surface.View.Height)
    {
        Position = dungeon.Position;
        Font = Fonts.C64;
        IsVisible = false;
    }

    public void Play(ScreenSurface firstScreen, ScreenSurface secondScreen, Color rectangleColor, Action? callback = null)
    {
        Surface.Clear();
        IsVisible = true;

        // set visibility
        secondScreen.IsVisible = false;
        firstScreen.IsVisible = true;

        // create animation rectangle
        Rectangle rectangle = Surface.Area;

        // calculate animation steps
        int halfHeight = Surface.Area.Height / 2;
        double horizontalRadius = (double)Surface.Area.Width / 2;
        int verticalRadius = halfHeight;
        double horizontalStep = (double)horizontalRadius / verticalRadius;

        // set animation speed
        TimeSpan speed = TimeSpan.FromMilliseconds(100);
        TimeSpan delta = speed;

        // play random level sound
        Sounds.Level.Play();

        // create instructions
        var instructions = new InstructionSet() { RemoveOnFinished = true }
        .Code((o, t) =>
        {
            delta += t;
            if (delta >= speed)
            {
                delta = TimeSpan.Zero;

                // draw rectangle
                DrawRectangle(rectangle, rectangleColor);

                // recalculate sizes
                horizontalRadius -= horizontalStep;
                horizontalRadius = horizontalRadius < 1 ? 1 : horizontalRadius;
                verticalRadius -= 1;

                // create a smaller rectangle
                if (verticalRadius > 0)
                    rectangle = new(Surface.Area.Center, (int)horizontalRadius, verticalRadius);
                else
                    return true;
            }
            return false;
        })
        .Code(() =>
        {
            delta = speed;
            callback?.Invoke();

            firstScreen.IsVisible = false;
            secondScreen.IsVisible = true;

            Sounds.Level.Play();
        })
        .Code((o, t) =>
        {
            delta += t;
            if (delta >= speed)
            {
                delta = TimeSpan.Zero;

                // draw rectangle
                DrawRectangle(rectangle, rectangleColor);

                // recalculate sizes
                horizontalRadius += horizontalStep;
                horizontalRadius = horizontalRadius > Surface.Width ? Surface.Width : horizontalRadius;
                verticalRadius += 1;

                // create a larger rectangle
                if (verticalRadius <= halfHeight)
                    rectangle = new(Surface.Area.Center, (int)horizontalRadius, verticalRadius);
                else
                    return true;
            }
            return false;
        })
        .Code(() =>
        {
            IsVisible = false;
        });

        instructions.Finished += InstructionSet_OnFinished;
        SadComponents.Add(instructions);
    }

    public void DrawRectangle(Rectangle rectangle, Color color)
    {
        // clear everything with black color
        Surface.DefaultBackground = _backgroundColor;
        Surface.Clear();

        // erase the center rectangle with transparent color
        Surface.DefaultBackground = Color.Transparent;
        Surface.Clear(rectangle);

        // draw rectangle
        Surface.DrawRectangle(rectangle, color, _backgroundColor);
    }

    void InstructionSet_OnFinished(object? o, EventArgs e)
    {
        Finished?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler? Finished;
}