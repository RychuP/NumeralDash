using SadConsole.Input;
using System.Linq;

namespace NumeralDash;

public static class Extensions
{
    /// <summary>
    /// Allows adding multiple screen objects at the same time.
    /// </summary>
    public static void Add(this ScreenObjectCollection collection, params IScreenObject[] childrenList) =>
        Array.ForEach(childrenList, child => collection.Add(child));

    public static Keys ToKey(this Direction d) => d.Type switch
    {
        Direction.Types.Left => Keys.Left,
        Direction.Types.Right => Keys.Right,
        Direction.Types.Up => Keys.Up,
        _ => Keys.Down
    };
}

public static class SurfaceExtensions
{
    public static void PrintCenter(this ICellSurface c, int y, string text) =>
    c.Print(0, y, text.Align(HorizontalAlignment.Center, c.Width));

    public static void PrintCenter(this ICellSurface c, int y, ColoredString text)
    {
        c.Clear(0, y, c.Width);
        int x = (c.Width - text.Length) / 2;
        c.Print(x, y, text);
    }

    /// <summary>
    /// Draws a rectangle around the perimeter of the <see cref="ICellSurface.Area"/>.
    /// </summary>
    /// <param name="fg">Foreground <see cref="Color"/>.</param>
    /// <param name="glyph">Glyph to use as an outline.</param>
    public static void DrawOutline(this ICellSurface cellSurface, Color? fg = null, Color? bg = null, int? glyph = null) =>
        cellSurface.DrawRectangle(cellSurface.Area, fg, bg, glyph);

    /// <summary>
    /// Draws a rectangle outline using either <see cref="ICellSurface.ConnectedLineThin"/> or the given glyph.
    /// </summary>
    /// <param name="rectangle"><see cref="Rectangle"/> to draw.</param>
    /// <param name="glyph">Glyph to use as an outline.</param>
    /// <param name="fg">Foreground <see cref="Color"/>.</param>
    public static void DrawRectangle(this ICellSurface cellSurface, Rectangle rectangle, 
        Color? fg = null, Color? bg = null, int? glyph = null)
    {
        var style = (glyph.HasValue) ?
            ShapeParameters.CreateStyledBox(ICellSurface.CreateLine(glyph.Value),
                new ColoredGlyph(fg ?? Color.White, bg ?? Color.Transparent)) :
            ShapeParameters.CreateStyledBox(ICellSurface.ConnectedLineThin,
                new ColoredGlyph(fg ?? Color.White, bg ?? Color.Transparent));
        cellSurface.DrawBox(rectangle, style);
    }
}

public static class KeyboardExtensions
{
    public static bool HasKeysReleased(this Keyboard k) =>
        k.KeysReleased.Count > 0;

    public static Direction GetDirection(this Keyboard k) =>
        k.IsKeyPressed(Keys.Left) ? Direction.Left :
        k.IsKeyPressed(Keys.Right) ? Direction.Right :
        k.IsKeyPressed(Keys.Up) ? Direction.Up :
        k.IsKeyPressed(Keys.Down) ? Direction.Down :
        Direction.None;
}