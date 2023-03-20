namespace NumeralDash.Consoles.SpecialScreens;

class ErrorScreen : SpecialScreen
{
    public ErrorScreen(int width, int height) : base(width, height, "internal", "error")
    {
        Surface.PrintCenter(TextRow, "Map failed to generate too many times. Algorithm is not perfect.");
        Surface.PrintCenter(TextRow + 2, "It doesn't happen very often, but sometimes it does.");
        Surface.PrintCenter(TextRow + 5, "Your progress is not lost.");
        Surface.PrintCenter(TextRow + 8, "Press Enter to try again...");
    }
}