using System;
using SadConsole;
using SadRogue.Primitives;
using System.Collections.Generic;
using NumeralDash.Tiles;
using System.Linq;

namespace NumeralDash.World
{
    class Room : IConnectable
    {
        public const int RoadsLimit = 3;

        // serves as an id generator
        static int counter = 0;

        public int ID { get; init; }

        // list of roads connected to this room
        List<Road> _roads { get; init; }

        // rectangle representing the room on the map
        Rectangle _area;

        // expanded rectangle that includes the wall around the room
        Rectangle _expandedArea;

        public Rectangle Area
        {
            get => _area;
            set
            {
                _area = value;
                _expandedArea = value.Expand(1, 1);

                if (_roads.Count > 0)
                {
                    throw new ArgumentException("Attempt at changing a size or position of a room with connections already established!");
                }
            }
        }

        public Room(int minSize, int maxSize) : base()
        {
            _roads = new();

            // generate an id
            ID = counter++;

            // generate a random size within the given constraints
            int width, height;
            do
            {
                // set the room's (width, height) as a random size between (minRoomSize, maxRoomSize)
                width = Game.Instance.Random.Next(minSize, maxSize);
                height = Game.Instance.Random.Next(minSize, maxSize);
            }
            // make sure the rooms are generated within the proportions below
            while (width < height * 0.8 || width > height * 1.4);

            // create the rectangle area
            Area = new(0, 0, width, height);
        }

        public void SetRandomPosition(int mapWidth, int mapHeight)
        {
            // sets the room's X/Y Position at a random point between the edges of the map
            // x 1 and y -1 to make sure there is at least one wall tile between the room and the edge of the map
            int x = Game.Instance.Random.Next(1, mapWidth - Area.Width - 1);
            int y = Game.Instance.Random.Next(1, mapHeight - Area.Height - 1);
            Area = Area.WithPosition(new Point(x, y));
        }

        // returns true if the room intersects or is too close to another room
        public bool InterferesWith(Room other) => _expandedArea.Intersects(other.Area);

        // returns a random start point for a road in the given direction
        public Point GetRoadStartPoint(Direction d)
        {
            Func<int, int, int> rand = Game.Instance.Random.Next;
            Rectangle r = _expandedArea;

            // random point at a side of the room that's at least one tile away from the corners
            int randomY = rand(Area.Y + 1, Area.MaxExtentY - 1),
                randomX = rand(Area.X + 1, Area.MaxExtentX - 1);

            return d.Type switch
            {
                Direction.Types.Left => (r.X, randomY),
                Direction.Types.Right => (r.MaxExtentX, randomY),
                Direction.Types.Up => (randomX, r.Y),
                _ => (randomX, r.MaxExtentY)
            };
        }

        public bool HasConnectionTo(Room room) => _roads.Any(road => road.HasConnectionTo(room));

        public bool HasZeroRoads() => _roads.Count == 0;

        public bool RoadLimitReached() => _roads.Count >= RoadsLimit;

        public void AddRoad(Road road)
        {
            if (road.HasConnectionTo(this))
            {
                _roads.Add(road);
            }
            else
            {
                throw new ArgumentException("Attempt at adding a road without the connection to this room.");
            }
        }

        public int RoadCount => _roads.Count;

        public bool Visited { get; set; }

        public bool CanReach(Room other, ref List<Room> testedRooms)
        {
            if (other == this) return true;

            if (testedRooms.Contains(this)) return false;

            else
            {
                if (_roads.Any(road => road.HasConnectionTo(other))) return true;
                else
                {
                    testedRooms.Add(this);
                    foreach (var road in _roads)
                    {
                        if (road.HasRoomThatCanReach(other, ref testedRooms)) return true;
                    }
                    return false;
                }
            }
        }

        public override string ToString() => $"RoomID {ID}";

        public string GetInfo() => $"Area: {Area.Size}, Position: {Area.Position}, Roads: {RoadCount}";
    }
}
