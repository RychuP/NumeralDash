using SadConsole.Instructions;

namespace NumeralDash.Screens.TopSideWindow;

// animated level number mask covering stats display during rectangle transitions
class LevelMask : ScreenSurface
{
    readonly LevelNumber[] _numbers = new LevelNumber[10];
    readonly RaysAnim _rayAnim = new (27, 20);
    int _level;

    public LevelMask() : base(StatsDisplay.Width, StatsDisplay.Height)
    {
        IsVisible = false;
        Position = (Program.Width - Surface.Width - 1, 1);

        // create level numbers
        for (int i = 0; i < 10; i++)
            _numbers[i] = new LevelNumber(i);

        // add ray anim to children
        Children.Add(_rayAnim);
    }

    void Animate(int level)
    {
        string text = level.ToString();
        var instructions = new InstructionSet() { RemoveOnFinished = true };
        if (text.Length == 1)
        {
            var levelNumber = _numbers[level];
            Children.Add(levelNumber);
            levelNumber.Reset();

            instructions = instructions.Code((o, t) => levelNumber.Animate())
            .Code((o, t) => _rayAnim.Animate(t))
            .Wait(TimeSpan.FromMilliseconds(300))
            .Code((o, t) => _rayAnim.Animate(t));
        }
        else if (text.Length == 2)
        {
            var firstNumber = _numbers[text[0] - 48];
            var secondNumber = _numbers[text[1] - 48];
            Children.Add(firstNumber);
            firstNumber.Reset();

            instructions = instructions.Code((o, t) => firstNumber.Animate())
            .Code((o, t) => _rayAnim.Animate(t))
            .Code(() => { 
                Children.Remove(firstNumber); 
                Children.Add(secondNumber); 
                secondNumber.Reset(); 
            })
            .Code((o, t) => secondNumber.Animate())
            .Code((o, t) => _rayAnim.Animate(t));
        }

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
            gm.Transition.Finished += Transition_OnFinished;
            gm.Dungeon.LevelChanged += (o, e) =>
            {
                _level = e.Level + 1;
            };
        }
        base.OnParentChanged(oldParent, newParent);
    }
}