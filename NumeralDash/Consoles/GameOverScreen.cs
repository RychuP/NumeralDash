using System;
using SadConsole;
using NumeralDash.Other;

namespace NumeralDash.Consoles
{
    class GameOverScreen : ScreenSurface
    {
        readonly TheDrawFont _drawFont;
        public bool IsShown { get; set; }

        public GameOverScreen(int width, int height, TheDrawFont drawFont) : base(width, height)
        {
            _drawFont = drawFont;

            // print the game name
            Surface.PrintDraw(5, "game", _drawFont, HorizontalAlignment.Center);
            Surface.PrintDraw(12, "over", _drawFont, HorizontalAlignment.Center);
            Surface.PrintCenter(26, "Press Enter to try again...");
        }

        public void DisplayStats(int level, TimeSpan timePlayed)
        {
            Surface.PrintCenter(20, $"You have reached level {level}. Well done.");
            Surface.PrintCenter(22, $"Total gameplay time: {timePlayed}");
            IsShown = true;
        }
    }
}
