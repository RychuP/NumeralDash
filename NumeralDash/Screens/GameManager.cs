using SadConsole.Input;
using NumeralDash.Screens.StaticScreens;
using NumeralDash.Screens.TopSideWindow;
using NumeralDash.Screens.BottomSideWindow;

namespace NumeralDash.Screens;

/// <summary>
/// Inititates the three main consoles and manages the game in general terms (start screen, full screen toggle, etc).
/// </summary>
class GameManager : Console
{
    #region Fields
    const bool IsDebugging = false;

    // border style around windows
    readonly ColoredGlyph _borderGlyph = new(Color.Green, Color.Black, 177);

    // other screens
    readonly StartScreen _startScreen = new();
    readonly GameOverScreen _gameOverScreen = new();
    readonly PauseScreen _pauseScreen = new();
    #endregion Fields

    #region Constructors
    public GameManager() : base(Program.Width, Program.Height)
    {
        IsFocused = true;

        // dungeon
        Dungeon.GameOver += Dungeon_OnGameOver;
        Dungeon.LevelCompleted += Dungeon_OnLevelCompleted;
        DrawBorder(new Rectangle(0, 0, Program.Width - StatsDisplay.Width - 1, Program.Height));
        Children.Add(Dungeon);

        // stats and mini map
        AddChild(StatsDisplay);
        AddChild(MiniMap);
        Children.Add(_startScreen, _gameOverScreen, _pauseScreen);

        // remaining children
        Children.Add(Transition, AnsiMask, LevelMask);

        // trigger minimap displaying correct info about ansi
        AnsiMask.IsVisible = true;

        // connect borders
        this.ConnectLines();
    }
    #endregion Constructors

    #region Properties
    public StatsDisplay StatsDisplay { get; } = new();
    public Dungeon Dungeon { get; init; } = new();
    public Transition Transition { get; init; } = new();
    public MiniMap MiniMap { get; } = new();
    public AnsiMask AnsiMask { get; } = new();
    public LevelMask LevelMask { get; } = new();
    #endregion Properties

    #region Methods
    void AddChild(ScreenSurface child)
    {
        var border = child.Surface.Area.Expand(1, 1);
        DrawBorder(border.WithPosition(child.Position - (1, 1)));
        Children.Add(child);
    }

    void DrawBorder(Rectangle rectangle) =>
        Surface.DrawBox(rectangle, ShapeParameters.CreateStyledBox(ICellSurface.ConnectedLineThick, _borderGlyph));

    public override bool ProcessKeyboard(Keyboard keyboard)
    {
        // full screen toggle regardless of what is being shown
        if (keyboard.IsKeyPressed(Keys.F5))
        {
            Game.Instance.ToggleFullScreen();
            return true;
        }

        if (IsDebugging && keyboard.IsKeyPressed(Keys.F1) && Dungeon.IsVisible)
        {
            Dungeon.Debug();
        }

        // keyboard handling when special screens are being shown
        else if (_startScreen.IsVisible || _gameOverScreen.IsVisible || _pauseScreen.IsVisible)
        {
            if (keyboard.HasKeysPressed)
            {
                if (keyboard.IsKeyPressed(Keys.Enter))
                {
                    if (_startScreen.IsVisible)
                        StartGame();

                    else if (_gameOverScreen.IsVisible)
                        RetryGame();

                    else if (_pauseScreen.IsVisible)
                        ResumeGame();
                }

                else if (keyboard.IsKeyPressed(Keys.Space))
                {
                    if (_startScreen.IsVisible)
                        _startScreen.ToggleText();
                }

                else if (keyboard.IsKeyPressed(Keys.Escape))
                {
                    if (_startScreen.IsVisible)
                        Environment.Exit(0);

                    else if (_gameOverScreen.IsVisible)
                        ShowStartScreen(_gameOverScreen);

                    else if (_pauseScreen.IsVisible)
                        ShowStartScreen(_pauseScreen);
                }
            }
        }

        // game play handling
        else if (Dungeon.IsVisible && !Transition.IsVisible)
        {
            // pause
            if (keyboard.IsKeyPressed(Keys.Escape))
            {
                Dungeon.Pause();
                _pauseScreen.IsVisible = true;
                return true;
            }

            // player handling
            else if (Dungeon.ProcessKeyboard(keyboard))
                return true;
        }

        // no meaningful keyboard presses
        return base.ProcessKeyboard(keyboard);
    }

    void ShowStartScreen(StaticScreen prevScreen)
    {
        prevScreen.IsVisible = false;
        _startScreen.IsVisible = true;
        OnStartScreenShown();
    }

    void ResumeGame()
    {
        _pauseScreen.IsVisible = false;
        Dungeon.Resume();
    }

    // called from the start screen
    void StartGame() =>
        Transition.Play(TransitionTypes.GameStart);

    // called from the game over screen
    void RetryGame() =>
        Transition.Play(TransitionTypes.GameStart);

    void Dungeon_OnGameOver(object? o, GameOverEventArgs e)
    {
        _gameOverScreen.DisplayStats(e.Level, e.Score, e.TimeTotal);
    }

    void Dungeon_OnLevelCompleted(object? o, EventArgs e) =>
        Transition.Play(TransitionTypes.LevelChange);

    void OnStartScreenShown()
    {
        StartScreenShown?.Invoke(this, EventArgs.Empty);
    }
    #endregion Methods

    #region Events
    public event EventHandler? StartScreenShown;
    #endregion Events
}