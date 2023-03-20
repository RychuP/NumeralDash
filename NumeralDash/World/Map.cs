using System;
using SadConsole;
using SadRogue.Primitives;
using System.Linq;
using System.Collections.Generic;
using NumeralDash.Tiles;

namespace NumeralDash.World
{
    // Stores, manipulates and queries Tile data
    class Map
    {
        // settings
        public const int DefaultSize = 50;                    // map is square -> this is the default length of its sides
               const int SizeModifier = 5,                    // by how much the map will grow each level
                         MaxRooms = 5,                        // max amount of rooms to be generated on the first level
                         MaxRoomsModifier = 2,                // amount of rooms that will be added each level
                         MinRoomSize = 5,                     // minimum length of any side of a room
                         MaxRoomSize = 12,                    // maximum length of any side of a room
                         MaxRoomPositionAttempts = 20,        // max number of failed room position finding per room after which a new room is generated
                         MaxRoomGenerationAttempts = 100,     // max number of failed room position finding per map generation after which an exception is thrown
                         MaxRoadGenerationAttempts = 100,     // max number of failed road generations for all rooms per map generation after which an exception is thrown
                         GenerationAttemptsPerLevel = 10,     // this is added to the above 3 maxes and multiplied by _level
                         MaxAttemptsMapGeneration = 100,      // how many times this object will try to generate a map with current settings
                         NumbersPerRoom = 1;                  // how many numbers per room can this map accept

        #region Storage

        // for debugging
        int _failedAttemptsMapGeneration = 0;
        int _failedAttemptsRoomGeneration = 0;
        int _failedAttemptsRoadGeneration = 0;

        /// <summary>
        /// List of all rooms.
        /// </summary>
        readonly List <Room> _rooms;

        /// <summary>
        /// Width of the map.
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Height of the map.
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// Contains all tile objects.
        /// </summary>
        public Tile[] Tiles { get; private set; }

        // Starting position for the player
        public Point PlayerStartPosition { get; private set; }

        readonly int _level;

        #endregion

        // constructor
        public Map(int level) : this(DefaultSize + level * SizeModifier, DefaultSize + level * SizeModifier)
        {
            _level = level;

            // keep trying to generate a map until one valid is made
            while (true)
            {
                try
                {
                    Generate(MaxRooms + level * MaxRoomsModifier, MinRoomSize, MaxRoomSize);
                    break;
                }
                catch (RoomGenerationException)
                {
                    _failedAttemptsMapGeneration++;
                    _failedAttemptsRoomGeneration++;
                }
                catch (RoadGenerationException)
                {
                    _failedAttemptsMapGeneration++;
                    _failedAttemptsRoadGeneration++;
                }

                if (_failedAttemptsMapGeneration > MaxAttemptsMapGeneration)
                {
                    throw new MapGenerationException(
                        _failedAttemptsRoomGeneration,
                        _failedAttemptsRoadGeneration,
                        _failedAttemptsMapGeneration
                    );
                }
            }
        }

        // constructor that creates an unwalkable map full with walls
        public Map(int width = DefaultSize, int height = DefaultSize)
        {
            Width = width;
            Height = height;
            Tiles = new Tile[width * height];
            _rooms = new();

            // Fill the entire tile array with walls
            for (int i = 0; i < Tiles.Length; i++)
            {
                Tiles[i] = new Wall();
            }

            PlayerStartPosition = (width / 2, height / 2);
        }

        /// <summary>
        /// Checks it the point p is outside the bounds of the map.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        bool PointIsOutOfBounds(Point p) => p.X < 0 || p.Y < 0 || p.X >= Width || p.Y >= Height;

        /// <summary>
        /// How many numbers to generate for this map (used by collection rules).
        /// </summary>
        public int NumberCount => Convert.ToInt32(Rooms.Count * NumbersPerRoom);


        #region Room Management

        /// <summary>
        /// All the rooms that have been generated on the map.
        /// </summary>
        public List<Room> Rooms
        {
            get => _rooms;
        }

        /// <summary>
        /// Returns a random room.
        /// </summary>
        /// <returns></returns>
        public Room GetRandomRoom()
        {
            int index = Game.Instance.Random.Next(0, _rooms.Count - 1);
            return _rooms[index];
        }

        #endregion


        #region Tile Management

        // returns true if the tile location is walkable
        public bool TileIsWalkable(Point p, out Room? room)
        {
            Tile tile = Tiles[p.ToIndex(Width)];
            room = null;
            if (!PointIsOutOfBounds(p) && !tile.IsBlockingMove)
            {
                if (tile is Floor f && f.Parent is Room r) room = r;
                return true;
            }
            else return false;
        }

        // returns a tile at the given point p
        public Tile GetTile(Point p) => Tiles[p.ToIndex(Width)];

        // returns info about the tile at the given point
        public string[] GetTileInfo(Point p)
        {
            if (PointIsOutOfBounds(p)) return new string[] { $"Point {p} is outside the bounds of the map." };
            else
            {
                var tile = Tiles[p.ToIndex(Width)];
                if (tile is Floor f) return f.GetExtendedInfo(p);
                else return new string[] { $"Position {p}, {tile.GetInfo()}" };
            } 
        }

        #endregion


        #region Map Generator
        public void Generate(int maxRooms, int minRoomSize, int maxRoomSize)
        {
            // create rooms on the map
            int totalRoomPositionAttemptCounter = 0;
            while (_rooms.Count < maxRooms)
            {
                // generate a room
                Room newRoom = new(minRoomSize, maxRoomSize);

                // try to find a valid position for the room
                int roomPositionAttemptCounter = 0;
                while (roomPositionAttemptCounter++ < MaxRoomPositionAttempts + GenerationAttemptsPerLevel * _level)
                {
                    newRoom.SetRandomPosition(Width, Height);

                    // check if the new room does not interfere with any of the previously generated
                    if (!_rooms.Any(room => room.InterferesWith(newRoom)))
                    {
                        // add the room to the map
                        _rooms.Add(newRoom);
                        CreateFloors(newRoom);
                        break;
                    }
                    else
                    {
                        if (totalRoomPositionAttemptCounter++ > MaxRoomGenerationAttempts + GenerationAttemptsPerLevel * _level)
                        {
                            throw new RoomGenerationException();
                        }
                    }
                }
            }

            // attempt to connect all the the rooms
            int totalRoadGenerationAttemptCounter = 0;
            while (!AllRoomsAreReachable())
            {
                foreach (var room in _rooms)
                {
                    // check for excessive connections to this room
                    if (room.CanAddRoad())
                    {
                        CreateRandomRoad(room);
                    }
                }

                if (totalRoadGenerationAttemptCounter++ > MaxRoadGenerationAttempts + GenerationAttemptsPerLevel * _level)
                {
                    throw new RoadGenerationException();
                }
            }

            // set the start position for the player
            PlayerStartPosition = _rooms[0].Area.Center;
        }

        /// <summary>
        /// Checks if all rooms are connected and reachable by a player.
        /// </summary>
        /// <returns></returns>
        bool AllRoomsAreReachable()
        {
            // first test if all the rooms have at least one road
            if (_rooms.Any(room => room.HasNoRoads())) return false;

            // get the first room as a reference point
            var firstRoom = _rooms[0];

            List<Room> testedRooms = new();

            foreach (var room in _rooms)
            {
                if (room == firstRoom) continue;

                // check if the first room can reach this room
                else
                {
                    testedRooms.Clear();
                    if (firstRoom.CanReach(room, ref testedRooms)) continue;
                    else return false;
                }
            }

            // the first room has not found a single other it cannot reach
            return true;
        }

        /// <summary>
        /// Carves out a rectangular room covered with TileFloors.
        /// </summary>
        /// <param name="room"></param>
        void CreateFloors(Room room)
        {
            //Carve out a rectangle of floors in the tile array
            for (int x = room.Area.X; x <= room.Area.MaxExtentX; x++)
            {
                for (int y = room.Area.Y; y <= room.Area.MaxExtentY; y++)
                {
                    // Calculates the appropriate position (index) in the array
                    // based on the y of tile, width of map, and x of tile
                    int index = y * Width + x;
                    Tiles[index] = new Floor(room);
                }
            }
        }

        /// <summary>
        /// Carves out a road covered with TileFloors.
        /// </summary>
        /// <param name="roadPoints"></param>
        /// <param name="road"></param>
        void CreateFloors(List<Point> roadPoints, Road road)
        {
            foreach (var point in roadPoints)
            {
                int index = point.ToIndex(Width);
                Tiles[index] = new Floor(road)
                {
                    Foreground = road.Color
                };
            }
        }

        /// <summary>
        /// Creates a random road.
        /// </summary>
        /// <param name="room">Start point for the road.</param>
        /// <returns></returns>
        public bool CreateRandomRoad(Room room)
        {
            int index = 0, legs = 1, legLength = 0, maxLegLength = Road.GetRandomLegLength();

            // create a random direction
            Direction direction = Road.GetRandomDirection();

            // get perpendicular directions
            Direction[] perpendicularDirections = Road.GetPerpendicularDirections(direction);

            // get a random start point for the road
            Point start = room.GetRoadStartPoint(direction);

            // create a storage for all the points that will form the road
            List<Point> roadPoints = new() { start };
            do
            {
                // get the tile from the map for the current road point
                int nextTileIndex = roadPoints[index].ToIndex(Width);
                Tile tile = Tiles[nextTileIndex];

                // check if the next road tile happens to be a floor -> we've reached something!
                if (tile is Floor t)
                {
                    // check if the road will have zero length
                    if (roadPoints.Count == 1)
                    {
                        return false;
                    }

                    // check if the destination is a room 
                    if (t.Parent is Room other)
                    {
                        // check if a road already exists between the two rooms or the other room reached a limit of roads
                        if (room.HasConnectionTo(other) || !other.CanAddRoad())
                        {
                            return false;
                        }

                        // create a new road leading from one room to the other
                        roadPoints.RemoveAt(index);
                        Road road = new(roadPoints, room, other, legs);

                        // create floors for the road
                        CreateFloors(roadPoints, road);
                        return true;
                    }
                    else if (t.Parent is Road road)
                    {
                        // check if the road already has a direct connection to this room
                        if (road.HasConnectionTo(room)

                        // check for indirect connections from rooms connected to that road
                        || road.HasRoomWithConnectionTo(room))
                        {
                            return false;
                        }
                        else
                        {
                            // attach this road to the existing one
                            roadPoints.RemoveAt(index);

                            // check if the existing road accepts the addition
                            if (road.AddLeg(roadPoints, room, legs))
                            {
                                // create floors for the road
                                CreateFloors(roadPoints, road);
                                return true;
                            }
                            else return false;
                        }
                    }

                    // Test for unknown parent
                    throw new Exception("Unknown floor parent. Program should not reach this code.");
                }

                // we've hit a wall tile
                else
                {
                    // check if adjacent tiles in perpendicular directions are not floors
                    // to prevent spawning two roads next to each other or roads destroying room walls
                    foreach (Direction d in perpendicularDirections)
                    {
                        var perpendicularPoint = roadPoints[index] + d;

                        // return false when road is going along the side of the map
                        if (PointIsOutOfBounds(perpendicularPoint)) return false;

                        Tile perpendicularTile = Tiles[perpendicularPoint.ToIndex(Width)];
                        if (perpendicularTile is Floor)
                        {
                            return false;
                        }
                    }

                    // create the next road point for checking
                    var nextRoadPoint = roadPoints[index++];
                    legLength++;

                    // make a new leg every MaxLegLength
                    if (legLength > maxLegLength)
                    {
                        ChangeDirection();
                    }

                    // check if the road has reached the max number of legs allowed, assuming this road will only connect 2 rooms
                    if (legs > Road.MaxLegsPerRoom * 2

                    // check if it's not too long, assuming this road will only connect 2 rooms
                    || index > Road.MaxLengthPerRoom * 2) return false;

                    // generate the next road point
                    roadPoints.Add(nextRoadPoint + direction);
                }
            } while (!PointIsOutOfBounds(roadPoints.Last()));

            // nextRoadPoint has reached the end of map without finding any floors
            return false;

            void ChangeDirection()
            {
                direction = Road.GetRandomDirection(perpendicularDirections);
                perpendicularDirections = Road.GetPerpendicularDirections(direction);
                maxLegLength = Road.GetRandomLegLength();
                legLength = 0;
                legs++;
            }
        }

        #endregion
    }

    class RoomGenerationException : OverflowException { }

    class RoadGenerationException : OverflowException { }

    class MapGenerationException : OverflowException
    {
        public AttemptCounters FailedAttempts;

        public MapGenerationException(int roomGenerationAttempts, int roadGenerationAttempts, int mapGenerationAttempts) : base()
        {
            FailedAttempts = new(roomGenerationAttempts, roadGenerationAttempts, mapGenerationAttempts);
        }
    }

    record AttemptCounters(int RoomGeneration, int RoadGeneration, int MapGeneration);
}
