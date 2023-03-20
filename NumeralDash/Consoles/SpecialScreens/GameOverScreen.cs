namespace NumeralDash.Consoles.SpecialScreens;

class GameOverScreen : SpecialScreen
{
    public GameOverScreen(int width, int height) : base(width, height, "game", "over")
    {
        Print(6, $"Press {Enter} to try again...");
    }

    public void DisplayStats(int level, TimeSpan timePlayed)
    {
        Print(0, $"You have reached level {Orange(level)}. Well done.");
        Print(2, $"Total gameplay time: {Green(timePlayed)}");
        IsVisible = true;
    }
}