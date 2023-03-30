namespace NumeralDash.Screens.StaticScreens;

class StartScreen : StaticScreen
{
    bool _showingDescription = true;

    public StartScreen() : base("numeral", "dash")
    {
        IsVisible = true;
        Print(9, $"Press {Enter} to start.");
    }

    void PrintDescription()
    {
        Print(0, "Collect all numbers scattered around the map in the given order");
        Print(2, "and leave before the time runs out.");
        Print(4, "                                                                                    ");
        Print(7, $"Press {Space} for game controls.");
    }

    void PrintControls()
    {
        Print(0, $"{Green("Arrow Keys")} move, {Green("F5")} toggle full screen, {Esc} pause or exit,");
        Print(2, $"{Green("Left Shift")} auto move stopping at numbers and the exit,");
        Print(4, $"{Green("Left Ctrl")} auto move ignoring numbers and stopping at road intersections.");
        Print(7, $"Press {Space} for game description.");
    }

    public void ToggleText()
    {
        if (_showingDescription)
            PrintControls();
        else
            PrintDescription();
        _showingDescription = !_showingDescription;
    }

    protected override void OnVisibleChanged()
    {
        if (IsVisible)
        {
            _showingDescription = true;
            PrintDescription();
        }
        base.OnVisibleChanged();
    }

    protected override void OnParentChanged(IScreenObject oldParent, IScreenObject newParent)
    {
        if (newParent is GameManager gm)
        {
            gm.Transition.MidPointReached += (o, e) =>
            {
                if (e.Type == TransitionTypes.GameStart)
                    IsVisible = false;
            };
        }
        base.OnParentChanged(oldParent, newParent);
    }
}