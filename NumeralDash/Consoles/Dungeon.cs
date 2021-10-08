using System;
using SadConsole;
using SadRogue.Primitives;
using SadConsole.Entities;
using NumeralDash.World;
using NumeralDash.Entities;
using System.Collections.Generic;
using System.Linq;
using NumeralDash.Rules;

namespace NumeralDash.Consoles
{
    class Dungeon : SadConsole.Console
    {
        // settings
        const float numbersPerRoom = 1.25f;

        Map _map;
        Player _player;
        Renderer _entityManager;

        public Dungeon(int viewSizeX, int viewSizeY, Map map) : base(viewSizeX, viewSizeY, map.Width, map.Height, map.Tiles)
        {
            _map = map;
            Font = Game.Instance.Fonts["C64"];

            // entity manager
            _entityManager = new Renderer();
            SadComponents.Add(_entityManager);

            // select a rule for number collections
            int numberOfRules = 2, numberCount = Convert.ToInt32(map.Rooms.Count * numbersPerRoom);
            var ruleNumber = Program.GetRandomIndex(numberOfRules);
            IRule rule = ruleNumber switch
            {
                0 => new SequentialOrder(numberCount),
                _ => new RandomOrder(numberCount)
            };

            // spawn the player
            _player = new Player(_map.PlayerStartPosition, rule);
            SadComponents.Add(new SadConsole.Components.SurfaceComponentFollowTarget() { Target = _player });
            _entityManager.Add(_player);

            // spawn entities (numbers and the exit)
            Room room;
            for (int i = 0; i < numberCount + 1 /* 1 for the exit */; i++)
            {
                Entity n = (i < numberCount) ? rule.Numbers[i] : new Exit(rule);
                _entityManager.Add(n);

                // keep looking for a room that will accept this entity
                if (map.Rooms.Any(room => room.CanAddEntity(n))) 
                {
                    do room = _map.GetRandomRoom(); 
                    while (!room.AddEntity(n, _map.PlayerStartPosition));
                }
                else
                {
                    throw new ArgumentException($"Excessive number of entities. No room can accept {i}.");
                }
            }
        }

        public bool MovePlayer(Point direction)
        {
            Point tileCoord = _player.GetNextMove(direction);
            if (_map.TileIsWalkable(tileCoord, out Room? room))
            {
                _player.MoveTo(tileCoord);

                // look for entities
                if (room is not null && room.GetEntityAt(tileCoord) is Entity e)
                {
                    if (!room.Visited)
                    {
                        room.Visited = true;
                    }

                    if (e is Number n)
                    {
                        Number drop = _player.Collect(n);
                        room.ReplaceNumber(n, drop);
                    }

                    // check if the exit allows passage (all the numbers are collected)
                    else if (e is Exit x && x.AllowsPassage())
                    {
                        OnLevelCompleted();
                    }
                }

                return true;
            }
            return false;
        }

        public string[] GetTileInfo() => _map.GetTileInfo(_player.Position);

        void OnLevelCompleted()
        {
            LevelCompleted?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler? LevelCompleted;
    }
}
