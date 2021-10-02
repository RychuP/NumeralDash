using System;
using SadConsole;
using SadRogue.Primitives;
using SadConsole.Entities;
using NumeralDash.World;
using NumeralDash.Entities;

namespace NumeralDash.Consoles
{
    class Dungeon : SadConsole.Console
    {
        Map _map;
        Player _player;
        Renderer _entityManager;

        public Dungeon(int viewSizeX, int viewSizeY, Map map) : base(viewSizeX, viewSizeY, map.Width, map.Height, map.Tiles)
        {
            _map = map;
            Font = Game.Instance.Fonts["C64"];

            // create player
            _player = new Player(_map.PlayerStartPosition);
            SadComponents.Add(new SadConsole.Components.SurfaceComponentFollowTarget() { Target = _player });

            // entity manager
            _entityManager = new Renderer();
            SadComponents.Add(_entityManager);
            _entityManager.Add(_player);
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
