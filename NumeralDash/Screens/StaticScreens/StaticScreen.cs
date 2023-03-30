using NumeralDash.Screens.TopSideWindow;

namespace NumeralDash.Screens.StaticScreens;

class StaticScreen : ScreenSurface
{
    const int TextRow = 20;
    static protected string Enter => Green("Enter");
    static protected string Space => Green("Space");
    static protected string Esc => Green("Esc");

    public StaticScreen(string topText, string bottomText) : 
        base(Program.Width - StatsDisplay.Width - 3, Program.Height - 2)
    {
        IsVisible = false;
        Position = (1, 1);

        Surface.PrintTheDraw(5, topText, Fonts.Destruct, HorizontalAlignment.Center);
        Surface.PrintTheDraw(12, bottomText, Fonts.Destruct, HorizontalAlignment.Center);

        Stars = new StarsEffect(Surface.Width, Surface.Height);
        Children.Add(Stars);
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

    protected override void OnParentChanged(IScreenObject oldParent, IScreenObject newParent)
    {
        if (newParent is GameManager gm)
            gm.Transition.Started += (o, e) => Stars.IsVisible = false;
        base.OnParentChanged(oldParent, newParent);
    }
}