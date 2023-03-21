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
}

public static class KeyboardExtensions
{
    public static bool HasKeysReleased(this Keyboard k) =>
        k.KeysReleased.Count > 0;

    public static bool HasDirectionKeyDown(this Keyboard k) =>
        k.HasKeysDown && 
        (k.IsKeyDown(Keys.Down) || k.IsKeyDown(Keys.Up) ||
        k.IsKeyDown(Keys.Left) || k.IsKeyDown(Keys.Right));

    public static Direction GetDirFromKeysDown(this Keyboard k) =>
        k.IsKeyDown(Keys.Left) ? Direction.Left :
        k.IsKeyDown(Keys.Right) ? Direction.Right :
        k.IsKeyDown(Keys.Up) ? Direction.Up :
        k.IsKeyDown(Keys.Down) ? Direction.Down :
        Direction.None;

    public static Direction GetDirection(this Keyboard k) =>
        k.IsKeyPressed(Keys.Left) ? Direction.Left :
        k.IsKeyPressed(Keys.Right) ? Direction.Right :
        k.IsKeyPressed(Keys.Up) ? Direction.Up :
        k.IsKeyPressed(Keys.Down) ? Direction.Down :
        Direction.None;
}