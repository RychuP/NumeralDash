using System;
using SadConsole;
using SadRogue.Primitives;
using SadConsole.Entities;
using System.Collections.Generic;
using System.Linq;
using NumeralDash.Rules;
using NumeralDash.Consoles;

namespace NumeralDash.Entities
{
    class Player : Entity
    {
        List<Number> _numbers;

        Number _inventory;

        Dungeon _dungeon;

        public Number LastDrop { get; private set; } = Number.Empty;

        public Player(Point startPosition, Dungeon dungeon) : base(Color.Yellow, Color.Black, glyph: 1, (int) Layer.Player)
        {
            Name = "Player";
            Position = startPosition;
            _numbers = new();
            _dungeon = dungeon;
            _inventory = Number.Empty;
            dungeon.LevelChanged += OnLevelChanged;
        }

        #region Position Handling

        /// <summary>
        /// Moves player's position in the given direction.
        /// </summary>
        /// <param name="d"></param>
        public void MoveInDirection(Direction d)
        {
            Position += d;
        }

        /// <summary>
        /// Returns the sum of the player's current position and the given direction.
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public Point GetNextMove(Direction d) => Position + d;

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

            // check if the number can be collected and placed in the numbers list
            else if (_dungeon.Rule.NextNumber == n)
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

        void OnLevelChanged(IRule rule, int level, string[] s)
        {
            _numbers = new();
            _inventory = Number.Empty;
        }

        #endregion
    }
}