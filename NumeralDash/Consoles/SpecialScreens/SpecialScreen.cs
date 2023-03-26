namespace NumeralDash.Consoles.SpecialScreens;

class SpecialScreen : ScreenSurface
{
    const int TextRow = 20;
    static protected string Enter => Green("Enter");
    static protected string Space => Green("Space");
    static protected string Esc => Green("Esc");

    public SpecialScreen(int width, int height, string topText, string bottomText) : base(width, height)
    {
        Surface.PrintTheDraw(5, topText, Fonts.Destruct, HorizontalAlignment.Center);
        Surface.PrintTheDraw(12, bottomText, Fonts.Destruct, HorizontalAlignment.Center);

        Stars = new StarsEffect(width, height);
        Children.Add(Stars);

        IsVisible = false;
    }

    public StarsEffect Stars { get; init; }

    protected void Print(int deltaY, string text)
    {
        var coloredText = ColoredString.Parser.Parse(text);
        Surface.Print(TextRow + deltaY, coloredText);
    }

    static string Recolor(string text, string color) =>
        $"[c:r f:{color}]{text}[c:undo]";

    static protected string Green(object text) =>
        Recolor(text?.ToString() ?? "", "lightgreen");

    static protected string Orange(object text) =>
        Recolor(text?.ToString() ?? "", "orange");
}