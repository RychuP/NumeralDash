namespace NumeralDash.Consoles.SpecialScreens;

class SpecialScreen : ScreenSurface
{
    protected const int TextRow = 20;

    public SpecialScreen(int width, int height, string topText, string bottomText) : base(width, height)
    {
        IsVisible = false;

        // print the title
        Surface.PrintTheDraw(5, topText, Fonts.Destruct, HorizontalAlignment.Center);
        Surface.PrintTheDraw(12, bottomText, Fonts.Destruct, HorizontalAlignment.Center);
    }

    protected void Print(int deltaY, string text) =>
        Surface.PrintCenter(TextRow + deltaY, text);
}