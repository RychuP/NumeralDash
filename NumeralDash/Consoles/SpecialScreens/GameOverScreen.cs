namespace NumeralDash.Consoles.SpecialScreens;

class GameOverScreen : SpecialScreen
{
    public GameOverScreen(int width, int height) : base(width, height, "game", "over")
    {
        Print(8, $"Press {Enter} to try again.");
    }

    public void DisplayStats(int level, int score, TimeSpan timePlayed)
    {
        Print(0, $"You have reached level {Orange(level + 1)}");
        Print(2, $"Total gameplay time: {Green(timePlayed)}");
        Print(4, $"Score: {Green(score)}");
        IsVisible = true;
    }
}