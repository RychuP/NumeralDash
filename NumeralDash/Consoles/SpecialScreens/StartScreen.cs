﻿namespace NumeralDash.Consoles.SpecialScreens
{
    class StartScreen : SpecialScreen
    {
        public StartScreen(int width, int height) : base(width, height, "numeral", "dash")
        {
            Print(0, "Collect all numbers scattered around the dungeon in the given order");
            Print(2, "and leave before the time runs out.");
            Print(5, $"{Orange("Controls")}: {Green("Arrow Keys")} move, " +
                $"{Green("F5")} toggle full screen, {Esc} pause.");
            Print(7, $"{Orange("Auto Move")}: {Green("Left Shift")} as far as possible, " +
                $"{Green("Ctrl")} until the next intersection.");
            Print(10, $"Press {Enter} to start.");
            IsVisible = true;
        }
    }
}