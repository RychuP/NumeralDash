using System;
using NumeralDash.Rules;
using SadConsole;
using SadRogue.Primitives;
using NumeralDash.World;

namespace NumeralDash.Consoles
{
    class BottomWindow : SadConsole.Console
    {
        public BottomWindow(int sizeX, int sizeY, Dungeon dungeon) : base(sizeX, sizeY)
        {
            dungeon.MapFailedToGenerate += OnMapFailedToGenerate;
            dungeon.LevelChanged += OnLevelChanged;
            dungeon.PlayerMoved += OnPlayerMoved;
        }

        public void Display(string txt)
        {
            this.Clear();
            this.Print(2, 0, txt);
        }

        public void Display(string[] txt)
        {
            this.Clear();

            int length = txt.Length < Height ? txt.Length : Height;
            for (int i = 0; i < length; i++)
            {
                this.Print(2, i, txt[i]);
            }
        }

        void OnPlayerMoved(string[] txt)
        {
            Display(txt);
        }

        void OnMapFailedToGenerate(AttemptCounters failedAttempts)
        {
            Display(new string[] { 
                "Map failed to generate. Please restart the game.",
                $"RoomGenerationAttempts: {failedAttempts.RoomGeneration}, " +
                    $"RoadGenerationAttempts: {failedAttempts.RoadGeneration}, " +
                    $"MapGenerationAttempts: {failedAttempts.MapGeneration}"
            });
        }

        void OnLevelChanged(IRule rule, int level, string[] txt)
        {
            Display(txt);
        }
    }
}
