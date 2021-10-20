using System;
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
        readonly ColoredGlyph _borderGlyph;

        // main consoles
        readonly Dungeon _dungeon;
        readonly SideWindow _sideWindow;
        readonly MiniMap _miniMap;

        // draw font area
        ScreenSurface _startScreen;
        TheDrawFont _drawFont;

        bool _gameOver = false,
             _startScreenShown = false;

        public GameManager(int width, int height) : base(width, height)
        {
            _borderGlyph = new ColoredGlyph(Color.Green, DefaultBackground, 177);

            // replace starting console
            Game.Instance.Screen = this;
            Game.Instance.DestroyDefaultStartingConsole();
            IsFocused = true;

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
            AddWindow(window, _dungeon);

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
            AddWindow(window, _sideWindow);

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
            AddWindow(window, _miniMap);

            #endregion Mini Map Initialization

            #region Start Screen Initialization

            // create start screen
            _startScreen = new(Width - sideWindowWidth, 50) { Position = (0, 0) };

            // load the draw font
            var fontEnumerator = TheDrawFont.ReadFonts(@"Fonts/DESTRUCX.TDF").GetEnumerator();
            fontEnumerator.MoveNext();
            _drawFont = fontEnumerator.Current;

            Children.Add(_startScreen);

            // print the game name
            _startScreen.Surface.PrintDraw(5, "numeral", _drawFont, HorizontalAlignment.Center);
            _startScreen.Surface.PrintDraw(12, "dash", _drawFont, HorizontalAlignment.Center);

            // print info
            PrintCenter(20, "Collect all numbers scattered around the dungeon in the given order");
            PrintCenter(22, "and leave before the time runs out.");
            PrintCenter(26, "Controls: Arrow buttons to move, F5 toggle full screen");
            PrintCenter(28, "Press Enter to start...");
            
            #endregion

            // connect borders
            this.ConnectLines();
        }

        void PrintCenter(int y, string text)
        {
            _startScreen.Surface.Print(0, y, text.Align(HorizontalAlignment.Center, _startScreen.Surface.Width));
        }

        void AddWindow(Rectangle r, SadConsole.Console c)
        {
            // draw a border around the window
            this.DrawBox(r, ShapeParameters.CreateStyledBox(ICellSurface.ConnectedLineThick, _borderGlyph));

            // add the window to the display list
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

                if (!_startScreenShown && keyboard.IsKeyPressed(Keys.Enter)) {
                    _startScreenShown = true;
                    Children.Remove(_startScreen);
                    Children.Add(_dungeon);
                    _dungeon.Start();
                }
                else
                {
                    if (!_gameOver)
                    {
                        _dungeon.ProcessKeyboard(keyboard);
                    }
                    else if (keyboard.IsKeyPressed(Keys.Enter))
                    {
                        _dungeon.Restart();
                        _gameOver = false;
                    }
                }
            }
            return true;
        }

        void OnGameOver()
        {
            _gameOver = true;
        }
    }
}