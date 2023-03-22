global using System;
global using SadConsole;
global using SadRogue.Primitives;
global using Console = SadConsole.Console;
using NumeralDash.Consoles;

namespace NumeralDash;

class Program
{
    const int Width = 120, Height = 34;
    static readonly bool s_startFullScreen = false;

    /// <summary>
    /// Used to generate colors that are of a certain, minimum brightness.
    /// </summary>
    public const float MinimumColorBrightness = 0.5f;

    public const string Version = "0.7.4";

    static void Main(string[] args)
    {
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
        try
        {
            Game.Instance.Screen = new GameManager(Width, Height);
            Game.Instance.DestroyDefaultStartingConsole();
        }
        catch
        {
            Game.Instance.StartingConsole.Print(2, 2, $"There has been a problem " +
                $"with starting the game... Press Alt + F4 to close the game.");
        }
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