using System;
using SadConsole;
using SadRogue.Primitives;
using SadConsole.Input;
using NumeralDash.World;

namespace NumeralDash.Consoles
{
    class GameManager : SadConsole.Console
    {
        const int sideWindowWidth = 27,        // keep this number odd to allow dungeon view fit snugly in the dungeon window
                  sideWindowHeight = 22,
                  twoBorders = 2,
                  oneBorder = 1;

        // border style around windows
        readonly ColoredGlyph _borderGlyph;

        // windows
        readonly Dungeon _dungeon;
        readonly SideWindow _sideWindow;
        readonly MiniMap _miniMap;

        bool _gameOver = false;

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

            // add a new child and display it
            AddWindow(window, _dungeon);

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

            // connect borders
            this.ConnectLines();

            // start game
            _dungeon.Start();
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
            return base.ProcessKeyboard(keyboard);
        }

        void OnGameOver()
        {
            _gameOver = true;
        }
    }
}