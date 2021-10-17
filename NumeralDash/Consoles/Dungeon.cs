﻿using System;
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
        #region Storage

        // fields
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

            // reposition player to the new start point
            Player.Position = _map.PlayerStartPosition;
        }

        void PlaceCollidableInRandomRoom(ICollidable c)
        {
            Room room;
            if (_map.Rooms.Any(room => !room.ReachedEntityLimit()))
            {
                do room = _map.GetRandomRoom();
                while (!room.AddCollidable(c, _map.PlayerStartPosition));
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
            var ruleNumber = Program.GetRandomIndex(2 /* number of rules in the switch expression */);
            Rule = ruleNumber switch
            {
                0 => new SequentialOrder(_map.NumberCount),
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
            PlayerMoved?.Invoke(_map.GetTileInfo(Player.Position));
        }

        public event Action<string[]>? PlayerMoved;

        void OnLevelChanged()
        {
            var mapGenerationInfo = new string[]
            {
                $"Dungeon level: {_level}, number of rooms: {_map.Rooms.Count}, map size: {Area.Size}."
            };

            LevelChanged?.Invoke(Rule, _level, mapGenerationInfo);
        }

        public event Action<IRule, int, string[]>? LevelChanged;

        void OnMapFailedToGenerate(AttemptCounters failedAttempts)
        {
            MapFailedToGenerate?.Invoke(failedAttempts);
        }

        public event Action<AttemptCounters>? MapFailedToGenerate;

        #endregion
    }
}
