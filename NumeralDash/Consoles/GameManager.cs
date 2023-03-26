using SadConsole.Input;
using NumeralDash.Consoles.SpecialScreens;

namespace NumeralDash.Consoles;

/// <summary>
/// Inititates the three main consoles and manages the game in general terms (start screen, full screen toggle, etc).
/// </summary>
class GameManager : Console
{
    #region Fields
    const int SideWindowWidth = 27;        // keep this number odd to allow dungeon view fit snugly in the dungeon window
    const int SideWindowHeight = 20;
    const bool IsDebugging = true;

    // border style around windows
    readonly ColoredGlyph _borderGlyph = new(Color.Green, Color.Black, 177);

    // main consoles
    readonly Dungeon _dungeon;
    readonly SideWindow _sideWindow;
    readonly MiniMap _miniMap;

    // other screens
    readonly StartScreen _startScreen;
    readonly GameOverScreen _gameOverScreen;
    readonly PauseScreen _pauseScreen;

    // animation
    readonly Transition _transition;
    #endregion Fields

    #region Constructors
    public GameManager(int width, int height) : base(width, height)
    {
        IsFocused = true;

        // dungeon
        Point dSize = (width - SideWindowWidth - 3, height - 2);
        Point dBorderPos = (0, 0);
        _dungeon = new(dSize.X / 2 - 1, dSize.Y);
        _dungeon.Position = (dBorderPos.X + 1, dBorderPos.Y + 1);
        _dungeon.GameOver += Dungeon_OnGameOver;
        _dungeon.LevelCompleted += Dungeon_OnLevelCompleted;
        _dungeon.MapFailedToGenerate += Dungeon_OnMapFailedToGenerate;
        AddChild(_dungeon, dSize, dBorderPos);

        // side window
        Point swSize = (SideWindowWidth, SideWindowHeight);
        Point swBorderPos = (dSize.X + 1, 0);
        _sideWindow = new(swSize.X, swSize.Y, _dungeon);
        _sideWindow.Position = (swBorderPos.X + 1, swBorderPos.Y + 1);
        AddChild(_sideWindow, swSize, swBorderPos);

        // mini map
        Point mmSize = (SideWindowWidth, height - SideWindowHeight - 3);
        Point mmBorderPos = (swBorderPos.X, SideWindowHeight + 1);
        _miniMap = new(mmSize.X, mmSize.Y, _dungeon);
        _miniMap.Position = (mmBorderPos.X + 1, mmBorderPos.Y + 1);
        AddChild(_miniMap, mmSize, mmBorderPos);

        // special screens
        int sWidth = Width - SideWindowWidth - 1;
        _startScreen = new(sWidth, Height);
        _gameOverScreen = new(sWidth, Height);
        _pauseScreen = new(sWidth, Height);
        Children.Add(_startScreen, _gameOverScreen, _pauseScreen);

        // animation
        _transition = new(_dungeon);
        Children.Add(_transition);
        
        // connect borders
        this.ConnectLines();
    }
    #endregion Constructors

    #region Methods
    void AddChild(Console c, Point consoleSize, Point borderPos)
    {
        Rectangle border = new(borderPos.X, borderPos.Y, consoleSize.X + 2, consoleSize.Y + 2);
        Surface.DrawBox(border, ShapeParameters.CreateStyledBox(ICellSurface.ConnectedLineThick, _borderGlyph));
        Children.Add(c);
    }

    public override bool ProcessKeyboard(Keyboard keyboard)
    {
        // full screen toggle regardless of what is being shown
        if (keyboard.IsKeyPressed(Keys.F5))
        {
            Game.Instance.ToggleFullScreen();
            return true;
        }

        if (IsDebugging && keyboard.IsKeyPressed(Keys.F1) && _dungeon.IsVisible)
        {
            _dungeon.Debug();
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
                        ShowDungeon(_pauseScreen, _dungeon.Resume);
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
                        OnGameAbandoned();
                }
            }
        }

        // game play handling
        else if (_dungeon.IsVisible && !_transition.IsVisible)
        {
            // pause
            if (keyboard.IsKeyPressed(Keys.Escape))
            {
                _dungeon.Pause();
                _pauseScreen.IsVisible = true;
                return true;
            }

            // player handling
            else if (_dungeon.ProcessKeyboard(keyboard))
                return true;
        }

        // no meaningful keyboard presses
        return base.ProcessKeyboard(keyboard);
    }

    void ShowStartScreen(SpecialScreen prevScreen)
    {
        prevScreen.IsVisible = false;
        _startScreen.IsVisible = true;
    }

    void ShowDungeon(SpecialScreen currentScreen, Action act)
    {
        currentScreen.IsVisible = false;
        _dungeon.IsVisible = true;
        act();
    }

    // called from the start screen
    void StartGame()
    {
        _dungeon.PrepareStartup();
        _transition.Finished += Transition_GameStart_OnFinished;
        _startScreen.Stars.IsVisible = false;
        _transition.Play(_startScreen, _dungeon, Color.LightBlue, () => _dungeon.ChangeMap());
    }

    // called from the game over screen
    void RetryGame()
    {
        _dungeon.PrepareStartup();
        _transition.Finished += Transition_GameStart_OnFinished;
        _gameOverScreen.Stars.IsVisible = false;
        _transition.Play(_gameOverScreen, _dungeon, Color.LightBlue, () => _dungeon.ChangeMap());
    }

    void Dungeon_OnGameOver(object? o, GameOverEventArgs e)
    {
        _dungeon.IsVisible = false;
        _gameOverScreen.DisplayStats(e.Level, e.Score, e.TimeTotal);
        _miniMap.ShowProgramVersion();
    }

    // when player presses escape while the pause screen is being shown
    void OnGameAbandoned()
    {
        ShowStartScreen(_pauseScreen);
        GameAbandoned?.Invoke(this, EventArgs.Empty);
    }

    void Dungeon_OnMapFailedToGenerate(object? o, EventArgs e)
    {
        _dungeon.Retry();
    }

    void Dungeon_OnLevelCompleted(object? o, EventArgs e)
    {
        _transition.Finished += Transition_LevelCompleted_OnFinished;
        _transition.Play(_dungeon, _dungeon, Color.Pink, () => _dungeon.ChangeMap());
    }

    void Transition_LevelCompleted_OnFinished(object? o, EventArgs e)
    {
        _dungeon.FinishStartup();
        _transition.Finished -= Transition_LevelCompleted_OnFinished;
    }

    void Transition_GameStart_OnFinished(object? o, EventArgs? e)
    {
        _dungeon.FinishStartup();
        _transition.Finished -= Transition_GameStart_OnFinished;
    }
    #endregion Methods

    #region Events
    public event EventHandler? GameAbandoned;
    #endregion Events
}