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
    // settings
    //const int levelTime = 0 * 60 + 5;
    const int LevelTime = 5 * 60 + 0;               // time in seconds for the initial level
    const int TimeChangePerLevel = 0 * 60 + 10;     // by how much to reduce the time per level in seconds

    #region Storage

    // fields
    readonly Timer _timer = new(1000) { AutoReset = true };
    readonly TimeSpan _oneSecond = new(0, 0, 1);
    TimeSpan _totalTimePlayed = TimeSpan.Zero;
    Renderer _entityManager;
    TimeSpan _time;
    int _level = 0;
    Map _map;

    // public properties
    public Player Player { get; init; }

    public ICollectionRule Rule { get; private set; }

    #endregion

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

    #region Level Management

    public void Restart()
    {
        _totalTimePlayed = TimeSpan.Zero;
        _level = 0;
        Start();
    }

    public void Retry()
    {
        ChangeLevel();
    }

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
        Rule = CollectionRuleBase.GetRandomRule(_map.NumberCount);
    }

    #endregion

    #region Player Management

    Point MovePlayer(Direction d)
    {
        Player.MoveInDirection(d);
        OnPlayerMoved();
        return Player.PrevPosition;
    }

    /// <summary>
    /// Tries to move the player by one tile in the given direction.
    /// </summary>
    /// <param name="direction">Direction of travel.</param>
    /// <returns>True if the move succeeded, otherwise false.</returns>
    public bool TryMovePlayer(Direction d)
    {
        Point tileCoord = Player.GetNextMove(d);
        if (_map.TileIsWalkable(tileCoord, out Room? room))
        {
            Point playerPrevPosition = MovePlayer(d);

            // check if the tile belongs to a room
            if (room is not null)
            {
                if (!room.Visited) room.Visited = true;

                // look for entities at the player's position
                if (room.GetCollidableAt(tileCoord) is Entity e)
                {
                    // check if the player is not walking over a long, multicell number and prevent that type of collections
                    if (e is Number n && !n.Coords.Contains(playerPrevPosition))
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
                            Player.SetEncounteredCollidable();
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

        if (Player.IsMovingFast)
        {
            bool success = TryMovePlayer(Player.FastMove.Direction);
            if (!success) Player.StopFastMove();
        }
    }

    public new void ProcessKeyboard(Keyboard keyboard)
    {
        // fast move with a left shift modifier
        if (keyboard.HasKeysDown)
        {
            if (keyboard.IsKeyDown(Keys.LeftShift))
            {
                if (keyboard.KeysDown.Count > 1)
                {
                    if (Player.IsMovingFast)
                    {
                        Player.FastMove.ChangeDirection(keyboard.GetDirectionFromKeysDown());
                    }
                    else
                    {
                        if (Player.EncounteredCollidable)
                        {
                            var direction = keyboard.GetDirectionFromKeysDown();
                            if (direction != Player.FastMove.Direction || (direction == Player.FastMove.Direction && Player.FastMove.IsReleased))
                            {
                                Player.StartFastMove(keyboard.GetDirectionFromKeysDown());
                            }
                            
                        }
                        else
                        {
                            Player.StartFastMove(keyboard.GetDirectionFromKeysDown());
                        }
                    }
                }
            }
        }

        // normal move by one tile
        if (keyboard.HasKeysPressed)
        {
            if ( (!Player.IsMovingFast && !Player.EncounteredCollidable) || (Player.EncounteredCollidable && !keyboard.IsKeyDown(Keys.LeftShift)) )
                TryMovePlayer(keyboard.GetDirectionFromKeysPressed());
        }

        if (Player.EncounteredCollidable && keyboard.IsKeyReleased(Player.FastMove.Direction.ToKey()))
        {
            Player.FastMove.IsReleased = true;
        }

        // doesn't work
        else if (Player.IsMovingFast && keyboard.IsKeyReleased(Keys.LeftShift))
        {
            Player.StopFastMove();
        }
    }

    #endregion

    #region Events

    void OnPlayerMoved()
    {
        // PlayerMoved?.Invoke(_map.GetTileInfo(Player.Position));
        PlayerMoved?.Invoke();
    }

    public event Action? PlayerMoved;

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

    public event Action<ICollectionRule, int, string[]>? LevelChanged;

    void OnMapFailedToGenerate(AttemptCounters failedAttempts)
    {
        MapFailedToGenerate?.Invoke(failedAttempts);
    }

    public event Action<AttemptCounters>? MapFailedToGenerate;

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

    public event Action<TimeSpan>? TimeElapsed;

    void OnGameOver()
    {
        GameOver?.Invoke(_level, _totalTimePlayed);
    }

    public event Action<int, TimeSpan>? GameOver;

    #endregion
}
