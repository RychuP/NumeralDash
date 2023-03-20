using SadConsole.Entities;

namespace NumeralDash.Entities;

class NumberExtension : Entity
{
    public Number Number { get; init; }

    public NumberExtension(int charCode, Color c, Number parent) : base(c, Color.Black, charCode, (int)Layer.Items)
    {
        Appearance.Background = Appearance.Background.FillAlpha();
        Number = parent;
    }
}
