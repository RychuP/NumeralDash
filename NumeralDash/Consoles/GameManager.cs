using System;
using System.Linq;
using SadConsole;
using NumeralDash.Other;
using SadRogue.Primitives;
using SadConsole.Input;
using NumeralDash.World;

namespace NumeralDash.Consoles
{
    /// <summary>
    /// Inititates the three main consoles and manages the game in general terms (start screen, full screen toggle, etc).
    /// </summary>
    class GameManager : SadConsole.Console
    {
        const int sideWindowWidth = 27,        // keep this number odd to allow dungeon view fit snugly in the dungeon window
                  sideWindowHeight = 22,
                  twoBorders = 2,
                  oneBorder = 1;

        // border style around windows
        readonly ColoredGlyph _borderGlyph = new(Color.Green, Color.Black, 177);

        // main consoles
        readonly Dungeon _dungeon;
        readonly SideWindow _sideWindow;
        readonly MiniMap _miniMap;

        // other screens
        readonly StartScreen _startScreen;
        readonly GameOverScreen _gameOverScreen;
        readonly TheDrawFont _drawFont;

        public GameManager(int width, int height) : base(width, height)
        {
            #region Dungeon Initialization

            // calculate dungeon window size (substract oneBorder only because another border is shared with neigbouring windows)
            Point dungeonWindowSize = (width - sideWindowWidth - oneBorder, height);
            Point dungeonPosition = (0, 0);
            Rectangle window = new(dungeonPosition.X, dungeonPosition.Y, dungeonWindowSize.X, dungeonWindowSize.Y);

            // create a dungeon (devide the window width by two to allow for the size of the C64 font 16x16 compared to the default 8x16)
            _dungeon = new(dungeonWindowSize.X / 2 - twoBorders, dungeonWindowSize.Y - twoBorders, new Map())
            {
                Position = (dungeonPosition.X + oneBorder, dungeonPosition.Y + oneBorder)
            };

            // do this just to draw the border
            AddConsole(window, _dungeon);

            // remove it to make space for the start screen
            Children.Remove(_dungeon);

            _dungeon.GameOver += OnGameOver;

            #endregion Dungeon Initialization

            #region Side Window Initialization

            // calculate side window size
            Point sideWindowSize = (sideWindowWidth + twoBorders, sideWindowHeight);
            Point sideWindowPosition = (dungeonWindowSize.X - oneBorder, 0);
            window = new(sideWindowPosition.X, sideWindowPosition.Y, sideWindowSize.X, sideWindowSize.Y);

            // create a side window (substractions and additions make allowance for the border)
            _sideWindow = new(sideWindowWidth, sideWindowSize.Y - twoBorders, _dungeon)
            {
                Position = (sideWindowPosition.X + oneBorder, sideWindowPosition.Y + oneBorder)
            };

            // add a new child and display it
            AddConsole(window, _sideWindow);

            #endregion Side Window Initialization

            #region Mini Map Initialization

            // calculate bottom window size
            Point miniMapSize = (sideWindowWidth + twoBorders, height - sideWindowHeight + oneBorder);
            Point miniMapPosition = (dungeonWindowSize.X - oneBorder, sideWindowHeight - oneBorder);
            window = new(miniMapPosition.X, miniMapPosition.Y, miniMapSize.X, miniMapSize.Y);

            // create a status console (substractions and additions make allowance for the border)
            _miniMap = new(miniMapSize.X - twoBorders, miniMapSize.Y, _dungeon)
            {
                Position = (miniMapPosition.X + oneBorder, miniMapPosition.Y + oneBorder)
            };

            // add a new child and display it
            AddConsole(window, _miniMap);

            #endregion Mini Map Initialization

            #region Start & End Screen Initialization

            // load the draw font
            string fontFileName = "DESTRUCX.TDF";
            var fontEnumerable = TheDrawFont.ReadFonts(@"Fonts/" + fontFileName);
            if (fontEnumerable is null) throw new FontLoadingException(fontFileName);
            _drawFont = fontEnumerable.ToArray()[3];

            // create screens
            _startScreen = new(Width - sideWindowWidth, Height, _drawFont);
            _gameOverScreen = new(Width - sideWindowWidth, Height, _drawFont);

            Children.Add(_startScreen);
            
            #endregion

            // connect borders
            this.ConnectLines();

            // replace starting console
            Game.Instance.Screen = this;
            Game.Instance.DestroyDefaultStartingConsole();
            IsFocused = true;
        }

        void AddConsole(Rectangle r, SadConsole.Console c)
        {
            // draw a border around the console
            this.DrawBox(r, ShapeParameters.CreateStyledBox(ICellSurface.ConnectedLineThick, _borderGlyph));

            // add the console to the display list
            Children.Add(c);
        }

        public override bool ProcessKeyboard(Keyboard keyboard)
        {
            if (keyboard.HasKeysPressed)
            {
                if (keyboard.IsKeyPressed(Keys.F5))
                {
                    Game.Instance.ToggleFullScreen();
                    return true;
                }

                if (!_startScreen.Finished && keyboard.IsKeyPressed(Keys.Enter)) {
                    _startScreen.Finished = true;
                    Children.Remove(_startScreen);
                    Children.Add(_dungeon);
                    _dungeon.Start();
                }
                else
                {
                    if (!_gameOverScreen.IsShown)
                    {
                        _dungeon.ProcessKeyboard(keyboard);
                    }
                    else if (keyboard.IsKeyPressed(Keys.Enter))
                    {
                        _gameOverScreen.IsShown = false;
                        Children.Remove(_gameOverScreen);
                        Children.Add(_dungeon);
                        _dungeon.Restart();
                    }
                }
            }
            return true;
        }

        void OnGameOver(int level, TimeSpan timePlayed)
        {
            Children.Remove(_dungeon);
            Children.Add(_gameOverScreen);
            _gameOverScreen.DisplayStats(level, timePlayed);
        }
    }

    public class FontLoadingException : Exception
    {
        public FontLoadingException(string message) : base(message) { }
    }
}