using System;
using SadConsole;
using SadRogue.Primitives;

namespace NumeralDash.Tiles
{
    public abstract class TileBase : ColoredGlyph
    {
        // Movement and Line of Sight Flags
        public bool IsBlockingMove;
        public bool IsBlockingLOS;

        // Tile's name
        protected string Name;

        public TileBase(Color foreground, Color background, int glyph, bool blockingMove=false, bool blockingLOS = false, string name = "") : 
            base(foreground, background, glyph)
        {
            IsBlockingMove = blockingMove;
            IsBlockingLOS = blockingLOS;
            Name = name;
        }

        public virtual string GetInfo() => $"This tile is a {Name}";
    }
}