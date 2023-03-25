namespace NumeralDash;

class RoomGenerationException : OverflowException
{
    public RoomGenerationException() : base("Room generation attempts limit reached.") { }
}

class RoadGenerationException : OverflowException
{
    public RoadGenerationException() : base("Road generation attempts limit reached.") { }
}

class MapGenerationException : OverflowException
{
    public MapGenerationException() : base("Map generation attempts limit reached.") { }
}