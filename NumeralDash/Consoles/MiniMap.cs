using System;
using NumeralDash.Rules;
using SadConsole;
using SadRogue.Primitives;
using NumeralDash.World;

namespace NumeralDash.Consoles
{
    class MiniMap : SadConsole.Console
    {
        // settings
        const int horizontalBorder = 1,
            verticalBorder = 1;

        // fields
        readonly ColoredGlyph _viewCell = new(Color.YellowGreen, Color.Transparent, 219);
        Point _lastLocalViewPosition = new();
        readonly Dungeon _dungeon;
        int _viewWidth, _viewHeight;

        public MiniMap(int sizeX, int sizeY, Dungeon dungeon) : base(sizeX, sizeY)
        {
            _dungeon = dungeon;
            dungeon.MapFailedToGenerate += OnMapFailedToGenerate;
            dungeon.LevelChanged += OnLevelChanged;
            dungeon.PlayerMoved += OnPlayerMoved;
        }

        void Display(string txt)
        {
            this.Clear();
            this.Print(horizontalBorder, verticalBorder, txt);
        }

        void Display(string[] txt)
        {
            this.Clear();

            int length = txt.Length < Height ? txt.Length : Height;
            for (int i = 0; i < length; i++)
            {
                this.Print(horizontalBorder, verticalBorder + i, txt[i]);
            }
        }

        void Display(Point p)
        {
            this.Clear();

            for (int x = 0; x < _viewWidth; x++)
            {
                for (int y = 0; y < _viewHeight; y++)
                {
                    this.SetCellAppearance(p.X + x, p.Y + y, _viewCell);
                }
            }
        }

        void Display()
        {
            Display(_lastLocalViewPosition);
        }

        Point GetNewViewPosition()
        {
            float xRatio = (float) _dungeon.ViewPosition.X / _dungeon.Width;
            float yRatio = (float) _dungeon.ViewPosition.Y / _dungeon.Height;
            return new Point(
                Convert.ToInt32((Width - 0) * xRatio),
                Convert.ToInt32((Height - 2) * yRatio)
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
            Display(new string[] { 
                "Map failed to generate.",
                "Please restart the game.",
                $"RoomGenFailures: {failedAttempts.RoomGeneration}",
                $"RoadGenAttempts: {failedAttempts.RoadGeneration}",
                $"MapGenAttempts: {failedAttempts.MapGeneration}"
            });
        }

        void OnLevelChanged(IRule rule, int level, string[] txt)
        {
            // calculate new view size
            float widthRatio = (float) _dungeon.View.Width / _dungeon.Width;
            float heightRatio = (float) _dungeon.View.Height / _dungeon.Height;
            _viewWidth = Convert.ToInt32((Width - 0) * widthRatio);
            _viewHeight = Convert.ToInt32((Height - 2) * heightRatio);

            // calculate new view position
            _lastLocalViewPosition = GetNewViewPosition();

            // display view on the mini map
            Display();
        }
    }
}
