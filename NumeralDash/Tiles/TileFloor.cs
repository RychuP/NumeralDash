using System;
using SadConsole;
using SadRogue.Primitives;
using NumeralDash.World;

namespace NumeralDash.Tiles
{
    class TileFloor : TileBase
    {
        public IConnectable Parent { get; init; }

        public TileFloor(IConnectable parent, bool blocksMovement = false, bool blocksLOS = false) : 
            base(Color.DarkGray, Color.Transparent, 250, blocksMovement, blocksLOS)
        {
            Name = "Floor";
            Parent = parent;
        }

        public override string GetInfo() => $"{Name} Parent: {Parent}, {Parent.GetInfo()}";

        public string[] GetExtendedInfo(Point p)
        {
            return new string[]
            {
                $"Position: {p}, {GetInfo()}"
            };
        }
    }
}