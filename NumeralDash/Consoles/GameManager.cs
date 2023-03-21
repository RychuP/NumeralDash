using System.Linq;
using SadConsole.Input;
using NumeralDash.World;
using NumeralDash.Consoles.SpecialScreens;

namespace NumeralDash.Consoles;

/// <summary>
/// Inititates the three main consoles and manages the game in general terms (start screen, full screen toggle, etc).
/// </summary>
class GameManager : Console
{
    const int SideWindowWidth = 27,        // keep this number odd to allow dungeon view fit snugly in the dungeon window
              SideWindowHeight = 20;

    // border style around windows
    readonly ColoredGlyph _borderGlyph = new(Color.Green, Color.Black, 177);

    // main consoles
    readonly Dungeon _dungeon;
    readonly SideWindow _sideWindow;
    readonly MiniMap _miniMap;

    // other screens
    readonly StartScreen _startScreen;
    readonly GameOverScreen _gameOverScreen;
    readonly ErrorScreen _errorScreen;
    readonly PauseScreen _pauseScreen;

    public GameManager(int width, int height) : base(width, height)
    {
        IsFocused = true;

        // dungeon
        Point dSize = (width - SideWindowWidth - 3, height - 2);
        Point dBorderPos = (0, 0);
        _dungeon = new(dSize.X / 2 - 2, dSize.Y) {
            Position = (dBorderPos.X + 1, dBorderPos.Y + 1),
        };
        _dungeon.GameOver += OnGameOver;
        _dungeon.MapFailedToGenerate += OnMapFailedToGenerate;
        AddChild(_dungeon, dSize, dBorderPos);

        // side window
        Point swSize = (SideWindowWidth, SideWindowHeight);
        Point swBorderPos = (dSize.X + 1, 0);
        _sideWindow = new(swSize.X, swSize.Y, _dungeon) { 
            Position = (swBorderPos.X + 1, swBorderPos.Y + 1) 
        };
        AddChild(_sideWindow, swSize, swBorderPos);

        // mini map
        Point mmSize = (SideWindowWidth, height - SideWindowHeight - 3);
        Point mmBorderPos = (swBorderPos.X, SideWindowHeight + 1);
        _miniMap = new(mmSize.X, mmSize.Y, _dungeon) {
            Position = (mmBorderPos.X + 1, mmBorderPos.Y + 1)
        };
        AddChild(_miniMap, mmSize, mmBorderPos);

        // special screens
        int sWidth = Width - SideWindowWidth - 1;
        _startScreen = new(sWidth, Height);
        _gameOverScreen = new(sWidth, Height);
        _errorScreen = new(sWidth, Height);
        _pauseScreen = new(sWidth, Height);
        Children.Add(_startScreen, _gameOverScreen, _errorScreen, _pauseScreen);
        
        // connect borders
        this.ConnectLines();
    }

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

        // keyboard handling when special screens are being shown
        else if (_startScreen.IsVisible || _gameOverScreen.IsVisible || _errorScreen.IsVisible || _pauseScreen.IsVisible)
        {
            if (keyboard.HasKeysPressed)
            {
                if (keyboard.IsKeyPressed(Keys.Enter))
                {
                    if (_startScreen.IsVisible)
                        ShowDungeon(_startScreen, _dungeon.Start);

                    else if (_gameOverScreen.IsVisible)
                        ShowDungeon(_gameOverScreen, _dungeon.Restart);

                    else if (_errorScreen.IsVisible)
                        ShowDungeon(_errorScreen, _dungeon.Retry);

                    else if (_pauseScreen.IsVisible)
                        ShowDungeon(_pauseScreen, _dungeon.Resume);
                }

                else if (keyboard.IsKeyPressed(Keys.Escape))
                {
                    if (_startScreen.IsVisible)
                        Environment.Exit(0);

                    else if (_gameOverScreen.IsVisible)
                        ShowStartScreen(_gameOverScreen);

                    else if (_errorScreen.IsVisible)
                        ShowStartScreen(_errorScreen);

                    else if (_pauseScreen.IsVisible)
                        OnGameAbandoned();
                }
            }
        }

        // game play handling
        else if (_dungeon.IsVisible)
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

    void ShowStartScreen(SpecialScreen s)
    {
        s.IsVisible = false;
        _startScreen.IsVisible = true;
    }

    void ShowDungeon(SpecialScreen s, Action act)
    {
        s.IsVisible = false;
        _dungeon.IsVisible = true;
        act();
    }

    void OnGameOver(int level, TimeSpan timePlayed)
    {
        _gameOverScreen.DisplayStats(level, timePlayed);
        _miniMap.ShowProgramVersion();
    }

    void OnGameAbandoned()
    {
        ShowStartScreen(_pauseScreen);
        _sideWindow.ClearItems();
        _miniMap.ShowProgramVersion();
    }

    void OnMapFailedToGenerate(AttemptCounters failedAttempts)
    {
        _errorScreen.IsVisible = true;
    }
}