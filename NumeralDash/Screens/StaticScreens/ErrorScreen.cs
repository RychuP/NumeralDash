namespace NumeralDash.Screens.StaticScreens;

class ErrorScreen : StaticScreen
{
    public ErrorScreen() : base("internal", "error")
    {
        Print(0, "Map failed to generate too many times. Algorithm is not perfect.");
        Print(2, "It doesn't happen very often, but sometimes it does.");
        Print(5, "Your progress is not lost.");
        Print(8, $"Press {Enter} to try again...");
    }
}