using NumeralDash.Screens.TopSideWindow;
using SadConsole.Instructions;
using System.Collections.Generic;

namespace NumeralDash.Screens;

class Transition : ScreenSurface
{
    readonly Color _backgroundColor = Color.Black;
    bool _isPlaying = false;

    readonly Dictionary<TransitionTypes, Color> _colors = new()
    {
        { TransitionTypes.GameStart, Color.LightBlue },
        { TransitionTypes.LevelChange, Color.Pink },
        { TransitionTypes.GameOver, Color.Crimson },
    };

    public Transition() : base(Program.Width - StatsDisplay.Width - 3, Program.Height - 2)
    {
        Position = (1, 1);
        IsVisible = false;
    }

    public TransitionTypes Type { get; private set; } = TransitionTypes.GameStart;

    public void Play(TransitionTypes type)
    {
        if (_isPlaying) 
            return;
        else
        {
            _isPlaying = true;
            Type = type;
            OnStarted();
        }
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

    void OnStarted()
    {
        Surface.Clear();
        IsVisible = true;

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
                DrawRectangle(rectangle, _colors[Type]);

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
            OnReachedMidPoint();
            Sounds.Level.Play();
        })
        .Code((o, t) =>
        {
            delta += t;
            if (delta >= speed)
            {
                delta = TimeSpan.Zero;

                // draw rectangle
                DrawRectangle(rectangle, _colors[Type]);

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
        
        instructions.Finished += (o, e) => OnFinished();
        SadComponents.Add(instructions);

        var args = new TransitionEventArgs(Type);
        Started?.Invoke(this, args);
    }

    void OnReachedMidPoint()
    {
        var args = new TransitionEventArgs(Type);
        MidPointReached?.Invoke(this, args);
    }

    void OnFinished()
    {
        _isPlaying = false;
        var args = new TransitionEventArgs(Type);
        Finished?.Invoke(this, args);
    }

    public event EventHandler<TransitionEventArgs>? Started;
    public event EventHandler<TransitionEventArgs>? MidPointReached;
    public event EventHandler<TransitionEventArgs>? Finished;
}