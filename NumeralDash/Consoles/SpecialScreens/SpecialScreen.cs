using System;
using System.Linq;
using SadConsole;
using NumeralDash.Other;
using SadRogue.Primitives;

namespace NumeralDash.Consoles.SpecialScreens
{
    class SpecialScreen : ScreenSurface
    {
        protected readonly TheDrawFont _drawFont;
        protected const int _textRow = 20;

        public SpecialScreen(int width, int height, string top, string bottom, TheDrawFont drawFont) : base(width, height)
        {
            IsVisible = false;
            _drawFont = drawFont;

            // print the game name
            Surface.PrintDraw(5, top, _drawFont, HorizontalAlignment.Center);
            Surface.PrintDraw(12, bottom, _drawFont, HorizontalAlignment.Center);
        }
    }
}
