using System.Collections.Generic;
using System.Linq;
using NumeralDash.Entities;

namespace NumeralDash.World
{
    class Room : IConnectable
    {
        // serves as an id generator
        static int counter = 0;

        #region Settings

        /// <summary>
        /// The maximum number of roads connected to a room.
        /// </summary>
        public const int RoadsLimit = 3;

        /// <summary>
        /// The maximum number of entities per room.
        /// </summary>
        public const int EntityLimit = 2;

        #endregion

        #region Storage

        /// <summary>
        /// Unique id for the object
        /// </summary>
        public int ID { get; init; }

        /// <summary>
        /// All roads connected to this room.
        /// </summary>
        readonly List<Road> _roads;

        /// <summary>
        /// Backing field for the Area property.
        /// </summary>
        Rectangle _area;

        /// <summary>
        /// An expanded rectangle area that includes the wall around the room.
        /// </summary>
        Rectangle _expandedArea;

        /// <summary>
        /// All layer 1 entities currently in this room.
        /// </summary>
        readonly List<ICollidable> _collidables;

        /// <summary>
        /// Whether this room has already been visited by the player or not.
        /// </summary>
        public bool Visited { get; set; }

        #endregion

        /// <summary>
        /// A basic, rectangular dungeon room.
        /// </summary>
        /// <param name="minSize">A minimum length that the side of a room can be.</param>
        /// <param name="maxSize">A maximum length that the side of a room can be.</param>
        public Room(int minSize, int maxSize) : base()
        {
            _roads = new();
            _collidables = new();

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

        #region Area Management

        /// <summary>
        /// A rectangle representing the room on the map.
        /// </summary>
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

        /// <summary>
        /// Generates a random position for the room within constraints of the map.
        /// </summary>
        /// <param name="mapWidth">Width of the map.</param>
        /// <param name="mapHeight">Height of the map.</param>
        public void SetRandomPosition(int mapWidth, int mapHeight)
        {
            // sets the room's X/Y Position at a random point between the edges of the map
            // x 1 and y -1 to make sure there is at least one wall tile between the room and the edge of the map
            int x = Game.Instance.Random.Next(1, mapWidth - Area.Width - 1);
            int y = Game.Instance.Random.Next(1, mapHeight - Area.Height - 1);
            Area = Area.WithPosition(new Point(x, y));
        }

        /// <summary>
        /// Checks if the room intersects another room or is too close to it (no wall between the rooms).
        /// </summary>
        /// <param name="other">Another room being checked for interference.</param>
        /// <returns></returns>
        public bool InterferesWith(Room other) => _expandedArea.Intersects(other.Area);

        /// <summary>
        /// Returns a random position within the bounds of the room area.
        /// </summary>
        /// <returns></returns>
        Point GetRandomPosition()
        {
            int x = Game.Instance.Random.Next(Area.X, Area.MaxExtentX);
            int y = Game.Instance.Random.Next(Area.Y, Area.MaxExtentY);
            return new Point(x, y);
        }

        #endregion

        #region Road Management

        /// <summary>
        /// Returns the number of roads connected to this room.
        /// </summary>
        public int RoadCount => _roads.Count;

        /// <summary>
        /// Checks if this room has no roads connected to it.
        /// </summary>
        /// <returns></returns>
        public bool HasNoRoads() => _roads.Count == 0;

        /// <summary>
        /// Checks if this room can accept another road connection.
        /// </summary>
        /// <returns></returns>
        public bool CanAddRoad() => _roads.Count < RoadsLimit;

        /// <summary>
        /// Adds a road to the room.
        /// </summary>
        /// <param name="road"></param>
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

        /// <summary>
        /// Generates a random start point for the road on the side of the room that corresponds to the given direction.
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Checks if any of the connected roads lead directly to the given destination room.
        /// </summary>
        /// <param name="room"></param>
        /// <returns></returns>
        public bool HasConnectionTo(Room room) => _roads.Any(road => road.HasConnectionTo(room));

        /// <summary>
        /// Checks if the network of rooms and roads this room is connected to contains the target.
        /// </summary>
        /// <param name="other">Target room we are searching for.</param>
        /// <param name="testedRooms">List of all rooms this recursive method has already tested.</param>
        /// <returns></returns>
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

        #endregion

        #region Number Management

        /// <summary>
        /// Checks if it's possible to add another entity to the room.
        /// </summary>
        public bool ReachedEntityLimit() => _collidables.Count >= EntityLimit;

        /// <summary>
        /// Adds a new entity to the room.
        /// </summary>
        /// <param name="e">The new entity.</param>
        /// <param name="PlayerPosition">Prevents spawning entities too close to the player.</param>
        /// <returns>True if operation was successful.</returns>
        public bool AddCollidable(ICollidable c, Player p)
        {
            int attemptCounter = 0, maxAttempts = 50;
            if (!ReachedEntityLimit())
            {
                // get a random position for the entity in the room
                do
                {
                    if (attemptCounter++ > maxAttempts) return false;
                    c.Coord = GetRandomPosition();
                }
                while (
                    // check if the position of the new number does not fall on the perimeter of the room
                    Area.PerimeterPositions().Any(p => c.CollidesWith(p)) ||

                    // check if the position is not on the same row as any of the other entities in the room
                    _collidables.Any(e => e.Coord.Y == c.Coord.Y) ||

                    // check if none of the other entities in the room would become a direct neighbour of the new entity
                    _collidables.Any(e => e.IsCloseTo(c)) ||

                    // check if the number will not spawn too close to the player position
                    p.IsCloseTo(c)
                );

                // add the number to the room
                _collidables.Add(c);

                // report success
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns an entity at the point p or null if not found.
        /// </summary>
        /// <param name="p">Position in the room.</param>
        /// <returns></returns>
        public ICollidable? GetCollidableAt(Point p) => _collidables.Find(c => c.Coords.Contains(p));

        public void RemoveNumber(Number n)
        {
            if (_collidables.Contains(n))
            {
                _collidables.Remove(n);
                n.MarkAsInvisible();
            }
            else
            {
                throw new InvalidOperationException("Attempt at removing a number from a room that doesn't contain it.");
            }
        }

        public void PlaceNumber(Number n, Point p)
        {
            if (n != Number.Empty && n != Number.Finished)
            {
                _collidables.Add(n);
                n.Coord = p;

                // if the number is large, keep moving it left until it fits inside the room
                while (Area.PerimeterPositions().Any(point => n.CollidesWith(point)))
                {
                    n.Coord = n.Coords[0] + Direction.Left;
                }

                n.MarkAsVisible();
            }
        }

        #endregion

        public override string ToString() => $"Room {ID}";

        public string GetInfo() => $"Area: {Area.Size}, Position: {Area.Position}, Roads: {RoadCount}";
    }
}
