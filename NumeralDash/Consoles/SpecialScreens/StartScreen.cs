using System;
using SadConsole;
using NumeralDash.Other;

namespace NumeralDash.Consoles.SpecialScreens
{
    class StartScreen : SpecialScreen
    {
        public StartScreen(int width, int height, TheDrawFont drawFont) : base(width, height, "numeral", "dash", drawFont)
        {
            Surface.PrintCenter(_textRow, "Collect all numbers scattered around the dungeon in the given order");
            Surface.PrintCenter(_textRow + 2, "and leave before the time runs out.");
            Surface.PrintCenter(_textRow + 5, "Arrow keys to move (fast with left shift), F5 toggle full screen.");
            Surface.PrintCenter(_textRow + 8, "Press Enter to start...");
            IsVisible = true;
        }
    }
}
