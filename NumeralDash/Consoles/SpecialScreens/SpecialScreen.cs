namespace NumeralDash.Consoles.SpecialScreens;

class SpecialScreen : ScreenSurface
{
    const int TextRow = 20;
    static protected string Enter => Violet("Enter");
    static protected string Esc => Green("Esc");

    public SpecialScreen(int width, int height, string topText, string bottomText) : base(width, height)
    {
        IsVisible = false;

        // print the title
        Surface.PrintTheDraw(5, topText, Fonts.Destruct, HorizontalAlignment.Center);
        Surface.PrintTheDraw(12, bottomText, Fonts.Destruct, HorizontalAlignment.Center);
    }

    protected void Print(int deltaY, string text)
    {
        var coloredText = ColoredString.Parser.Parse(text);
        Surface.PrintCenter(TextRow + deltaY, coloredText);
    }

    static string Recolor(string text, string color) =>
        $"[c:r f:{color}]{text}[c:undo]";

    static protected string Green(object text) =>
        Recolor(text?.ToString() ?? "", "lightgreen");

    static protected string Orange(object text) =>
        Recolor(text?.ToString() ?? "", "orange");

    static protected string Violet(object text) =>
        Recolor(text?.ToString() ?? "", "violet");

}