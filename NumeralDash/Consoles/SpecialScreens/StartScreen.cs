﻿namespace NumeralDash.Consoles.SpecialScreens
{
    class StartScreen : SpecialScreen
    {
        public StartScreen(int width, int height) : base(width, height, "numeral", "dash")
        {
            Print(0, "Collect all numbers scattered around the map in the given order");
            Print(2, "and leave before the time runs out.");
            Print(4, $"{Orange("Controls")}: {Green("Arrow Keys")} move, " +
                $"{Green("F5")} toggle full screen, {Esc} pause or exit,");
            Print(6, $"{Green("Left Shift")} auto move stopping at numbers and the exit,");
            Print(8, $"{Green("Left Ctrl")} auto move ignoring numbers and stopping at road intersections.");
            Print(10, $"Press {Enter} to start.");
            IsVisible = true;
        }
    }
}