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
    const int DefaultLevelTimeLimit = 5 * 60 + 0;               // time in seconds for the initial level
    const int TimeChangePerLevel = 0 * 60 + 10;     // by how much to reduce the time per level in seconds
    readonly Timer _timer = new(1000) { AutoReset = true };
    readonly TimeSpan OneSecond = TimeSpan.FromSeconds(1);
    TimeSpan _gameTimeTotal = TimeSpan.Zero;
    TimeSpan _time;

    // level
    bool _levelComplete = false;
    ICollectionRule _rule = new EmptyOrder();
    Renderer _entityManager;
    int _level = 0;
    int _score = 0;
    Map _map;

    // movement modifiers
    bool _shiftIsDown = false;
    bool _ctrlIsDown = false;
    #endregion Fields

    #region Constructors
    public Dungeon(int viewSizeX, int viewSizeY) : base(viewSizeX, viewSizeY, Map.DefaultSize, Map.DefaultSize)
    {
        _map = new Map();
        _timer.Elapsed += Timer_OnTimeElapsed;
        Font = Fonts.C64;

        // entity manager (temporary -> will be removed in ChangeMap)
        _entityManager = new Renderer();
        SadComponents.Add(_entityManager);

        // create a player
        Player = new Player(_map.PlayerStartPosition, this);
        Player.PositionChanged += Player_OnPositionChanged;
        Player.DepositMade += Player_OnDepositMade;

        IsVisible = false;
    }
    #endregion Constructors

    #region Properties
    public Player Player { get; init; }

    public ICollectionRule Rule
    {
        get => _rule;
        private set
        {
            _rule = value;
            OnCollectionRuleChanged(value);
        }
    }

    public int Level
    {
        get => _level;
        private set
        {
            if (value < 0)
                throw new ArgumentException("Level cannot be smaller than 1.");
            if (value != 0 && value <= _level)
                throw new ArgumentException("Levels cannot be going down or staying the same.");
            _level = value;
            OnLevelChanged(value);
        }
    }

    public int Score
    {
        get => _score;
        private set
        {
            if (value < 0)
                throw new ArgumentException("Score cannot be negative.");
            if (value != 0 && value <= _score)
                throw new ArgumentException("Score cannot be going down or staying the same.");
            _score = value;
            OnScoreChanged(value);
        }
    }

    public TimeSpan Time
    {
        get => _time;
        private set
        {
            _time = value;
            OnTimeChanged(value);
        }
    }
    #endregion Properties

    #region Methods
    // soft reset after the level generation error
    public void Retry() =>
        ChangeMap();

    // triggers various methods for debugging
    public void Debug() =>
        OnGameOver();

    // resets variables before the game starts
    public void PrepareStartup()
    {
        Level = 0;
        Score = 0;
    }

    // finishes the preperation to start after the level has been changed during rectangle animation
    public void FinishStartup()
    {
        _timer.Start();
    }

    void SetTimer()
    {
        int totalTime = DefaultLevelTimeLimit - Level * TimeChangePerLevel;
        int minutes = Convert.ToInt32(totalTime / 60);
        int seconds = totalTime - minutes * 60;
        Time = new(0, minutes, seconds);
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

    /// <summary>
    /// Generates a new map.
    /// </summary>
    public void ChangeMap()
    {
        try
        {
            _map = new(Level);
        }
        catch (MapGenerationException)
        {
            OnMapFailedToGenerate(_map.RoomGenFailedAttempts, _map.RoadGenFailedAttempts, _map.MapGenFailedAttempts);
        }
        OnMapChanged();
    }

    /// <summary>
    /// Changes cell surface and populates it with map tiles.
    /// </summary>
    void ChangeCellSurface()
    {
        var (x, y) = (ViewWidth, ViewHeight);
        Surface = new CellSurface(_map.Width, _map.Height, _map.Tiles);
        (ViewWidth, ViewHeight) = (x, y);
    }

    void ChangeEntityRenderer()
    {
        // remove prev renderer
        SadComponents.Remove(_entityManager);

        // get new renderer
        _entityManager = new Renderer();
        SadComponents.Add(_entityManager);
    }

    void SpawnPlayer()
    {
        // register player with the renderer
        _entityManager.Add(Player);

        // set player position to the new start point
        Player.Position = _map.PlayerStartPosition;
    }

    /// <summary>
    /// Spawns all numbers for the current level.
    /// </summary>
    void SpawnNumbers()
    {
        foreach (ICollidable number in Rule.Numbers)
        {
            // find a room for the new collidable entity (number)
            PlaceCollidableInRandomRoom(number);

            // register entity
            _entityManager.Add(number as Entity);

            // register number extensions if any
            if (number.Size > 1 && number is Number n)
            {
                foreach (var e in n.Extensions)
                    _entityManager.Add(e);
            }
        }
    }

    /// <summary>
    /// Spawns level exit.
    /// </summary>
    void SpawnExit()
    {
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
        Rule = CollectionRuleBase.GetNextRule(Level, _map.NumberCount);
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
                if (room.GetCollidableAt(newPosition) is Entity entity)
                {
                    // check if player is ignoring numbers while auto moving
                    if (!(Player.IsMovingFast && _ctrlIsDown) &&
                        // check if the player is walking over a long, multicell number and prevent that type of collections
                        entity is Number number && !number.Coords.Contains(prevPosition))
                    {
                        // player can collect the number
                        room.RemoveNumber(number);
                        Number drop = Player.PickUp(number);
                        room.PlaceNumber(drop, number.Position);
                        Sounds.PickUp.Play();
                    }

                    // check if the level is completed
                    else if (entity is Exit)
                    {
                        if (Rule.NextNumber == Number.Empty)
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
        // update view position
        Point prevViewPosition = Surface.View.Position;
        Surface.View = View.WithCenter(Player.Position);
        if (prevViewPosition != Surface.View.Position)
            OnViewPositionChanged(prevViewPosition, Surface.View.Position);

        // update player position
        if (!_levelComplete && Player.IsMovingFast)
        {
            // stop player when encountered collectible or fast move stop modifier conditions are met
            if ((_ctrlIsDown && Player.IsAtIntersection) ||
                (_shiftIsDown && Player.EncounteredCollidable) ||
                (_shiftIsDown && Player.IsAbeamCollidable))
                    Player.FastMove.Stop();

            // try moving player in the fast move direction
            else if (!TryMovePlayer(Player.FastMove.Direction))
                Player.FastMove.Stop();
        }

        base.Update(delta);
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

    void Player_OnDepositMade(object? o, NumberEventArgs e)
    {
        Score += e.Number;
        OnDepositMade(e.Number);
    }

    void Player_OnPositionChanged(object? o, EventArgs e)
    {
        // calculate fast move stop points
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
        Level++;
    }

    void OnMapFailedToGenerate(int roomGenAttempts, int roadGenAttempts, int mapGenAttempts)
    {
        var args = new MapGenEventArgs(roomGenAttempts, roadGenAttempts, mapGenAttempts);
        MapFailedToGenerate?.Invoke(this, args);
    }

    void Timer_OnTimeElapsed(object? o, ElapsedEventArgs e)
    {
        Time -= OneSecond;
        _gameTimeTotal += OneSecond;

        if (Time == TimeSpan.Zero)
        {
            Pause();
            OnGameOver();
        }
    }

    void OnMapChanged()
    {
        _levelComplete = false;

        ChangeCellSurface();
        ChangeEntityRenderer();
        ChangeRule();
        SpawnPlayer();
        SpawnNumbers();
        SpawnExit();
        SetTimer();

        var size = new Size(Surface.Width, Surface.Height);
        var args = new MapEventArgs(size, Surface.View);
        MapChanged?.Invoke(this, args);
    }

    void OnGameOver()
    {
        var args = new GameOverEventArgs(Level, Score, _gameTimeTotal);
        GameOver?.Invoke(this, args);
    }

    void OnCollectionRuleChanged(ICollectionRule rule)
    {
        var args = new RuleEventArgs(rule);
        RuleChanged?.Invoke(this, args);
    }

    void OnDepositMade(Number number)
    {
        Rule.Dequeue();

        var args = new DepositEventArgs(number, Rule.NextNumber, Rule.Numbers.Count);
        DepositMade?.Invoke(this, args);
    }

    void OnScoreChanged(int score)
    {
        var args = new ScoreEventArgs(score);
        ScoreChanged?.Invoke(this, args);
    }

    void OnLevelChanged(int level)
    {
        var args = new LevelEventArgs(level);
        LevelChanged?.Invoke(this, args);
    }

    void OnTimeChanged(TimeSpan time)
    {
        var args = new TimeEventArgs(time);
        TimeChanged?.Invoke(this, args);
    }

    void OnViewPositionChanged(Point prevPosition, Point newPosition)
    {
        var args = new PositionEventArgs(newPosition);
        ViewPositionChanged?.Invoke(this, args);
    }
    #endregion Methods

    #region Events
    // map events
    public event EventHandler<LevelEventArgs>? LevelChanged;
    public event EventHandler<RuleEventArgs>? RuleChanged;
    public event EventHandler<MapEventArgs>? MapChanged;
    public event EventHandler<MapGenEventArgs>? MapFailedToGenerate;

    // gameplay events
    public event EventHandler<TimeEventArgs>? TimeChanged;
    public event EventHandler<ScoreEventArgs>? ScoreChanged;
    public event EventHandler<DepositEventArgs>? DepositMade;
    public event EventHandler<PositionEventArgs>? ViewPositionChanged;
    public event EventHandler<GameOverEventArgs>? GameOver;
    public event EventHandler? LevelCompleted;
    #endregion Events
}