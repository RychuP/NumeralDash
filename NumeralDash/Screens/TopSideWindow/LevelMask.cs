using SadConsole.Instructions;

namespace NumeralDash.Screens.TopSideWindow;

// animated level number mask covering stats display during rectangle transitions
class LevelMask : ScreenSurface
{
    readonly string[] _numberFileNames =
    {
        "one",
        "two",
        "three",
        "four",
    };

    readonly LevelNumber[] _numbers;
    readonly RaysAnim _rayAnim = new (27, 20);
    int _level;

    public LevelMask() : base(StatsDisplay.Width, StatsDisplay.Height)
    {
        IsVisible = false;
        Position = (Program.Width - Surface.Width - 1, 1);

        // create level numbers
        _numbers = new LevelNumber[_numberFileNames.Length];
        for (int i = 0; i < _numberFileNames.Length; i++)
            _numbers[i] = new LevelNumber(_numberFileNames[i], 14, 12);

        // add ray anim to children
        Children.Add(_rayAnim);
    }

    void Animate(int level)
    {
        var levelNumber = level >= 0 && level < _numberFileNames.Length ? _numbers[level] : _numbers[0];
        Children.Add(levelNumber);
        levelNumber.Reset();
        _rayAnim.Reset();

        var instructions = new InstructionSet() { RemoveOnFinished = true }
            .Code((o, t) => levelNumber.Animate())
            .Code((o, t) => _rayAnim.Animate(t));

        SadComponents.Add(instructions);
    }

    void AnimateRays()
    {
        SadComponents.Clear();
        _rayAnim.Reset();

        var instructions = new InstructionSet() { RemoveOnFinished = true }
            .Wait(TimeSpan.FromMilliseconds(300))
            .Code((o, t) => _rayAnim.Animate(t));

        SadComponents.Add(instructions);
    }

    void Transition_OnStarted(object? o, TransitionEventArgs e)
    {
        switch (e.Type)
        {
            case TransitionTypes.GameStart:
                IsVisible = true;
                Animate(_level);
                break;

            case TransitionTypes.LevelChange:
                IsVisible = true;
                Animate(_level + 1);
                break;
        }
    }

    void Transition_OnMidPointReached(object? o, TransitionEventArgs e)
    {
        switch (e.Type)
        {
            case TransitionTypes.GameStart:
                AnimateRays();
                break;

            case TransitionTypes.LevelChange:
                AnimateRays();
                break;
        }
    }

    void Transition_OnFinished(object? o, TransitionEventArgs e)
    {
        switch (e.Type)
        {
            case TransitionTypes.GameStart:
                IsVisible = false;
                break;

            case TransitionTypes.LevelChange:
                IsVisible = false;
                break;
        }
    }

    protected override void OnVisibleChanged()
    {
        if (IsVisible)
            Children.Add(_rayAnim);
        else
            Children.Clear();
        base.OnVisibleChanged();
    }

    protected override void OnParentChanged(IScreenObject oldParent, IScreenObject newParent)
    {
        if (newParent is GameManager gm)
        {
            gm.Transition.Started += Transition_OnStarted;
            gm.Transition.MidPointReached += Transition_OnMidPointReached;
            gm.Transition.Finished += Transition_OnFinished;
            gm.Dungeon.LevelChanged += (o, e) =>
            {
                _level = e.Level;
            };
        }
        base.OnParentChanged(oldParent, newParent);
    }
}