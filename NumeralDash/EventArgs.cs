using NumeralDash.Entities;
using NumeralDash.Rules;

namespace NumeralDash;

class ScoreEventArgs : EventArgs
{
    public int Score { get; init; }
    public ScoreEventArgs(int score) =>
        Score = score;
}

class PositionEventArgs : EventArgs
{
    public Point Position { get; init; }
    public PositionEventArgs(Point position) =>
        Position = position;
}

class NumberEventArgs : EventArgs
{
    public Number Number { get; init; }
    public NumberEventArgs(Number number) =>
        Number = number;
}

class LevelEventArgs : EventArgs
{
    public int Level { get; init; }
    public LevelEventArgs(int level) => 
        Level = level;
}

class RuleEventArgs : EventArgs
{
    public ICollectionRule Rule { get; init; }
    public RuleEventArgs(ICollectionRule rule) => 
        Rule = rule;
}

class TimeEventArgs : EventArgs
{
    public TimeSpan Time { get; init; }
    public TimeEventArgs(TimeSpan time) => 
        Time = time;
}

class DepositEventArgs : EventArgs
{
    public Number DepositedNumber { get; init; }
    public Number NextNumber { get; init; }
    public int NumbersCount { get; init; }
    public DepositEventArgs(Number depositedNumber, Number nextNumber, int remainingCount) =>
        (DepositedNumber, NextNumber, NumbersCount) = (depositedNumber, nextNumber, remainingCount);
}

class MapEventArgs : EventArgs
{
    public Size MapSize { get; init; }
    public Rectangle View { get; init; }
    public MapEventArgs(Size size, Rectangle view) =>
        (MapSize, View) = (size, view);
}

class MapGenEventArgs : EventArgs
{
    public int RoomGenAttempts { get; init; }
    public int RoadGenAttempts { get; init; }
    public int MapGenAttempts { get; init; }
    public MapGenEventArgs(int roomGenAttempts, int roadGenAttempts, int mapGenAttempts) =>
        (RoomGenAttempts, RoadGenAttempts, MapGenAttempts) = (roomGenAttempts, roadGenAttempts, mapGenAttempts);
}

class GameOverEventArgs : EventArgs
{
    public int Level { get; init; }
    public int Score { get; init; }
    public TimeSpan TimeTotal { get; init; }
    public GameOverEventArgs(int level, int score, TimeSpan timeTotal) =>
        (Level, Score, TimeTotal) = (level, score, timeTotal);
}