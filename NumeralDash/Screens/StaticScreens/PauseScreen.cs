namespace NumeralDash.Screens.StaticScreens;

class PauseScreen : StaticScreen
{
    public PauseScreen() : base("game", "paused")
    {
        Print(0, $"Press {Enter} to resume.");
        Print(3, $"Press {Esc} to return back to the main menu.");
    }
}