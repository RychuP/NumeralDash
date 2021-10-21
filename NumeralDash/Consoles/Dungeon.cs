using System;
using SadConsole;
using SadRogue.Primitives;
using SadConsole.Entities;
using NumeralDash.World;
using NumeralDash.Entities;
using System.Collections.Generic;
using NumeralDash.Other;
using System.Linq;
using NumeralDash.Rules;
using SadConsole.Input;
using System.Timers;

namespace NumeralDash.Consoles
{
    class Dungeon : SadConsole.Console
    {
        // settings
        //const int levelTime = 0 * 60 + 5;
        const int levelTime = 5 * 60 + 0;               // time in seconds for the initial level
        const int timeChangePerLevel = 0 * 60 + 10;     // by how much to reduce the time per level in seconds

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
        Direction _fastMoveDirection = Direction.None;
        bool _playerIsMovingFast;

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
            _playerIsMovingFast = false;

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

        public void StartFastMove(Direction direction)
        {
            _playerIsMovingFast = true;
            _fastMoveDirection = direction;
        }

        Point MovePlayer(Direction d)
        {
            Point originalPosition = Player.Position;
            Player.MoveInDirection(d);
            OnPlayerMoved();
            return originalPosition;
        }

        /// <summary>
        /// Tries to move the player by one tile in the given direction.
        /// </summary>
        /// <param name="direction">Direction of travel.</param>
        /// <returns>True if the move succeeded, otherwise false.</returns>
        public bool TryMovePlayer(Direction d)
        {
            Point tileCoord = Player.GetNextMove(d);
            if (_map.TileIsWalkable(tileCoord, out Room? room))
            {
                Point playerPrevPosition = MovePlayer(d);

                // check if the tile belongs to a room
                if (room is not null)
                {
                    if (!room.Visited) room.Visited = true;

                    // look for entities at the player's position
                    if (room.GetCollidableAt(tileCoord) is Entity e)
                    {
                        // check if the player is not walking over a long, multicell number and prevent that type of collections
                        if (e is Number n && !n.Coords.Contains(playerPrevPosition))
                        {
                            // player can collect the number
                            room.RemoveNumber(n);
                            Number drop = Player.PickUp(n);
                            room.PlaceNumber(drop, n.Position);

                            // stop the fast move
                            _playerIsMovingFast = false;
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

        public override void Update(TimeSpan delta)
        {
            base.Update(delta);

            if (_playerIsMovingFast)
            {
                bool success = TryMovePlayer(_fastMoveDirection);
                if (!success) _playerIsMovingFast = false;
            }
        }

        public new void ProcessKeyboard(Keyboard keyboard)
        {
            // fast move with a left shift modifier
            if (keyboard.HasKeysDown && keyboard.IsKeyDown(Keys.LeftShift))
            {
                // accept only one direction at a time
                if (keyboard.IsKeyDown(Keys.Left))
                {
                    StartFastMove(Direction.Left);
                }
                else if (keyboard.IsKeyDown(Keys.Right))
                {
                    StartFastMove(Direction.Right);
                }
                else if (keyboard.IsKeyDown(Keys.Up))
                {
                    StartFastMove(Direction.Up);
                }
                else if (keyboard.IsKeyDown(Keys.Down))
                {
                    StartFastMove(Direction.Down);
                }
            }
            // normal move by one tile
            else if (keyboard.HasKeysPressed)
            {
                Point delta = (0, 0);

                if (keyboard.IsKeyPressed(Keys.Up))
                {
                    delta += Direction.Up;
                }
                else if (keyboard.IsKeyPressed(Keys.Down))
                {
                    delta += Direction.Down;
                }

                if (keyboard.IsKeyPressed(Keys.Left))
                {
                    delta += Direction.Left;
                }
                else if (keyboard.IsKeyPressed(Keys.Right))
                {
                    delta += Direction.Right;
                }

                // check if the direction has changed at all
                if (delta.X != 0 || delta.Y != 0)
                {
                    TryMovePlayer(Direction.GetCardinalDirection(delta));
                }
            }
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
            GameOver?.Invoke(_level, _totalTimePlayed);
        }

        public event Action<int, TimeSpan>? GameOver;

        #endregion
    }
}
