using System;
using SadConsole;
using SadRogue.Primitives;
using SadConsole.Entities;
using NumeralDash.World;
using NumeralDash.Entities;
using System.Collections.Generic;
using System.Linq;
using NumeralDash.Rules;
using SadConsole.Input;

namespace NumeralDash.Consoles
{
    class Dungeon : SadConsole.Console
    {
        // settings
        const float numbersPerRoom = 1.25f;

        #region Storage

        // fields
        readonly Renderer _entityManager;
        readonly Map _blankMap;
        int _level = 1;
        Map _map;

        // public properties
        public Player Player { get; init; }

        public IRule Rule { get; private set; }

        #endregion

        public Dungeon(int viewSizeX, int viewSizeY, Map blankMap) : base(viewSizeX, viewSizeY, blankMap.Width, blankMap.Height, blankMap.Tiles)
        {
            _blankMap = blankMap;
            _map = blankMap;
            Font = Game.Instance.Fonts["C64"];

            // entity manager
            _entityManager = new Renderer();
            SadComponents.Add(_entityManager);

            // set a temp rule
            Rule = new EmptyOrder();

            // create a player
            Player = new Player(_map.PlayerStartPosition, this);
            SadComponents.Add(new SadConsole.Components.SurfaceComponentFollowTarget() { Target = Player });
            _entityManager.Add(Player);
        }

        #region Level Management

        public void Start()
        {
            ChangeLevel();
        }

        void ChangeLevel()
        {
            try
            {
                ChangeMap();
                ChangeRule();
                SpawnEntities();
                OnLevelChanged();
            }
            catch (OverflowException)
            {
                _map = _blankMap;
                OnMapFailedToGenerate();
            }
        }

        void ChangeMap()
        {
            _map = new(_level++);
            int x = ViewWidth, y = ViewHeight;
            Surface = new CellSurface(_map.Width, _map.Height, _map.Tiles);
            ViewWidth = x;
            ViewHeight = y;
        }

        /// <summary>
        /// Spawns entities (all numbers and an exit).
        /// </summary>
        void SpawnEntities()
        {
            Room room;
            for (int i = 0; i < Rule.Numbers.Length + 1 /* 1 for the exit */; i++)
            {
                Entity n = (i < Rule.Numbers.Length) ? Rule.Numbers[i] : new Exit(Rule);
                _entityManager.Add(n);

                // keep looking for a room that will accept this entity
                if (_map.Rooms.Any(room => room.CanAddEntity(n)))
                {
                    do room = _map.GetRandomRoom();
                    while (!room.AddEntity(n, _map.PlayerStartPosition));
                }
                else
                {
                    throw new ArgumentException($"Excessive number of entities. No room can accept {i}.");
                }
            }

            // reposition player to the new start point
            Player.Position = _map.PlayerStartPosition;
        }

        /// <summary>
        /// Selects a new rule for number collections.
        /// </summary>
        void ChangeRule()
        {
            // select a rule for number collections
            int numberOfRules = 2, numberCount = Convert.ToInt32(_map.Rooms.Count * numbersPerRoom);
            var ruleNumber = Program.GetRandomIndex(numberOfRules);
            Rule = ruleNumber switch
            {
                0 => new SequentialOrder(numberCount),
                _ => new RandomOrder(numberCount)
            };
        }

        #endregion

        #region Player Management

        public bool MovePlayer(Point direction)
        {
            Point tileCoord = Player.GetNextMove(direction);
            if (_map.TileIsWalkable(tileCoord, out Room? room))
            {
                Player.MoveTo(tileCoord);

                // look for entities
                if (room is not null && room.GetEntityAt(tileCoord) is Entity e)
                {
                    if (!room.Visited)
                    {
                        room.Visited = true;
                    }

                    if (e is Number n)
                    {
                        Number drop = Player.PickUp(n);
                        room.ReplaceNumber(n, drop);
                    }

                    // check if the exit allows passage (all the numbers are collected)
                    else if (e is Exit x && x.AllowsPassage())
                    {
                        OnLevelComplete();
                    }
                }

                return true;
            }
            return false;
        }

        public override bool ProcessKeyboard(Keyboard keyboard)
        {
            if (keyboard.HasKeysPressed)
            {
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
                    MovePlayer(direction);
                    OnPlayerMoved();
                }
            }

            return base.ProcessKeyboard(keyboard);
        }

        #endregion

        #region Events

        void OnPlayerMoved()
        {
            PlayerMoved?.Invoke(_map.GetTileInfo(Player.Position));
        }

        public event Action<string[]>? PlayerMoved;

        void OnLevelComplete()
        {
            LevelComplete?.Invoke();
        }

        public event Action? LevelComplete;

        void OnLevelChanged()
        {
            var mapGenerationInfo = new string[]
            {
                $"There are {_map.Rooms.Count} rooms in this dungeon. " +
                    $"Screen x cells: { Game.Instance.ScreenCellsX}, y cells: { Game.Instance.ScreenCellsY}",
                $"Dungeon size: {Area.Size}, Dungeon view size: ({ViewWidth}, {ViewHeight}).",
                $"All rooms are connected: {_map.AllRoomsAreConnected}, " +
                    $"AllRoomsAreReachable() iterations: {_map.noOfChecksForAllRoomReachability}, " +
                    $"Failed attempts at map generation: {_map.FailedAttemptsAtGeneratingMap}"
            };

            LevelChanged?.Invoke(Rule, _level, mapGenerationInfo);
        }

        public event Action<IRule, int, string[]>? LevelChanged;

        void OnMapFailedToGenerate()
        {
            MapFailedToGenerate?.Invoke("Failure.");
        }

        public event Action<string>? MapFailedToGenerate;

        #endregion
    }
}
