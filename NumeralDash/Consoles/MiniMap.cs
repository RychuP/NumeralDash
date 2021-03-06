using System;
using NumeralDash.Rules;
using SadConsole;
using SadRogue.Primitives;
using NumeralDash.World;

namespace NumeralDash.Consoles
{
    class MiniMap : SadConsole.Console
    {
        // fields
        readonly ColoredGlyph _viewCell = new(Color.YellowGreen, Color.Transparent, 219);
        Point _lastLocalViewPosition = new();
        readonly Dungeon _dungeon;
        int _viewWidth, _viewHeight;

        public MiniMap(int sizeX, int sizeY, Dungeon dungeon) : base(sizeX, sizeY)
        {
            _dungeon = dungeon;
            Print(Height / 2, $"Version {Program.Version}");

            dungeon.MapFailedToGenerate += OnMapFailedToGenerate;
            dungeon.LevelChanged += OnLevelChanged;
            dungeon.PlayerMoved += OnPlayerMoved;
            dungeon.GameOver += OnGameOver;
        }

        void Display()
        {
            Surface.Clear();

            for (int x = 0; x < _viewWidth; x++)
            {
                for (int y = 0; y < _viewHeight; y++)
                {
                    this.SetCellAppearance(_lastLocalViewPosition.X + x, _lastLocalViewPosition.Y + y, _viewCell);
                }
            }
        }

        Point GetNewViewPosition()
        {
            float xRatio = (float) _dungeon.ViewPosition.X / _dungeon.Width;
            float yRatio = (float) _dungeon.ViewPosition.Y / _dungeon.Height;
            return new Point(
                Convert.ToInt32(Width * xRatio),
                Convert.ToInt32(Height * yRatio)
            );
        }

        void OnPlayerMoved()
        {
            Point newViewPos = GetNewViewPosition();
            if (_lastLocalViewPosition != newViewPos)
            {
                _lastLocalViewPosition = newViewPos;
                Display();
            }
        }

        void OnMapFailedToGenerate(AttemptCounters failedAttempts)
        {
            Surface.Clear();
            int start = (Height - 6) / 2;
            Print(start, $"Room Gen Failures: {failedAttempts.RoomGeneration}");
            Print(start + 2, $"Road Gen Attempts: {failedAttempts.RoadGeneration}");
            Print(start + 4, $"Map Gen Attempts: {failedAttempts.MapGeneration}");
        }

        void Print(int y, string text) => Surface.Print(0, y, text.Align(HorizontalAlignment.Center, Width));

        void OnLevelChanged(IRule rule, int level, string[] txt)
        {
            // calculate new view size
            float widthRatio = (float) _dungeon.View.Width / _dungeon.Width;
            float heightRatio = (float) _dungeon.View.Height / _dungeon.Height;
            _viewWidth = Convert.ToInt32(Width * widthRatio);
            _viewHeight = Convert.ToInt32(Height * heightRatio);

            // calculate new view position
            _lastLocalViewPosition = GetNewViewPosition();

            // display view on the mini map
            Display();
        }

        void OnGameOver(int level, TimeSpan timePlayed)
        {
            Surface.Clear();
        }
    }
}
