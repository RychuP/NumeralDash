using System;
using SadConsole;
using SadRogue.Primitives;
using SadConsole.Input;
using NumeralDash.World;

namespace NumeralDash.Consoles
{
    public class GameManager : SadConsole.Console
    {
        const int statusWindowHeight = 6,
                  inventoryWindowWidth = 27,        // keep this number odd to allow dungeon view fit snugly in the dungeon window
                  mapWidth = 50,
                  mapHeight = 50,
                  twoBorders = 2,
                  oneBorder = 1;

        // border style around windows
        readonly ColoredGlyph _borderGlyph;

        // windows
        readonly Dungeon _dungeon;
        readonly Inventory _inventory;
        readonly Status _status;

        public GameManager(int width, int height) : base(width, height)
        {
            bool mapFailedToGenerate = false;

            _borderGlyph = new ColoredGlyph(Color.Green, DefaultBackground, 177);

            // replace starting console
            Game.Instance.Screen = this;
            Game.Instance.DestroyDefaultStartingConsole();
            IsFocused = true;

            #region Dungeon Initialization

            // calculate dungeon window size (substract oneBorder only because another border is shared with neigbouring windows)
            Point dungeonWindowSize = (Width - inventoryWindowWidth - oneBorder, height - statusWindowHeight - oneBorder);
            Point dungeonPosition = (0, 0);
            Rectangle window = new(dungeonPosition.X, dungeonPosition.Y, dungeonWindowSize.X, dungeonWindowSize.Y);

            // create a map
            Map map;
            try
            {
                map = new(mapWidth, mapHeight, 7, 5, 12);
            }
            catch (OverflowException e)
            {
                map = new(mapWidth, mapHeight);
                mapFailedToGenerate = true;
            }

            // create a dungeon (devide the window width by two to allow for the size of the C64 font 16x16 compared to the default 8x16)
            _dungeon = new(dungeonWindowSize.X / 2 - twoBorders, dungeonWindowSize.Y - twoBorders, map)
            {
                Position = (dungeonPosition.X + oneBorder, dungeonPosition.Y + oneBorder)
            };

            // add a new child and display it
            AddWindow(window, _dungeon);

            #endregion Dungeon

            #region Inventory Initialization

            // calculate inventory window size
            Point inventoryWindowSize = (inventoryWindowWidth + twoBorders, dungeonWindowSize.Y);
            Point inventoryPosition = (dungeonWindowSize.X - oneBorder, 0);
            window = new(inventoryPosition.X, inventoryPosition.Y, inventoryWindowSize.X, inventoryWindowSize.Y);

            // create an inventory (substractions and additions make allowance for the border)
            _inventory = new(inventoryWindowWidth, inventoryWindowSize.Y - twoBorders)
            {
                Position = (inventoryPosition.X + oneBorder, inventoryPosition.Y + oneBorder)
            };

            // add a new child and display it
            AddWindow(window, _inventory);

            #endregion Inventory

            #region Status Initialization

            // calculate status window size
            Point statusWindowSize = (Width, statusWindowHeight + twoBorders);
            Point statusPosition = (0, dungeonWindowSize.Y - oneBorder);
            window = new(statusPosition.X, statusPosition.Y, statusWindowSize.X, statusWindowSize.Y);

            // create a status console (substractions and additions make allowance for the border)
            _status = new(statusWindowSize.X - twoBorders, statusWindowHeight)
            {
                Position = (statusPosition.X + oneBorder, statusPosition.Y + oneBorder)
            };

            // add a new child and display it
            AddWindow(window, _status);

            #endregion Status

            // connect borders
            this.ConnectLines();

            if (mapFailedToGenerate)
            {
                // inform the user about the failed map generation
                _status.Print(2, 0, $"Map generation failed. Number of attempts: {map.FailedAttemptsAtGeneratingMap}");
            }
            else
            {
                // some debugging info in the status window
                _status.Print(2, 0, $"There are {map.Rooms.Count} rooms in this dungeon. " +
                $"Screen x cells: { Game.Instance.ScreenCellsX}, y cells: { Game.Instance.ScreenCellsY}");
                _status.Print(2, 1, $"Dungeon size: {_dungeon.Area.Size}, " +
                    $"Dungeon view size: ({_dungeon.ViewWidth}, {_dungeon.ViewHeight}), " +
                    $"Inventory size: {_inventory.Area.Size}, Status size: {_status.Area.Size}");
                _status.Print(2, 2, $"All rooms are connected: {map.AllRoomsAreConnected}, " +
                    $"AllRoomsAreReachable() iterations: {map.noOfChecksForAllRoomReachability}, " +
                    $"Failed attempts at map generation: {map.FailedAttemptsAtGeneratingMap}");
            }
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

                Point direction = (0, 0);

                if (keyboard.IsKeyPressed(Keys.Up))
                {
                    direction += Direction.Up;
                }
                else if (keyboard.IsKeyPressed(Keys.Down))
                {
                    direction += Direction.Down;
                }

                if (keyboard.IsKeyPressed(Keys.Left))
                {
                    direction += Direction.Left;
                }
                else if (keyboard.IsKeyPressed(Keys.Right))
                {
                    direction += Direction.Right;
                }

                // check if the direction has changed at all
                if (direction.X != 0 || direction.Y != 0)
                {
                    _dungeon.MovePlayer(direction);

                    // display some info about the current location
                    _status.Display(_dungeon.GetTileInfo());
                }
            }

            return base.ProcessKeyboard(keyboard);
        }
    }
}