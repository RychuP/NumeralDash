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
using System.Timers;

namespace NumeralDash.Consoles
{
    class Dungeon : SadConsole.Console
    {
        // settings
        const int levelTime = 5 * 60 + 0,           // time in seconds for the initial level
         timeChangePerLevel = 0 * 60 + 10;          // by how much to reduce the time per level in seconds

        #region Storage

        // fields
        readonly Timer _timer = new(1000) { AutoReset = true };
        TimeSpan _time;
        readonly TimeSpan _oneSecond = new TimeSpan(0, 0, 1);
        TimeSpan _totalTimePlayed = TimeSpan.Zero;
        readonly Map _blankMap;
        Renderer _entityManager;
        int _level = 0;
        Map _map;

        // public properties
        public Player Player { get; init; }

        public IRule Rule { get; private set; }

        #endregion

        public Dungeon(int viewSizeX, int viewSizeY, Map blankMap) : base(viewSizeX, viewSizeY, blankMap.Width, blankMap.Height, blankMap.Tiles)
        {
            _blankMap = blankMap;
            _map = blankMap;
            _timer.Elapsed += OnTimeElapsed;
            Font = Game.Instance.Fonts["C64"];

            // entity manager (temporary -> will be removed in ChangeMap)
            _entityManager = new Renderer();
            SadComponents.Add(_entityManager);

            // set a temp rule
            Rule = new EmptyOrder();

            // create a player
            Player = new Player(_map.PlayerStartPosition, this);
            SadComponents.Add(new SadConsole.Components.SurfaceComponentFollowTarget() { Target = Player });
        }

        #region Level Management

        public void Restart()
        {
            _totalTimePlayed = TimeSpan.Zero;
            _level = 0;
            Start();
        }

        public void Start()
        {
            ChangeLevel();
        }

        void ChangeLevel()
        {
            try
            {
                _map = new(_level++);
                ChangeMap();
                ChangeRule();
                SpawnEntities();
                StartTimer();
                OnLevelChanged();
            }
            catch (MapGenerationException e)
            {
                _map = _blankMap;
                ChangeMap();
                Player.Position = _map.PlayerStartPosition;
                OnMapFailedToGenerate(e.FailedAttempts);
            }
            
        }

        void StartTimer()
        {
            int totalTime = levelTime - (_level - 1) * timeChangePerLevel;
            int minutes = Convert.ToInt32(totalTime / 60);
            int seconds = totalTime - minutes * 60;
            _time = new(0, minutes, seconds);
            _timer.Start();
        }

        void ChangeMap()
        {
            // get new surface
            int x = ViewWidth, y = ViewHeight;
            Surface = new CellSurface(_map.Width, _map.Height, _map.Tiles);
            ViewWidth = x;
            ViewHeight = y;

            // remove prev renderer
            SadComponents.Remove(_entityManager);

            // get new renderer
            _entityManager = new Renderer();
            SadComponents.Add(_entityManager);

            // register player with the renderer
            _entityManager.Add(Player);
        }

        /// <summary>
        /// Spawns entities (all numbers and the exit).
        /// </summary>
        void SpawnEntities()
        {
            // reposition player to the new start point
            Player.Position = _map.PlayerStartPosition;

            // spawn numbers
            for (int i = 0; i < Rule.Numbers.Length; i++)
            {
                // find a room for the new collidable entity (number)
                ICollidable c = Rule.Numbers[i];
                PlaceCollidableInRandomRoom(c);

                // register entity
                _entityManager.Add(c as Entity);

                // register number extensions if any
                if (c.Size > 1 && c is Number n)
                {
                    foreach (var e in n.Extensions)
                    {
                        _entityManager.Add(e);
                    }
                }
            }

            // spawn exit
            var exit = new Exit();
            PlaceCollidableInRandomRoom(exit);
            _entityManager.Add(exit);
        }

        void PlaceCollidableInRandomRoom(ICollidable c)
        {
            Room room;
            if (_map.Rooms.Any(room => !room.ReachedEntityLimit()))
            {
                do room = _map.GetRandomRoom();
                while (!room.AddCollidable(c, Player));
            }
            else
            {
                throw new ArgumentException($"Excessive number of entities. No room can accept {c}.");
            }
        }

        void PrintCenter(int y, string s)
        {
            s = s.Align(HorizontalAlignment.Center, Width);
            this.Print(0, y, s);
        }

        /// <summary>
        /// Selects a new rule for number collections.
        /// </summary>
        void ChangeRule()
        {
            // select a rule for number collections
            var ruleNumber = Program.GetRandomIndex(3 /* number of rules in the switch expression */);
            Rule = ruleNumber switch
            {
                0 => new SequentialOrder(_map.NumberCount),
                1 => new ReverseOrder(_map.NumberCount),
                _ => new RandomOrder(_map.NumberCount)
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

                // check if the tile belongs to a room
                if (room is not null)
                {
                    // mark it as visited if not already
                    if (!room.Visited)
                    {
                        room.Visited = true;
                    }

                    // look for entities at the player's position
                    if (room.GetCollidableAt(tileCoord) is Entity e)
                    {
                        if (e is Number n)
                        {
                            room.RemoveNumber(n);
                            Number drop = Player.PickUp(n);
                            room.PlaceNumber(drop, n.Position);
                        }

                        // check if the level is completed
                        else if (e is Exit && Rule.NextNumber == Number.Finished)
                        {
                            _timer.Stop();
                            ChangeLevel();
                        }
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
                    // save the current level number to see if the player's move triggered a change
                    int currentLevel = _level;

                    // move the player
                    MovePlayer(direction);

                    // if the map remains the same, trigger an event
                    if (currentLevel == _level)
                    {
                        OnPlayerMoved();
                    }
                }
            }

            return base.ProcessKeyboard(keyboard);
        }

        #endregion

        #region Events

        void OnPlayerMoved()
        {
            // PlayerMoved?.Invoke(_map.GetTileInfo(Player.Position));
            PlayerMoved?.Invoke();
        }

        public event Action? PlayerMoved;

        void OnLevelChanged()
        {
            var mapGenerationInfo = new string[]
            {
                $"Dungeon level: {_level}, number of rooms: {_map.Rooms.Count}, map size: {Area.Size}."
            };

            LevelChanged?.Invoke(Rule, _level, mapGenerationInfo);

            // invoke this as well to show the timer as soon as the new level is displayed (otherwise there is a 1 sec delay)
            TimeElapsed?.Invoke(_time);
        }

        public event Action<IRule, int, string[]>? LevelChanged;

        void OnMapFailedToGenerate(AttemptCounters failedAttempts)
        {
            MapFailedToGenerate?.Invoke(failedAttempts);
        }

        public event Action<AttemptCounters>? MapFailedToGenerate;

        void OnTimeElapsed(object source, ElapsedEventArgs e)
        {
            _time -= _oneSecond;
            _totalTimePlayed += _oneSecond;

            if (_time == TimeSpan.Zero)
            {
                _timer.Stop();
                OnGameOver();
            }

            TimeElapsed?.Invoke(_time);
        }

        public event Action<TimeSpan>? TimeElapsed;

        void OnGameOver()
        {
            // get new surface
            Surface = new CellSurface(ViewWidth, ViewHeight);

            // remove prev renderer
            SadComponents.Remove(_entityManager);

            PrintCenter(10, "Game Over");
            PrintCenter(13, $"You have reached level {_level}. Well done.");
            PrintCenter(15, $"Total gameplay time: {_totalTimePlayed}");
            PrintCenter(19, "Press Enter to try again...");

            GameOver?.Invoke();
        }

        public event Action? GameOver;

        #endregion
    }
}
