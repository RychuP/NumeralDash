using SadConsole.Entities;
using NumeralDash.World;
using NumeralDash.Entities;
using System.Linq;
using NumeralDash.Rules;
using SadConsole.Input;
using System.Timers;

namespace NumeralDash.Consoles;

class Dungeon : Console
{
    #region Fields
    //const int levelTime = 0 * 60 + 5;
    const int LevelTime = 5 * 60 + 0;               // time in seconds for the initial level
    const int TimeChangePerLevel = 0 * 60 + 10;     // by how much to reduce the time per level in seconds

    // fields
    readonly Timer _timer = new(1000) { AutoReset = true };
    readonly TimeSpan _oneSecond = new(0, 0, 1);
    TimeSpan _totalTimePlayed = TimeSpan.Zero;
    Renderer _entityManager;
    TimeSpan _time;
    int _level = 0;
    Map _map;
    #endregion Fields

    #region Constructors
    public Dungeon(int viewSizeX, int viewSizeY) : base(viewSizeX, viewSizeY, Map.DefaultSize, Map.DefaultSize)
    {
        _map = new Map();
        _timer.Elapsed += OnTimeElapsed;
        Font = Fonts.C64;

        // entity manager (temporary -> will be removed in ChangeMap)
        _entityManager = new Renderer();
        SadComponents.Add(_entityManager);

        // set a temp rule
        Rule = new EmptyOrder();

        // create a player
        Player = new Player(_map.PlayerStartPosition, this);
        SadComponents.Add(new SadConsole.Components.SurfaceComponentFollowTarget() { Target = Player });

        IsVisible = false;
    }
    #endregion Constructors

    #region Properties
    public Player Player { get; init; }
    public ICollectionRule Rule { get; private set; }
    #endregion Properties

    #region Methods
    // hard reset
    public void Restart() =>
        Start();

    // soft reset after the level generation error
    public void Retry() =>
        ChangeLevel();

    public void Start()
    {
        _level = 0;
        ChangeLevel();
    }

    void ChangeLevel()
    {
        try
        {
            _map = new(_level++);
            ChangeMap();
            ChangeRule();
            SpawnEntities();
            StartTimer();
            OnLevelChanged();
        }
        catch (MapGenerationException e)
        {
            IsVisible = false;
            OnMapFailedToGenerate(e.FailedAttempts);
        }
    }

    void StartTimer()
    {
        int totalTime = LevelTime - (_level - 1) * TimeChangePerLevel;
        int minutes = Convert.ToInt32(totalTime / 60);
        int seconds = totalTime - minutes * 60;
        _time = new(0, minutes, seconds);
        _timer.Start();
    }

    public void Pause()
    {
        _timer.Stop();
        IsVisible = false;
    }

    public void Resume()
    {
        _timer.Start();
        IsVisible = true;
    }

    void ChangeMap()
    {
        // get new surface
        int x = ViewWidth, y = ViewHeight;
        Surface = new CellSurface(_map.Width, _map.Height, _map.Tiles);
        ViewWidth = x;
        ViewHeight = y;

        // remove prev renderer
        SadComponents.Remove(_entityManager);

        // get new renderer
        _entityManager = new Renderer();
        SadComponents.Add(_entityManager);

        // register player with the renderer
        _entityManager.Add(Player);
    }

    /// <summary>
    /// Spawns entities (all numbers and the exit).
    /// </summary>
    void SpawnEntities()
    {
        // reposition player to the new start point
        Player.Position = _map.PlayerStartPosition;

        // spawn numbers
        for (int i = 0; i < Rule.Numbers.Length; i++)
        {
            // find a room for the new collidable entity (number)
            ICollidable c = Rule.Numbers[i];
            PlaceCollidableInRandomRoom(c);

            // register entity
            _entityManager.Add(c as Entity);

            // register number extensions if any
            if (c.Size > 1 && c is Number n)
            {
                foreach (var e in n.Extensions)
                {
                    _entityManager.Add(e);
                }
            }
        }

        // spawn exit
        var exit = new Exit();
        PlaceCollidableInRandomRoom(exit);
        _entityManager.Add(exit);
    }

    void PlaceCollidableInRandomRoom(ICollidable c)
    {
        Room room;
        if (_map.Rooms.Any(room => !room.ReachedEntityLimit()))
        {
            do room = _map.GetRandomRoom();
            while (!room.AddCollidable(c, Player));
        }
        else
        {
            throw new ArgumentException($"Excessive number of entities. No room can accept {c}.");
        }
    }

    /// <summary>
    /// Selects a new rule for number collections.
    /// </summary>
    void ChangeRule()
    {
        Rule = CollectionRuleBase.GetNextRule(_level, _map.NumberCount);
    }    

    /// <summary>
    /// Tries to move the player by one tile in the given direction.
    /// </summary>
    /// <param name="direction">Direction of travel.</param>
    /// <returns>True if the move succeeded, otherwise false.</returns>
    public bool TryMovePlayer(Direction d)
    {
        Point newPosition = Player.GetNextMove(d);
        if (_map.IsWalkable(newPosition, out Room? room))
        {
            var prevPosition = Player.Position;
            Player.Position = newPosition;

            // check if the tile belongs to a room
            if (room is not null)
            {
                if (!room.Visited) room.Visited = true;

                // look for entities at the player's position
                if (room.GetCollidableAt(newPosition) is Entity e)
                {
                    // check if the player is walking over a long, multicell number and prevent that type of collections
                    if (e is Number n && !n.Coords.Contains(prevPosition))
                    {
                        // player can collect the number
                        room.RemoveNumber(n);
                        Number drop = Player.PickUp(n);
                        room.PlaceNumber(drop, n.Position);
                    }

                    // check if the level is completed
                    else if (e is Exit)
                    {
                        if (Rule.NextNumber == Number.Finished)
                        {
                            _timer.Stop();
                            ChangeLevel();
                        }
                        else
                        {
                            Player.EncounteredCollidable = true;
                        }
                    }
                }
            }

            return true;
        }

        return false;
    }

    public override void Update(TimeSpan delta)
    {
        base.Update(delta);

        if (Player.IsMovingFast && !Player.EncounteredCollidable)
        {
            bool success = TryMovePlayer(Player.FastMove.Direction);
            if (!success) Player.FastMove.Stop();
        }
    }

    public new bool ProcessKeyboard(Keyboard keyboard)
    {
        if (keyboard.HasKeysPressed)
        {
            Direction direction = keyboard.GetDirection();

            // auto move stopping only at collidables
            if (keyboard.IsKeyDown(Keys.LeftShift) && !keyboard.IsKeyDown(Keys.LeftControl))
            {
                if (direction != Direction.None)
                {
                    if (Player.IsMovingFast)
                    {
                        Player.FastMove.Direction = direction;
                        return true;
                    }
                    else
                    {
                        Player.FastMove.Start(direction);
                        return true;
                    }
                }
            }

            // auto move stopping at road intersections
            else if (keyboard.IsKeyDown(Keys.LeftControl) && !keyboard.IsKeyDown(Keys.LeftShift))
            {

            }

            // auto move stopping at road and collidable intersections
            else if (keyboard.IsKeyDown(Keys.LeftControl) && keyboard.IsKeyDown(Keys.LeftShift))
            {

            }

            // normal move by one tile without any modifiers
            if (!Player.IsMovingFast)
            {
                if (direction != Direction.None && TryMovePlayer(direction))
                    return true;
            }

            // check for fast move key releases
            else
            {
                if (keyboard.HasKeysReleased() && FastMoveKeysReleased(keyboard))
                {
                    Player.FastMove.Stop();
                    return true;
                }
            }
        }

        // check for fast move key releases
        else if (keyboard.HasKeysReleased() && Player.IsMovingFast && FastMoveKeysReleased(keyboard))
        {
            Player.FastMove.Stop();
            return true;
        }

        // no meaningful key presses
        return false;
    }

    // checks if none of the fast move keys are down and one of them was released this frame
    bool FastMoveKeysReleased(Keyboard keyboard)
    {
        return !keyboard.IsKeyDown(Keys.LeftShift) && !keyboard.IsKeyDown(Keys.LeftControl) &&
            (keyboard.IsKeyReleased(Keys.LeftShift) || keyboard.IsKeyReleased(Keys.LeftControl));
    }

    void OnLevelChanged()
    {
        var mapGenerationInfo = new string[]
        {
            $"Dungeon level: {_level}, number of rooms: {_map.Rooms.Count}, map size: {Area.Size}."
        };

        LevelChanged?.Invoke(Rule, _level, mapGenerationInfo);

        // invoke this as well to show the timer as soon as the new level is displayed (otherwise there is a 1 sec delay)
        TimeElapsed?.Invoke(_time);
    }

    void OnMapFailedToGenerate(AttemptCounters failedAttempts)
    {
        MapFailedToGenerate?.Invoke(failedAttempts);
    }

    void OnTimeElapsed(object? o, ElapsedEventArgs e)
    {
        _time -= _oneSecond;
        _totalTimePlayed += _oneSecond;

        if (_time == TimeSpan.Zero)
        {
            Pause();
            OnGameOver();
        }

        TimeElapsed?.Invoke(_time);
    }

    void OnGameOver()
    {
        GameOver?.Invoke(_level, _totalTimePlayed);
    }
    #endregion

    #region Events
    public event Action<TimeSpan>? TimeElapsed;
    public event Action<int, TimeSpan>? GameOver;
    public event Action<AttemptCounters>? MapFailedToGenerate;
    public event Action<ICollectionRule, int, string[]>? LevelChanged;
    public event Action? PlayerMoved;
    #endregion Events
}