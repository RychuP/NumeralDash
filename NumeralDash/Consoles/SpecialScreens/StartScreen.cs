using SadConsole.Readers;

namespace NumeralDash.Consoles.SpecialScreens
{
    class StartScreen : SpecialScreen
    {
        public StartScreen(int width, int height, TheDrawFont drawFont) : base(width, height, "numeral", "dash", drawFont)
        {
            Surface.PrintCenter(TextRow, "Collect all numbers scattered around the dungeon in the given order");
            Surface.PrintCenter(TextRow + 2, "and leave before the time runs out.");
            Surface.PrintCenter(TextRow + 5, "Arrow keys to move (fast with left shift), F5 toggle full screen.");
            Surface.PrintCenter(TextRow + 8, "Press Enter to start...");
            IsVisible = true;
        }
    }
}