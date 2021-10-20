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

        /// <summary>
        /// Used to generate colors that are of a certain, minimum brightness.
        /// </summary>
        public const float MinimumColorBrightness = 0.5f;

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
            var sc = Game.Instance.StartingConsole;
            string problem = "There has been a problem with";

            try
            {
                var gm = new GameManager(Width, Height);
            }
            catch (FontLoadingException e)
            {
                sc.Print(2, 2, $"{problem} loading the font file {e.Message}");
            }
            catch
            {
                sc.Print(2, 2, $"{problem} starting the game...");
            }
        }

        /// <summary>
        /// Returns a random color.
        /// </summary>
        /// <returns></returns>
        public static Color GetRandomColor() => Color.White.GetRandomColor(Game.Instance.Random);

        /// <summary>
        /// Returns a random index (between 0 and count - 1).
        /// </summary>
        /// <param name="count">Size of the collection.</param>
        public static int GetRandomIndex(int count) => Game.Instance.Random.Next(0, count);
    }
}
