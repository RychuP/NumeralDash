using System;
using SadConsole;
using NumeralDash.Other;
using SadRogue.Primitives;

namespace NumeralDash.Consoles.SpecialScreens
{
    class ErrorScreen : SpecialScreen
    {
        public ErrorScreen(int width, int height, TheDrawFont drawFont) : base(width, height, "internal", "error", drawFont)
        {
            Surface.PrintCenter(_textRow, "Map failed to generate too many times. Algorithm is not perfect.");
            Surface.PrintCenter(_textRow + 2, "It doesn't happen very often, but sometimes it does.");
            Surface.PrintCenter(_textRow + 5, "Your progress is not lost.");
            Surface.PrintCenter(_textRow + 8, "Press Enter to try again...");
        }
    }
}
