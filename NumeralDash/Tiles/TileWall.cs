using System;
using SadConsole;
using SadRogue.Primitives;

namespace NumeralDash.Tiles
{
    class TileWall : TileBase
    {
        public TileWall(bool blocksMovement = true, bool blocksLOS = true) : 
            base(Color.LightGray, Color.Transparent, 219, blocksMovement, blocksLOS)
        {
            Name = "Wall";
        }
    }
}