namespace NumeralDash;

record Size(int Width, int Height)
{
    public static readonly Size Empty = new(0, 0);
}