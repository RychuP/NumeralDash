namespace NumeralDash.Screens.StaticScreens;

class GameOverScreen : StaticScreen
{
    public GameOverScreen() : base("game", "over")
    {
        Print(8, $"Press {Enter} to try again.");
    }

    public void DisplayStats(int level, int score, TimeSpan timePlayed)
    {
        IsVisible = true;
        Print(0, $"You have reached level {Orange(level + 1)}");
        Print(2, $"Total gameplay time: {Green(timePlayed)}");
        Print(4, $"Score: {Green(score)}");
    }
}