using SadConsole.Entities;
using System.Collections.Generic;
using System.Linq;
using NumeralDash.Screens;

namespace NumeralDash.Entities;

class Player : Entity
{
    #region Fields
    readonly Dungeon _dungeon;
    List<Number> _numbers;
    Number _inventory = Number.Empty;
    #endregion Fields

    #region Constructors
    public Player(Point startPosition, Dungeon dungeon) : base(Color.Yellow, Color.Black, glyph: 1, (int) Layer.Player)
    {
        Name = "Player";
        Position = startPosition;
        _numbers = new();
        _dungeon = dungeon;
        dungeon.MapChanged += Dungeon_OnLevelChanged;
        FastMove.Stopped += FastMove_OnStopped;
    }
    #endregion Constructors

    #region Properties
    public bool IsMovingFast =>
        FastMove.IsOn;

    public Number LastDrop { get; private set; } = Number.Empty;

    public FastMove FastMove { get; init; } = new();

    // there is a road to the player's left or right a distance away
    public bool IsAtIntersection { get; set; } = false;

    // there is a collidable to the player's left or right a distance away
    public bool IsAbeamCollidable { get; set; } = false;

    // the new tile just entered contained a collidable
    public bool EncounteredCollidable { get; set; } = false;

    public Number Inventory
    {
        get => _inventory;
        private set
        {
            _inventory = value;
            OnInventoryChanged(value);
        }
    }
    #endregion Properties

    #region Methods
    /// <summary>
    /// Returns the sum of the player's current position and the given direction.
    /// </summary>
    /// <param name="d"></param>
    /// <returns></returns>
    public Point GetNextMove(Direction d) => 
        Position + d;

    /// <summary>
    /// Checks if the Player's position or surrounding posititions contain a collidable entity.
    /// </summary>
    /// <param name="c"></param>
    /// <returns></returns>
    public bool IsCloseTo(ICollidable c) =>
        Position == c.Coord || Position.GetDirectionPoints().Any(p => c.CollidesWith(p));

    /// <summary>
    /// Places the number in the player's inventory or collects it if it matches Rule.NextNumber.
    /// </summary>
    /// <param name="n"></param>
    /// <returns></returns>
    public Number PickUp(Number n)
    {
        if (_numbers.Contains(n) || (Inventory == n))
        {
            throw new ArgumentException("Trying to collect a duplicate number.");
        }

        EncounteredCollidable = true;

        // check if the number can be collected and placed in the numbers list
        if (_dungeon.Rule.NextNumber != Number.Empty && _dungeon.Rule.NextNumber == n)
        {
            DepositNumber(n);

            // check if the number in the inventory can now be placed in the numbers list as well
            if (_dungeon.Rule.NextNumber != Number.Empty && _dungeon.Rule.NextNumber == Inventory)
            {
                DepositNumber(Inventory);
                Inventory = Number.Empty;
            }
            
            return Number.Empty;
        }

        // place the number in the players inventory
        else
        {
            Number drop = Inventory;
            Inventory = n;
            LastDrop = drop;
            return drop;
        }
    }

    /// <summary>
    /// Add the number to _numbers, trigger event & set next number.
    /// </summary>
    /// <param name="deposit">Number to deposit.</param>
    void DepositNumber(Number deposit)
    {
        _numbers.Add(deposit);
        OnDepositMade(deposit);
    }

    void ResetPositionData()
    {
        EncounteredCollidable = false;
        IsAtIntersection = false;
        IsAbeamCollidable = false;
    }

    void FastMove_OnStopped(object? o, EventArgs e) =>
        ResetPositionData();

    void Dungeon_OnLevelChanged(object? o, EventArgs e)
    {
        _numbers = new();
        Inventory = Number.Empty;
        ResetPositionData();
        FastMove.Reset();
    }

    protected override void OnPositionChanged(Point oldPosition, Point newPosition)
    {
        ResetPositionData();
        base.OnPositionChanged(oldPosition, newPosition);
    }

    /// <summary>
    /// Raises the InventoryChanged event.
    /// </summary>
    void OnInventoryChanged(Number number)
    {
        var args = new NumberEventArgs(number);
        InventoryChanged?.Invoke(this, args);
    }

    /// <summary>
    /// Raises the NumbersChanged event.
    /// </summary>
    void OnDepositMade(Number number)
    {
        var args = new NumberEventArgs(number);
        DepositMade?.Invoke(this, args);
    }
    #endregion Methods

    #region Events
    /// <summary>
    /// Raised when the player has collected a valid number and placed it in their numbers list;
    /// </summary>
    public event EventHandler<NumberEventArgs>? DepositMade;

    /// <summary>
    /// Raised when the player has placed a new number in their inventory.
    /// </summary>
    public event EventHandler<NumberEventArgs>? InventoryChanged;
    #endregion Events
}