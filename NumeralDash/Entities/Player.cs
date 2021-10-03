using System;
using SadConsole;
using SadRogue.Primitives;
using SadConsole.Entities;
using System.Collections.Generic;
using System.Linq;

namespace NumeralDash.Entities
{
    class Player : Entity
    {
        List<Number> _numbers;

        Number _inventory;

        public Player(Point startPosition) : base(Color.Yellow, Color.Black, glyph: 1, zIndex : 2)
        {
            _numbers = new();
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
            if (n is null)
            {
                throw new ArgumentException("Null cannot be passed to this method.");
            }

            if (n.Position != Position)
            {
                throw new ArgumentException("Number's position is not the same as player's position.");
            }

            if (_numbers.Contains(n) || (_inventory is not null && _inventory == n))
            {
                throw new ArgumentException("Trying to collect a duplicate number.");
            }

            // check if the number can be collected and placed in the numbers list
            else if (n == 1 || (_numbers.Count > 0 &&  n == _numbers.Last().Next))
            {
                _numbers.Add(n);
                if (_inventory == _numbers.Last().Next)
                {
                    _numbers.Add(_inventory);
                    _inventory = null;
                    OnInventoryChanged();
                }
                OnNumbersChanged();
                return null;
            }

            // place the number in the players inventory
            else
            {
                Number temp = _inventory is Number ? _inventory : null;
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
        /// Indicates that the player has placed a new number in their temporary inventory.
        /// </summary>
        public event EventHandler InventoryChanged;

        /// <summary>
        /// Indicates that the player has collected a valid number and placed it in their numbers list;
        /// </summary>
        public event EventHandler NumbersChanged;

        #endregion
    }
}