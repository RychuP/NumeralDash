using System;
using SadConsole;
using NumeralDash.Other;

namespace NumeralDash.Consoles.SpecialScreens
{
    class SpecialScreen : ScreenSurface
    {
        protected readonly TheDrawFont _drawFont;
        protected const int _textRow = 20;

        public bool IsBeingShown { get; set; }

        public SpecialScreen(int width, int height, string top, string bottom, TheDrawFont drawFont) : base(width, height)
        {
            _drawFont = drawFont;

            // print the game name
            Surface.PrintDraw(5, top, _drawFont, HorizontalAlignment.Center);
            Surface.PrintDraw(12, bottom, _drawFont, HorizontalAlignment.Center);
        }
    }
}
