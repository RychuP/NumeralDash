namespace NumeralDash.Consoles.SpecialScreens;

class GameOverScreen : SpecialScreen
{
    public GameOverScreen(int width, int height) : base(width, height, "game", "over")
    {
        Surface.PrintCenter(TextRow + 6, "Press Enter to try again...");
    }

    public void DisplayStats(int level, TimeSpan timePlayed)
    {
        Surface.PrintCenter(TextRow, $"You have reached level {level}. Well done.");
        Surface.PrintCenter(TextRow + 2, $"Total gameplay time: {timePlayed}");
        IsVisible = true;
    }
}