namespace NumeralDash.Tiles;

class Wall : Tile
{
    public Wall(bool blocksMovement = true, bool blocksLOS = true) : 
        base(Color.LightGray, Color.Transparent, 219, blocksMovement, blocksLOS)
    {
        Name = "Wall";
    }
}