namespace NumeralDash.Consoles.SpecialScreens;

class PauseScreen : SpecialScreen
{
    public PauseScreen(int width, int height) : base(width, height, "game", "paused")
    {
        //Print(0, "Game is paused.");
        Print(0, $"Press {Enter} to resume.");
        Print(3, $"Press {Esc} to return back to the main menu.");
    }
}