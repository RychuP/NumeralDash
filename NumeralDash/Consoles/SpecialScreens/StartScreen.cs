namespace NumeralDash.Consoles.SpecialScreens
{
    class StartScreen : SpecialScreen
    {
        public StartScreen(int width, int height) : base(width, height, "numeral", "dash")
        {
            Print(0, "Collect all numbers scattered around the dungeon in the given order");
            Print(2, "and leave before the time runs out.");
            Print(5, "Arrow keys to move (fast with left shift), F5 toggle full screen.");
            Print(8, "Press Enter to start...");
            IsVisible = true;
        }
    }
}