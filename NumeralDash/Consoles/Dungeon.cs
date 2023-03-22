using SadConsole.Entities;
using NumeralDash.World;
using NumeralDash.Entities;
using System.Linq;
using NumeralDash.Rules;
using SadConsole.Input;
using System.Timers;
using NumeralDash.Tiles;

namespace NumeralDash.Consoles;

class Dungeon : Console
{
    #region Fields
    // timer
    const int LevelTime = 5 * 60 + 0;               // time in seconds for the initial level
    const int TimeChangePerLevel = 0 * 60 + 10;     // by how much to reduce the time per level in seconds
    readonly Timer _timer = new(1000) { AutoReset = true };
    readonly TimeSpan _oneSecond = new(0, 0, 1);
    TimeSpan _totalTimePlayed = TimeSpan.Zero;
    TimeSpan _time;

    // level
    bool _levelComplete = false;
    Renderer _entityManager;
    int _level = 0;
    Map _map;

    // movement modifiers
    bool _shiftIsDown = false;
    bool _ctrlIsDown = false;
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
        Player.PositionChanged += Player_OnPositionChanged;
        SadComponents.Add(new SadConsole.Components.SurfaceComponentFollowTarget() { Target = Player });

        IsVisible = false;
    }
    #endregion Constructors

    #region Properties
    public Player Player { get; init; }
    public ICollectionRule Rule { get; private set; }
    #endregion Properties

    #region Methods
    // soft reset after the level generation error
    public void Retry() =>
        ChangeLevel();

    public void Debug() =>
        OnGameOver();

    public void Start()
    {
        _level = 0;
        _levelComplete = false;
        ChangeLevel();
        StartTimer();
        OnLevelChanged();
    }

    public void StartAfterAnimation()
    {
        _levelComplete = false;
        StartTimer();
        OnLevelChanged();
    }

    public void PrepareToStart()
    {
        _level = 0;
    }

    public void ChangeLevel()
    {
        try
        {
            _map = new(_level++);
            ChangeMap();
            ChangeRule();
            SpawnEntities();
            Surface.View = Surface.View.WithCenter(Player.AbsolutePosition);
        }
        catch (MapGenerationException e)
        {
            IsVisible = false;
            _level--;
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
            Sounds.Step.Play();

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
                        Sounds.PickUp.Play();
                    }

                    // check if the level is completed
                    else if (e is Exit)
                    {
                        if (Rule.NextNumber == Number.Finished)
                            OnLevelCompleted();
                        else
                            Player.EncounteredCollidable = true;
                    }
                }
            }

            return true;
        }

        return false;
    }

    public override void Update(TimeSpan delta)
    {
        if (_levelComplete) return;
        base.Update(delta);

        if (Player.IsMovingFast)
        {
            // stop player when encountered collectible or fast move stop modifier conditions are met
            if (Player.EncounteredCollidable || 
                (_ctrlIsDown && Player.IsAtIntersection) ||
                (_shiftIsDown && Player.IsAbeamCollidable))
                    Player.FastMove.Stop();

            // try moving player in the fast move direction
            else if (!TryMovePlayer(Player.FastMove.Direction))
                Player.FastMove.Stop();
        }
    }

    public new bool ProcessKeyboard(Keyboard keyboard)
    {
        if (keyboard.HasKeysPressed)
        {
            Direction direction = keyboard.GetDirection();
            _shiftIsDown = keyboard.IsKeyDown(Keys.LeftShift);
            _ctrlIsDown = keyboard.IsKeyDown(Keys.LeftControl);

            // fast move modifiers
            if (_shiftIsDown || _ctrlIsDown)
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
        return !_shiftIsDown && !_ctrlIsDown &&
            (keyboard.IsKeyReleased(Keys.LeftShift) || keyboard.IsKeyReleased(Keys.LeftControl));
    }

    void Player_OnPositionChanged(object? o, EventArgs e)
    {
        if (Player.IsMovingFast)
        {
            var playerLeft = Player.FastMove.Direction - 2;

            // look for intersections at the new position in 2 directions: left and right
            for (int i = 0; i <= 4; i += 4)
            {
                // get direction and initial tile
                var direction = playerLeft + i;
                var position = Player.Position;

                while (true) 
                {
                    position += direction;
                    Tile tile = _map.GetTile(position);

                    if (tile is Floor floor)
                    {
                        if (floor.Parent is Road)
                            Player.IsAtIntersection = true;

                        else if (floor.Parent is Room room && room.GetCollidableAt(position) is not null)
                            Player.IsAbeamCollidable = true;

                        if (Player.IsAtIntersection && Player.IsAbeamCollidable)
                            return;
                    }
                    else break;
                }
            }
        }
    }

    void OnLevelCompleted()
    {
        _timer.Stop();
        _levelComplete = true;
        LevelCompleted?.Invoke(this, EventArgs.Empty);
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
    public event EventHandler? LevelCompleted;
    #endregion Events
}