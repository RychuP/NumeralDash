global using System;
global using SadConsole;
global using SadRogue.Primitives;
global using Console = SadConsole.Console;
using NumeralDash.Consoles;

namespace NumeralDash;

class Program
{
    static int Width = 120, Height;
    static readonly bool s_startFullScreen = false;

    /// <summary>
    /// Used to generate colors that are of a certain, minimum brightness.
    /// </summary>
    public const float MinimumColorBrightness = 0.5f;

    public const string Version = "0.6.5";

    static void Main(string[] args)
    {
        // calculate the cell count for the Game.Height which will fill the full screen with the given Width and font size
        Point fontSize = (8, 16);
        int pixelWidth = Width * fontSize.X,
            pixelHeight = pixelWidth * 9 / 16;
        Height = (int)Math.Round((decimal)(pixelHeight / fontSize.Y)) + 1;

        // set title and resize mode
        Settings.WindowTitle = "Numeral Dash";
        Settings.ResizeMode = Settings.WindowResizeOptions.Fit;

        // setup the engine and create the main window.
        Game.Create(Width, Height);

        // hook the start event so we can add consoles to the system.
        Game.Instance.OnStart += Init;

        // reduce repeat delay
        Game.Instance.Keyboard.InitialRepeatDelay = 0.2f;

        // start the game
        Game.Instance.Run();
        Game.Instance.Dispose();
    }

    static void Init()
    {
        if (s_startFullScreen) Game.Instance.ToggleFullScreen();
        var sc = Game.Instance.StartingConsole;

        try
        {
            _ = new GameManager(Width, Height);
        }
        catch
        {
            sc.Print(2, 2, $"There has been a problem with starting the game... " +
                $"Press Alt + F4 to close the game.");
        }

        Game.Instance.DestroyDefaultStartingConsole();
    }

    /// <summary>
    /// Returns a random color.
    /// </summary>
    /// <returns></returns>
    public static Color GetRandomColor() => 
        Color.White.GetRandomColor(Game.Instance.Random);

    /// <summary>
    /// Returns a random index (between 0 and count - 1).
    /// </summary>
    /// <param name="count">Size of the collection.</param>
    public static int GetRandomIndex(int count) => 
        Game.Instance.Random.Next(0, count);
}
