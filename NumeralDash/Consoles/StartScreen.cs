using System;
using SadConsole;
using NumeralDash.Other;

namespace NumeralDash.Consoles
{
    class StartScreen : ScreenSurface
    {
        readonly TheDrawFont _drawFont;

        public bool Finished { get; set; } = false;

        public StartScreen(int width, int height, TheDrawFont drawFont) : base(width, height)
        {
            _drawFont = drawFont;

            // print the game name
            Surface.PrintDraw(5, "numeral", _drawFont, HorizontalAlignment.Center);
            Surface.PrintDraw(12, "dash", _drawFont, HorizontalAlignment.Center);

            // print info
            Surface.PrintCenter(20, "Collect all numbers scattered around the dungeon in the given order");
            Surface.PrintCenter(22, "and leave before the time runs out.");
            Surface.PrintCenter(26, "Controls: Arrow buttons to move, F5 toggle full screen");
            Surface.PrintCenter(28, "Press Enter to start...");
        }
    }
}
