using NumeralDash;
using NumeralDash.Screens.TopSideWindow;

namespace NumeralDash.Screens.BottomSideWindow;

class MiniMap : ScreenSurface
{
    #region Fields
    readonly ColoredGlyph Appearance = new(Color.YellowGreen, Color.Transparent, 219);
    Rectangle MiniView = Rectangle.Empty;
    Size MapSize = Size.Empty;
    readonly ColoredString _line;
    #endregion Fields

    #region Constructors
    public MiniMap() : base(StatsDisplay.Width, Program.Height - StatsDisplay.Height - 3)
    {
        Position = (Program.Width - Surface.Width - 1, Program.Height - Surface.Height - 1);
        _line = new ColoredString(new string((char)196, Surface.Width - 2), Color.Green, Color.Transparent);
    }
    #endregion Constructors

    #region Properties

    #endregion Properties

    #region Methods
    void DisplayMiniView()
    {
        Surface.Clear();
        Surface.Fill(MiniView, Appearance.Foreground, glyph: Appearance.Glyph);
    }

    void PrintVersion()
    {
        Surface.Clear();
        Surface.Print(3, _line);
        Print(5, $"Version {Program.Version}");
        Surface.Print(7, _line);
        Print(8, "Made with");
        Print(10, "SadConsole 9.2.2");
    }

    void ChangeMiniViewSize(Size mapViewSize, Size mapAreaSize)
    {
        // calculate mini view size
        float widthRatio = (float)mapViewSize.Width / mapAreaSize.Width;
        float heightRatio = (float)mapViewSize.Height / mapAreaSize.Height;
        int width = Convert.ToInt32(Surface.Width * widthRatio);
        int height = Convert.ToInt32(Surface.Height * heightRatio);
        MiniView = MiniView.WithSize(width, height);
    }

    void ChangeMiniViewPosition(Point mapViewPosition, Size mapAreaSize)
    {
        float xRatio = (float)mapViewPosition.X / mapAreaSize.Width;
        float yRatio = (float)mapViewPosition.Y / mapAreaSize.Height;
        int x = Convert.ToInt32(Surface.Width * xRatio);
        int y = Convert.ToInt32(Surface.Height * yRatio);
        MiniView = MiniView.WithPosition(new Point(x, y));
    }

    void Print(int y, string text) =>
        Surface.Print(0, y, text.Align(HorizontalAlignment.Center, Surface.Width));

    void Dungeon_OnMapFailedToGenerate(MapGenEventArgs e)
    {
        Surface.Clear();
        int start = (Surface.Height - 6) / 2;
        Print(start, $"Room Gen Failures: {e.RoomGenAttempts}");
        Print(start + 2, $"Road Gen Attempts: {e.RoadGenAttempts}");
        Print(start + 4, $"Map Gen Attempts: {e.MapGenAttempts}");
    }

    void Dungeon_OnLevelCompleted(object? o, EventArgs e)
    {
        MiniView = Rectangle.Empty;
        MapSize = Size.Empty;
    }

    void Dungeon_OnMapChanged(object? o, MapEventArgs e)
    {
        MapSize = e.MapSize;
        ChangeMiniViewSize(e.View.Size.ToSize(), e.MapSize);
        ChangeMiniViewPosition(e.View.Position, MapSize);
        DisplayMiniView();
    }

    void Dungeon_OnViewPositionChanged(object? o, PositionEventArgs e)
    {
        if (MapSize == Size.Empty) return;
        ChangeMiniViewPosition(e.Position, MapSize);
        DisplayMiniView();
    }

    void Transition_OnStarted(object? o, TransitionEventArgs e)
    {
        switch (e.Type)
        {
            case TransitionTypes.GameStart:

                break;

            case TransitionTypes.LevelChange:

                break;

            case TransitionTypes.GameOver:

                break;
        }
    }

    void Transition_OnReachedMidPoint(object? o, TransitionEventArgs e)
    {
        switch (e.Type)
        {
            case TransitionTypes.GameStart:

                break;

            case TransitionTypes.LevelChange:

                break;

            case TransitionTypes.GameOver:

                break;
        }
    }

    void Transition_OnFinished(object? o, TransitionEventArgs e)
    {
        switch (e.Type)
        {
            case TransitionTypes.GameStart:

                break;

            case TransitionTypes.LevelChange:

                break;

            case TransitionTypes.GameOver:

                break;
        }
    }

    void AnsiMask_VisibleChanged(object? o, EventArgs e)
    {
        
        if (o is AnsiMask mask)
        {
            if (mask.IsVisible)
            {
                PrintVersion();
                Print(0, $"\"{mask.Description}\"");
                Print(2, "by Whazzit / Blocktronics");
            }
        }
    }

    void LevelMask_VisibleChanged(object? o, EventArgs e)
    {
        if (o is LevelMask mask)
        {
            if (mask.IsVisible)
            {
                PrintVersion();
                Print(1, "Level is loading...");
            }
        }
    }

    protected override void OnParentChanged(IScreenObject oldParent, IScreenObject newParent)
    {
        if (newParent is GameManager gm)
        {
            gm.Dungeon.LevelCompleted += Dungeon_OnLevelCompleted;
            gm.Dungeon.MapChanged += Dungeon_OnMapChanged;
            gm.Dungeon.ViewPositionChanged += Dungeon_OnViewPositionChanged;
            gm.AnsiMask.VisibleChanged += AnsiMask_VisibleChanged;
            gm.LevelMask.VisibleChanged += LevelMask_VisibleChanged;
        }
        base.OnParentChanged(oldParent, newParent);
    }
    #endregion Methods
}