namespace NumeralDash.Screens.TopSideWindow;

// static ansi graphics mask covering stats display outside gameplay
class AnsiMask : ScreenSurface
{
    readonly AnsiGraphics[] _cities =
    {
        new("neworleans", "Jacques IMO New Orleans"),
        new("shangrila", "Shangri-La Bridge"),
        new("montmartre", "Montmartre"),
        new("goldcoast", "Gold Coast"),
        new("berlin", "Berlin Wall"),
        new("lisbon", "Lisbon"),
        new("hoian", "Hoi An"),
    };

    AnsiGraphics _currentAnsi;

    public AnsiMask() : base(StatsDisplay.Width, StatsDisplay.Height)
    {
        IsVisible = false;
        Position = (Program.Width - Surface.Width - 1, 1);

        Surface.DefaultBackground = Color.Black;
        Surface.Clear();
        foreach (var ansi in _cities)
            Children.Add(ansi);

        _currentAnsi = _cities[Program.GetRandomIndex(_cities.Length)];
        _currentAnsi.IsVisible = true;
    }

    public string Description =>
        _currentAnsi.Description;


    void Transition_OnFinished(object? o, TransitionEventArgs e)
    {
        if (e.Type == TransitionTypes.GameOver)
            IsVisible = true;
    }

    protected override void OnVisibleChanged()
    {
        if (IsVisible)
        {
            _currentAnsi.IsVisible = false;
            AnsiGraphics ansi;
            do ansi = _cities[Program.GetRandomIndex(_cities.Length)];
            while (ansi == _currentAnsi);
            ansi.IsVisible = true;
            _currentAnsi = ansi;
        }
        base.OnVisibleChanged();
    }

    protected override void OnParentChanged(IScreenObject oldParent, IScreenObject newParent)
    {
        if (newParent is GameManager gm)
        {
            gm.Transition.Started += (o, e) => IsVisible = false;
            gm.Transition.Finished += Transition_OnFinished;
            gm.StartScreenShown += (o, e) => IsVisible = true;
            gm.Dungeon.GameOver += (o, e) => IsVisible = true;
        }
        base.OnParentChanged(oldParent, newParent);
    }
}