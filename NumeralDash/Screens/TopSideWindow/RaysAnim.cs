using System.IO;
using System.Linq;

namespace NumeralDash.Screens.TopSideWindow;

// outer section of the level number animation showing rays going away from the number in the center
class RaysAnim : ScreenSurface
{
    const char Dot = '.';
    const char Glyph = '*'; //(char)254;
    const int RayCount = 18;
    const int RayLength = 3;
    readonly Point[,] _rays = new Point[RayCount, RayLength];
    int _stage = 0, _currentPoint = 0;
    readonly TimeSpan _speed = TimeSpan.FromMilliseconds(100);
    TimeSpan _delta = TimeSpan.Zero;

    public RaysAnim(int width, int height) : base(width, height)
    {
        Surface.DefaultBackground = Color.Black;
        Surface.DefaultForeground = Color.Yellow;
        Surface.Clear();

        // read the blueprint file
        var path = Path.Combine("Resources", "Text", "rays.txt");
        var text = File.ReadAllText(path);
        var eol = text.Contains("\r\n") ? "\r\n" : "\n";
        var lines = text.Split(eol);

        // check the lines
        if (lines.Length != height || lines.Any(l => l.Length != width))
            throw new ArgumentException("Text file width or height invalid.");

        // point areas that we will read perimeter positions from
        Rectangle[] pointAreas =
        {
            new Rectangle(4, 2, width - 8, height - 4),
            new Rectangle(2, 1, width - 4, height - 2),
            new Rectangle(0, 0, width, height),
        };

        // transfer dots from the text file to rays array as points
        int ray = 0;
        int rayPoint = 0;
        foreach (var area in pointAreas)
        {
            var perimeter = area.PerimeterPositions();
            foreach (var point in perimeter)
            {
                // if the point is a dot in the text file add it to a ray
                var (x, y) = point;
                if (lines[y][x] == Dot)
                    _rays[ray++, rayPoint] = point;
            }

            // reset pointers
            ray = 0;
            rayPoint++;
        }
    }

    public void Reset()
    {
        Surface.Clear();
        _stage = _currentPoint = 0;
        _delta = _speed;
    }

    public bool Animate(TimeSpan delta)
    {
        _delta += delta;
        if (_delta >= _speed)
            _delta = TimeSpan.Zero;
        else
            return false;

        char glyph = _stage == 0 ? Glyph : (char)0;
        for (int i = 0; i < RayCount; i++)
            Surface.Print(_rays[i, _currentPoint], glyph);
        if (++_currentPoint == RayLength)
        {
            if (++_stage == 2)
                return true;
            else
                _currentPoint = 0;
        }
        return false;
    }
}