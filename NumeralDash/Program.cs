using System;
using SadConsole;
using SadRogue.Primitives;
using Console = SadConsole.Console;
using NumeralDash.Consoles;

namespace NumeralDash
{
    class Program
    {
        static int Width = 120, Height;

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

            // Setup the engine and create the main window.
            Game.Create(Width, Height);

            // Hook the start event so we can add consoles to the system.
            Game.Instance.OnStart += Init;

            // Start the game.
            Game.Instance.Run();
            Game.Instance.Dispose();
        }

        static void Init()
        {
            Game.Instance.ToggleFullScreen();
            Game.Instance.LoadFont(@"Fonts/C64.font");
            
            var gm = new GameManager(Width, Height);
        }

        // returns a random color
        public static Color GetRandomColor() => Color.White.GetRandomColor(Game.Instance.Random);
    }
}
