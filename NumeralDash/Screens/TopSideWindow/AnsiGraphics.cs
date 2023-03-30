using SadConsole.Ansi;
using System.IO;

namespace NumeralDash.Screens.TopSideWindow;

// ansi pictures displayed in the ansi mask surface
class AnsiGraphics : ScreenSurface
{
    public string Description { get; init; }
    public AnsiGraphics(string fileName, string description) : base(54, 41, 54, 47)
    {
        FontSize = (4, 8);
        UsePixelPositioning = true;
        Position = (1, -6);
        IsVisible = false;
        Description = description;
        string path = Path.Combine("Resources", "Ansi", $"{fileName}.ans");
        Document doc = new(path);
        AnsiWriter writer = new(doc, Surface);
        writer.ReadEntireDocument();
    }
}