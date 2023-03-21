using SadConsole.Entities;
using System.Collections.Generic;
using System.Linq;
using NumeralDash.Rules;
using NumeralDash.Consoles;

namespace NumeralDash.Entities;

class Player : Entity
{
    List<Number> _numbers;
    Number _inventory;
    readonly Dungeon _dungeon;
    bool _encounteredCollidable = false;

    public Number LastDrop { get; private set; } = Number.Empty;

    public FastMove FastMove { get; init; } = new();

    public Point PrevPosition { get; private set; }

    public Player(Point startPosition, Dungeon dungeon) : base(Color.Yellow, Color.Black, glyph: 1, (int) Layer.Player)
    {
        Name = "Player";
        Position = startPosition;
        PrevPosition = startPosition;
        _numbers = new();
        _dungeon = dungeon;
        _inventory = Number.Empty;
        dungeon.LevelChanged += Dungeon_OnLevelChanged;
        FastMove.Started += FastMove_OnStarted;
    }

    public bool EncounteredCollidable
    {
        get => _encounteredCollidable;
        set
        {
            _encounteredCollidable = value;
            OnEncounteredCollidableChanged(value);
        }
    }

    #region Position Handling
    public bool IsMovingFast =>
        FastMove.IsOn;

    /// <summary>
    /// Adds direction to the player's position.
    /// </summary>
    /// <param name="d"></param>
    public void MoveInDirection(Direction d)
    {
        PrevPosition = Position;
        Position += d;
        EncounteredCollidable = false;
    }

    /// <summary>
    /// Returns the sum of the player's current position and the given direction.
    /// </summary>
    /// <param name="d"></param>
    /// <returns></returns>
    public Point GetNextMove(Direction d) => Position + d;

    /// <summary>
    /// Checks if the Player's position or surrounding posititions contain a collidable entity.
    /// </summary>
    /// <param name="c"></param>
    /// <returns></returns>
    public bool IsCloseTo(ICollidable c)
    {
        return Position == c.Coord || Position.GetDirectionPoints().Any(p => c.CollidesWith(p));
    }

    #endregion

    #region Number Handling
    /// <summary>
    /// Places the number in the player's inventory or collects it if it matches Rule.NextNumber.
    /// </summary>
    /// <param name="n"></param>
    /// <returns></returns>
    public Number PickUp(Number n)
    {
        if (_numbers.Contains(n) || (_inventory == n))
        {
            throw new ArgumentException("Trying to collect a duplicate number.");
        }

        EncounteredCollidable = true;

        // check if the number can be collected and placed in the numbers list
        if (_dungeon.Rule.NextNumber == n)
        {
            DepositNumber(n);

            // check if the number in the inventory can now be placed in the numbers list as well
            if (_dungeon.Rule.NextNumber == _inventory)
            {
                DepositNumber(_inventory);
                _inventory = Number.Empty;
                OnInventoryChanged();
            }
            
            return Number.Empty;
        }

        // place the number in the players inventory
        else
        {
            Number drop = _inventory;
            _inventory = n;
            OnInventoryChanged();
            LastDrop = drop;
            return drop;
        }
    }

    /// <summary>
    /// Add the number to _numbers, trigger event & set next number.
    /// </summary>
    /// <param name="n">Number to deposit.</param>
    void DepositNumber(Number n)
    {
        _numbers.Add(n);
        _dungeon.Rule.SetNextNumber();
        OnDepositMade();
    }

    #endregion

    #region Events

    void FastMove_OnStarted(object? o, EventArgs e)
    {
        EncounteredCollidable = false;
    }

    void FastMove_OnStopped(object? o, EventArgs e)
    {
        
    }

    /// <summary>
    /// Raises the InventoryChanged event.
    /// </summary>
    void OnInventoryChanged()
    {
        InventoryChanged?.Invoke(_inventory);
    }

    /// <summary>
    /// Raised when the player has placed a new number in their inventory.
    /// </summary>
    public event Action<Number>? InventoryChanged;

    /// <summary>
    /// Raises the NumbersChanged event.
    /// </summary>
    void OnDepositMade()
    {
        var lastNumber = _numbers.Last();
        var totalNumbersCollected = _numbers.Count;
        DepositMade?.Invoke(lastNumber, totalNumbersCollected);
    }

    /// <summary>
    /// Raised when the player has collected a valid number and placed it in their numbers list;
    /// </summary>
    public event Action<Number, int>? DepositMade;

    void Dungeon_OnLevelChanged(ICollectionRule rule, int level, string[] s)
    {
        _numbers = new();
        _inventory = Number.Empty;
        FastMove.Reset();
    }

    void OnEncounteredCollidableChanged(bool newValue)
    {
        if (IsMovingFast && newValue == true)
            FastMove.Stop();
    }
    #endregion
}

class FastMove
{
    Direction _direction = Direction.None;
    bool _isOn = false;

    public void Reset() =>
        Stop();

    public void Start(Direction direction)
    {
        if (direction == Direction.None)
            throw new ArgumentException("Invalid start direction.");
        Direction = direction;
        IsOn = true;
    }

    public void Stop()
    {
        Direction = Direction.None;
        IsOn = false;
    }

    public void ChangeDirection(Direction direction) =>
        Direction = direction;

    public Direction Direction
    {
        get => _direction;
        set
        {
            if (value != Direction.None && !value.IsCardinal())
                throw new ArgumentException("Invalid direction.");
            _direction = value;
        }
    }

    public bool IsOn
    {
        get => _isOn;
        set
        {
            bool prevValue = _isOn;
            _isOn = value;
            OnIsOnChanged(prevValue, value);
        }
    }

    void OnIsOnChanged(bool prevValue, bool newValue)
    {
        if (prevValue == true && newValue == false)
            Stopped?.Invoke(this, EventArgs.Empty);
        else if (prevValue == false && newValue == true)
            Started?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler? Started;
    public event EventHandler? Stopped;
}