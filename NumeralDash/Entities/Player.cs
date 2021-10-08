using System;
using SadConsole;
using SadRogue.Primitives;
using SadConsole.Entities;
using System.Collections.Generic;
using System.Linq;
using NumeralDash.Rules;

namespace NumeralDash.Entities
{
    class Player : Entity
    {
        List<Number> _numbers;

        Number _inventory;

        IRule _collectionOrder;

        public Player(Point startPosition, IRule collectionOrder) : base(Color.Yellow, Color.Black, glyph: 1, (int) Layer.Player)
        {
            _numbers = new();
            _collectionOrder = collectionOrder;
            _inventory = Number.Empty;
            Position = startPosition;
        }

        #region Position Handling

        /// <summary>
        /// Changes the player's position.
        /// </summary>
        /// <param name="newPosition"></param>
        public void MoveTo(Point newPosition)
        {
            Position = newPosition;
        }

        /// <summary>
        /// Returns the sum of the player's current position and the given direction.
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public Point GetNextMove(Point direction)
        {
            return Position.Translate(direction);
        }

        #endregion

        #region Number Handling

        public Number Collect(Number n)
        {
            if (n.Position != Position)
            {
                throw new ArgumentException("Number's position is not the same as player's position.");
            }

            if (_numbers.Contains(n) || (_inventory == n))
            {
                throw new ArgumentException("Trying to collect a duplicate number.");
            }

            // check if the number can be collected and placed in the numbers list
            else if (_numbers.Count > 0 && _collectionOrder.GetNext(_numbers.Last()) == n)
            {
                _numbers.Add(n);

                // check if the number in the inventory can now be placed in the numbers list as well
                if (_collectionOrder.GetNext(_numbers.Last()) == _inventory)
                {
                    _numbers.Add(_inventory);
                    _inventory = Number.Empty;
                    OnInventoryChanged();
                }
                OnNumbersChanged();
                return Number.Empty;
            }

            // place the number in the players inventory
            else
            {
                Number temp = _inventory;
                _inventory = n;
                OnInventoryChanged();
                return temp;
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Raises the InventoryChanged event.
        /// </summary>
        void OnInventoryChanged()
        {
            InventoryChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the NumbersChanged event.
        /// </summary>
        void OnNumbersChanged()
        {
            NumbersChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raised when the player has placed a new number in their inventory.
        /// </summary>
        public event EventHandler? InventoryChanged;

        /// <summary>
        /// Raised when the player has collected a valid number and placed it in their numbers list;
        /// </summary>
        public event EventHandler? NumbersChanged;

        #endregion
    }
}