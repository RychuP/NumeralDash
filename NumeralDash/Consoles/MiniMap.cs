namespace NumeralDash.Consoles;

class MiniMap : Console
{
    #region Fields
    readonly ColoredGlyph Appearance = new(Color.YellowGreen, Color.Transparent, 219);
    Rectangle MiniView = Rectangle.Empty;
    Size MapSize = Size.Empty;
    #endregion Fields

    #region Constructors
    public MiniMap(int width, int height, Dungeon dungeon) : base(width, height)
    {
        ShowProgramVersion();

        //dungeon.MapFailedToGenerate += Dungeon_OnMapFailedToGenerate;
        dungeon.LevelCompleted += Dungeon_OnLevelCompleted;
        dungeon.MapChanged += Dungeon_OnMapChanged;
        dungeon.ViewPositionChanged += Dungeon_OnViewPositionChanged;
        dungeon.GameOver += Dungeon_OnGameOver;
    }
    #endregion Constructors

    #region Methods
    void DisplayMiniView()
    {
        Surface.Clear();
        Surface.Fill(MiniView, Appearance.Foreground, glyph: Appearance.Glyph);
    }

    public void ShowProgramVersion()
    {
        Surface.Clear();
        Print(2, $"Version {Program.Version}");
        Print(6, "Made with");
        Print(8, "SadConsole 9.2.2");
    }

    void ChangeMiniViewSize(Size mapViewSize, Size mapAreaSize)
    {
        // calculate mini view size
        float widthRatio = (float)mapViewSize.Width / mapAreaSize.Width;
        float heightRatio = (float)mapViewSize.Height / mapAreaSize.Height;
        int width = Convert.ToInt32(Width * widthRatio);
        int height = Convert.ToInt32(Height * heightRatio);
        MiniView = MiniView.WithSize(width, height);
    }

    void ChangeMiniViewPosition(Point mapViewPosition, Size mapAreaSize)
    {
        float xRatio = (float)mapViewPosition.X / mapAreaSize.Width;
        float yRatio = (float)mapViewPosition.Y / mapAreaSize.Height;
        int x = Convert.ToInt32(Width * xRatio);
        int y = Convert.ToInt32(Height * yRatio);
        MiniView = MiniView.WithPosition(new Point(x, y));
    }

    void Print(int y, string text) =>
        Surface.Print(0, y, text.Align(HorizontalAlignment.Center, Width));

    void Dungeon_OnMapFailedToGenerate(MapGenEventArgs e)
    {
        Surface.Clear();
        int start = (Height - 6) / 2;
        Print(start, $"Room Gen Failures: {e.RoomGenAttempts}");
        Print(start + 2, $"Road Gen Attempts: {e.RoadGenAttempts}");
        Print(start + 4, $"Map Gen Attempts: {e.MapGenAttempts}");
    }

    void Dungeon_OnLevelCompleted(object? o, EventArgs e)
    {
        Surface.Clear();
        MiniView = Rectangle.Empty;
        MapSize = Size.Empty;
    }

    void Dungeon_OnMapChanged(object? o, MapEventArgs e)
    {
        MapSize = e.MapSize;
        ChangeMiniViewSize(e.View.Size.ToSize(), e.MapSize);
        ChangeMiniViewPosition(e.View.Position, MapSize);
        DisplayMiniView();
        //Surface.Print(0, 0, e.View.Position.ToString());
    }

    void Dungeon_OnViewPositionChanged(object? o, PositionEventArgs e)
    {
        if (MapSize == Size.Empty) return;
        ChangeMiniViewPosition(e.Position, MapSize);
        DisplayMiniView();
        //Surface.Print(0, 1, e.Position.ToString());
    }

    void Dungeon_OnGameOver(object? o, EventArgs e)
    {
        ShowProgramVersion();
    }

    void GameManager_OnGameAbandoned(object? o, EventArgs e)
    {
        ShowProgramVersion();
    }

    protected override void OnParentChanged(IScreenObject oldParent, IScreenObject newParent)
    {
        if (newParent is GameManager gm)
            gm.GameAbandoned += GameManager_OnGameAbandoned;
        base.OnParentChanged(oldParent, newParent);
    }
    #endregion Methods
}