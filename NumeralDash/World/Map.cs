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
        // for debugging
        public int noOfChecksForAllRoomReachability = 0,
            FailedAttemptsAtGeneratingMap = 0;
        public bool AllRoomsAreConnected = false;

        #region Storage

        // list of all rooms
        List<Room> _rooms;

        // Width of the map
        public int Width { get; }

        // Height of the map
        public int Height { get; }

        // contains all tile objects
        public TileBase[] Tiles { get; private set; }

        // Starting position for the player
        public Point PlayerStartPosition { get; private set; }

        #endregion

        // constructor
        public Map(int width, int height, int maxRooms, int minRoomSize, int maxRoomSize) : this(width, height)
        {
            // keep trying to generate a map until one valid is made
            while (!Generate(maxRooms, minRoomSize, maxRoomSize))
            {
                FailedAttemptsAtGeneratingMap++;
                noOfChecksForAllRoomReachability = 0;
                if (FailedAttemptsAtGeneratingMap > 10)
                {
                    throw new OverflowException("Too many failed attempts at generating a map.");
                }
            }
        }

        // constructor that creates an unwalkable map full with walls
        public Map(int width, int height)
        {
            Width = width;
            Height = height;
            Tiles = new TileBase[width * height];
            _rooms = new List<Room>();

            // Fill the entire tile array with walls
            for (int i = 0; i < Tiles.Length; i++)
            {
                Tiles[i] = new TileWall();
            }

            PlayerStartPosition = (width / 2, height / 2);
        }

        // Checks it the point p is outside the bounds of the map
        bool PointIsOutOfBounds(Point p) => p.X < 0 || p.Y < 0 || p.X >= Width || p.Y >= Height;


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
        public bool TileIsWalkable(Point p, out Room room)
        {
            TileBase tile = Tiles[p.ToIndex(Width)];
            room = null;
            if (!PointIsOutOfBounds(p) && !tile.IsBlockingMove)
            {
                if (tile is TileFloor f && f.Parent is Room r) room = r;
                return true;
            }
            else return false;
        }

        // returns a tile at the given point p
        public TileBase GetTile(Point p) => Tiles[p.ToIndex(Width)];

        // returns info about the tile at the given point
        public string[] GetTileInfo(Point p)
        {
            if (PointIsOutOfBounds(p)) return new string[] { $"Point {p} is outside the bounds of the map." };
            else
            {
                var tile = Tiles[p.ToIndex(Width)];
                if (tile is TileFloor f) return f.GetExtendedInfo(p);
                else return new string[] { $"Position {p}, {tile.GetInfo()}" };
            } 
        }

        #endregion


        #region Map Generator
        public bool Generate(int maxRooms, int minRoomSize, int maxRoomSize)
        {
            int attemptCounter = 0, maxAttempts = maxRooms * 5;

            // create rooms on the map
            while (_rooms.Count < maxRooms && attemptCounter++ < maxAttempts)
            {
                // generate a room
                Room newRoom = new(minRoomSize, maxRoomSize);

                // try to find a valid position for the room
                int attemptCounter2 = 0;
                while (attemptCounter2++ < maxAttempts)
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
                } 
            }

            // attempt to connect all the the rooms
            attemptCounter = 0;
            while (!AllRoomsAreReachable() && attemptCounter++ < maxAttempts)
            {
                foreach (var room in _rooms)
                {
                    // check for excessive connections to this room
                    if (room.CanAddRoad())
                    {
                        CreateRandomRoad(room);
                    }
                }
            }

            if (!AllRoomsAreConnected) return false;

            // set the start position for the player
            PlayerStartPosition = _rooms[0].Area.Center;

            // success, map has been generated, all rooms are reachable
            return true;
        }

        bool AllRoomsAreReachable()
        {
            // first test if all the rooms have at least one road
            if (_rooms.Any(room => room.HasNoRoads())) return false;

            noOfChecksForAllRoomReachability++;

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
            AllRoomsAreConnected = true;
            return true;
        }

        // Carve out a rectangular floor using the TileFloors class
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
                    Tiles[index] = new TileFloor(room);
                }
            }
        }

        void CreateFloors(List<Point> roadPoints, Road road)
        {
            foreach (var point in roadPoints)
            {
                int index = point.ToIndex(Width);
                Tiles[index] = new TileFloor(road)
                {
                    Foreground = road.Color
                };
            }
        }

        // creates a random road
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
                TileBase tile = Tiles[nextTileIndex];

                // check if the next road tile happens to be a floor -> we've reached something!
                if (tile is TileFloor t)
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

                        TileBase perpendicularTile = Tiles[perpendicularPoint.ToIndex(Width)];
                        if (perpendicularTile is TileFloor)
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
}
