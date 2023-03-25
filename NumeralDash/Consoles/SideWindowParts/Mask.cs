namespace NumeralDash.Consoles.SideWindowParts;

class Mask : ScreenSurface
{
    public Mask(int width, int height) : base(width, height)
    {
        Surface.DefaultBackground = Color.Black;
        Surface.Clear();
        Surface.Print(4, "Numeral");
        Surface.Print(7, "Dash");
    }
}