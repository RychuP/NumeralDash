using System;
using SadConsole;
using SadRogue.Primitives;
using System.Collections.Generic;
using System.Linq;

namespace NumeralDash.World
{
    class Road : IConnectable
    {
        // serves as an id generator
        static int counter = 0;

        // settings
        public const int MaxLegLength = 30,
            MinLegLength = 5,
            MaxLegsPerRoom = 2,
            MaxRooms = 4,                       // max rooms that are connected to this road
            MaxLengthPerRoom = 40;              // max total length for the road per connected room

        // number of legs this road consists of
        int _legs = 0;

        public int ID { get; init; }

        // list of rooms connected to this road
        List<Room> _rooms { get; init; }

        // list of all the points that form this road
        List<Point> _points { get; init; }

        // color of the dots that form the road on the map
        public Color Color { get; init; }

        public Road(List<Point> points, Room room, Room destination, int legs)
        {
            int roomCount = 2;

            if (points.Count > MaxLengthPerRoom * roomCount)
            {
                throw new ArgumentException("Attempt at creating a road that exceeds the maximum allowed length.");
            }

            if (legs > MaxLegsPerRoom * roomCount)
            {
                throw new ArgumentException("Attempt at creating a road with too many legs.");
            }

            // generate an id
            ID = counter++;

            // initiate fields and properties
            _rooms = new() { room, destination };
            _points = points;
            _legs = legs;
            do Color = Program.GetRandomColor();
            while (Color.GetBrightness() < Program.MinimumColorBrightness);

            // add the road to the rooms
            room.AddRoad(this);
            destination.AddRoad(this);
        }

        public bool AddLeg(List<Point> points, Room room, int legs)
        {
            int roomCount = _rooms.Count + 1;

            if (roomCount > MaxRooms) return false;

            if (_legs + legs > MaxLegsPerRoom * roomCount) return false;

            _rooms.Add(room);
            _points.AddRange(points);
            room.AddRoad(this);
            _legs += legs;
            return true;
        }

        // returns true if the road is directly connected to the parameter room
        public bool HasConnectionTo(Room other) => _rooms.Any(room => room == other);

        // returns true if the road is connected to a room that has another road with direct connection to the parameter room
        public bool HasRoomWithConnectionTo(Room other) => _rooms.Any(room => room.HasConnectionTo(other));

        // returns true if any of the directly connected rooms can reach the parameter room
        public bool HasRoomThatCanReach(Room other, ref List<Room> testedRooms)
        {
            foreach (var room in _rooms)
            {
                if (testedRooms.Contains(room)) continue;
                else if (room.CanReach(other, ref testedRooms)) return true;
            }

            return false;
        }

        public static int GetRandomLegLength() => Game.Instance.Random.Next(MinLegLength, MaxLegLength);

        public override string ToString() => $"Road {ID}";

        public string GetInfo() => $"Connects: {_rooms.Count} rooms, Length: {_points.Count}, Legs: {_legs}, Color: {Color.ToParser()}";


        #region Static Direction Helper Methods

        public static Direction[] Directions = new Direction[]
        {
            Direction.Left,
            Direction.Right,
            Direction.Up,
            Direction.Down
        };

        public static Direction GetRandomDirection()
        {
            int index = Game.Instance.Random.Next(0, Directions.Length - 1);
            return Directions[index];
        }

        public static Direction GetRandomDirection(Direction[] d)
        {
            int index = Game.Instance.Random.Next(0, d.Length - 1);
            return d[index];
        }

        public static Direction[] GetPerpendicularDirections(Direction d)
        {
            Direction[] directionsUD = { Direction.Up, Direction.Down },
                directionsLR = { Direction.Left, Direction.Right };

            return d.Type switch
            {
                Direction.Types.Left => directionsUD,
                Direction.Types.Right => directionsUD,
                Direction.Types.Up => directionsLR,
                _ => directionsLR
            };
        }
        #endregion
    }
}
