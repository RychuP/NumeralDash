using System;
using SadConsole;
using NumeralDash.Other;

namespace NumeralDash.Consoles.SpecialScreens
{
    class GameOverScreen : SpecialScreen
    {
        public GameOverScreen(int width, int height, TheDrawFont drawFont) : base(width, height, "game", "over", drawFont)
        {
            Surface.PrintCenter(_textRow + 6, "Press Enter to try again...");
        }

        public void DisplayStats(int level, TimeSpan timePlayed)
        {
            Surface.PrintCenter(20, $"You have reached level {level}. Well done.");
            Surface.PrintCenter(22, $"Total gameplay time: {timePlayed}");
            IsBeingShown = true;
        }
    }
}
