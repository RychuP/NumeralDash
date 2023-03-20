using SadConsole.Readers;

namespace NumeralDash.Consoles.SpecialScreens;

class SpecialScreen : ScreenSurface
{
    protected const int TextRow = 20;

    public SpecialScreen(int width, int height, string top, string bottom, TheDrawFont drawFont) : base(width, height)
    {
        IsVisible = false;

        // print the title
        Surface.PrintTheDraw(5, top, drawFont, HorizontalAlignment.Center);
        Surface.PrintTheDraw(12, bottom, drawFont, HorizontalAlignment.Center);
    }
}