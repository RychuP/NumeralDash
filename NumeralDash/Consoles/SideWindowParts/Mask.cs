using SadConsole.Ansi;
using System.IO;
using System.Linq;

namespace NumeralDash.Consoles.SideWindowParts;

class Mask : ScreenSurface
{
    readonly Ansi[] _cities =
    {
        new("neworleans", "Jacques IMO New Orleans"),
        new("shangrila", "Shangri-La Bridge"),
        new("montmartre", "Montmartre"),
        new("goldcoast", "Gold Coast"),
        new("berlin", "Berlin Wall"),
        new("lisbon", "Lisbon"),
        new("hoian", "Hoi An"),
    };

    Ansi _currentAnsi;

    public Mask(int width, int height) : base(width, height)
    {
        Surface.DefaultBackground = Color.Black;
        Surface.Clear();
        foreach (var ansi in _cities)
            Children.Add(ansi);

        _currentAnsi = _cities[Program.GetRandomIndex(_cities.Length)];
        _currentAnsi.IsVisible = true;
    }

    public string Description =>
        _currentAnsi.Description;

    protected override void OnVisibleChanged()
    {
        if (IsVisible)
        {
            _currentAnsi.IsVisible = false;
            Ansi ansi;
            do ansi = _cities[Program.GetRandomIndex(_cities.Length)];
            while (ansi == _currentAnsi);
            ansi.IsVisible = true;
            _currentAnsi = ansi;
        }
        base.OnVisibleChanged();
    }
}

class Ansi : ScreenSurface
{
    public string Description { get; init; }
    public Ansi(string fileName, string description) : base(54, 41, 54, 47)
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