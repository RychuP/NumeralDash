using System;
using SadConsole;
using SadRogue.Primitives;
using SadConsole.Input;
using NumeralDash.World;

namespace NumeralDash.Consoles
{
    class GameManager : SadConsole.Console
    {
        const int bottomWindowHeight = 6,
                  sideWindowWidth = 27,        // keep this number odd to allow dungeon view fit snugly in the dungeon window
                  twoBorders = 2,
                  oneBorder = 1;

        // border style around windows
        readonly ColoredGlyph _borderGlyph;

        // windows
        readonly Dungeon _dungeon;
        readonly SideWindow _sideWindow;
        readonly BottomWindow _bottomWindow;

        // game
        int level = 1;

        public GameManager(int width, int height) : base(width, height)
        {
            _borderGlyph = new ColoredGlyph(Color.Green, DefaultBackground, 177);

            // replace starting console
            Game.Instance.Screen = this;
            Game.Instance.DestroyDefaultStartingConsole();
            IsFocused = true;

            #region Dungeon Initialization

            // calculate dungeon window size (substract oneBorder only because another border is shared with neigbouring windows)
            Point dungeonWindowSize = (Width - sideWindowWidth - oneBorder, height - bottomWindowHeight - oneBorder);
            Point dungeonPosition = (0, 0);
            Rectangle window = new(dungeonPosition.X, dungeonPosition.Y, dungeonWindowSize.X, dungeonWindowSize.Y);

            // create a dungeon (devide the window width by two to allow for the size of the C64 font 16x16 compared to the default 8x16)
            _dungeon = new(dungeonWindowSize.X / 2 - twoBorders, dungeonWindowSize.Y - twoBorders, new Map())
            {
                Position = (dungeonPosition.X + oneBorder, dungeonPosition.Y + oneBorder)
            };

            // add a new child and display it
            AddWindow(window, _dungeon);

            #endregion Dungeon

            #region Side Window Initialization

            // calculate inventory window size
            Point inventoryWindowSize = (sideWindowWidth + twoBorders, dungeonWindowSize.Y);
            Point inventoryPosition = (dungeonWindowSize.X - oneBorder, 0);
            window = new(inventoryPosition.X, inventoryPosition.Y, inventoryWindowSize.X, inventoryWindowSize.Y);

            // create an inventory (substractions and additions make allowance for the border)
            _sideWindow = new(sideWindowWidth, inventoryWindowSize.Y - twoBorders, _dungeon)
            {
                Position = (inventoryPosition.X + oneBorder, inventoryPosition.Y + oneBorder)
            };

            // add a new child and display it
            AddWindow(window, _sideWindow);

            #endregion Inventory

            #region Bottom Window Initialization

            // calculate status window size
            Point statusWindowSize = (Width, bottomWindowHeight + twoBorders);
            Point statusPosition = (0, dungeonWindowSize.Y - oneBorder);
            window = new(statusPosition.X, statusPosition.Y, statusWindowSize.X, statusWindowSize.Y);

            // create a status console (substractions and additions make allowance for the border)
            _bottomWindow = new(statusWindowSize.X - twoBorders, bottomWindowHeight, _dungeon)
            {
                Position = (statusPosition.X + oneBorder, statusPosition.Y + oneBorder)
            };

            // add a new child and display it
            AddWindow(window, _bottomWindow);

            #endregion Status

            // connect borders
            this.ConnectLines();

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
            }

            _dungeon.ProcessKeyboard(keyboard);
            return base.ProcessKeyboard(keyboard);
        }
    }
}