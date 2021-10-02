using System;
using SadConsole;
using SadRogue.Primitives;
using SadConsole.Entities;

namespace NumeralDash.Entities
{
    class Player : Entity
    {
        public Player(Point startPosition) : base(Color.Yellow, Color.Black, glyph: 1, zIndex : 1)
        {
            Position = startPosition;
        }

        public void MoveTo(Point newPosition)
        {
            Position = newPosition;
        }

        public Point GetNextMove(Point direction)
        {
            return Position.Translate(direction);
        }
    }
}