using System;
using SadConsole;
using SadRogue.Primitives;
using SadConsole.Entities;
using NumeralDash.World;
using NumeralDash.Entities;
using System.Collections.Generic;
using System.Linq;

namespace NumeralDash.Consoles
{
    class Dungeon : SadConsole.Console
    {
        Map _map;
        Player _player;
        Renderer _entityManager;
        List<Number> _numbers;

        public Dungeon(int viewSizeX, int viewSizeY, Map map) : base(viewSizeX, viewSizeY, map.Width, map.Height, map.Tiles)
        {
            _map = map;
            Font = Game.Instance.Fonts["C64"];

            // entity manager
            _entityManager = new Renderer();
            SadComponents.Add(_entityManager);

            // spawn the player
            _player = new Player(_map.PlayerStartPosition);
            SadComponents.Add(new SadConsole.Components.SurfaceComponentFollowTarget() { Target = _player });
            _entityManager.Add(_player);

            // spawn the numbers
            int numberCount = map.Rooms.Count;
            Room room;
            for (int i = 1; i <= numberCount; i++)
            {
                Number n = new(i);
                _entityManager.Add(n);
                if (map.Rooms.Any(room => room.CanAddNumber(n))) 
                {
                    do room = _map.GetRandomRoom(); 
                    while (!room.AddNumber(n, _map.PlayerStartPosition));
                }
                else
                {
                    throw new ArgumentException($"Excessive number of collectibles to be added. No room can accept {i}.");
                }
            }
        }

        public void MovePlayer(Point direction)
        {
            Point tileCoord = _player.GetNextMove(direction);
            if (_map.TileIsWalkable(tileCoord, out Room room))
            {
                _player.MoveTo(tileCoord);

                if (room is not null && !room.Visited)
                {
                    room.Visited = true;
                }
            }
        }

        public string[] GetTileInfo() => _map.GetTileInfo(_player.Position);
    }
}
