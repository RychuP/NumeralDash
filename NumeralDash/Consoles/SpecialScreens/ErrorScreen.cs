namespace NumeralDash.Consoles.SpecialScreens;

class ErrorScreen : SpecialScreen
{
    public ErrorScreen(int width, int height) : base(width, height, "internal", "error")
    {
        Print(0, "Map failed to generate too many times. Algorithm is not perfect.");
        Print(2, "It doesn't happen very often, but sometimes it does.");
        Print(5, "Your progress is not lost.");
        Print(8, $"Press {Enter} to try again...");
    }
}