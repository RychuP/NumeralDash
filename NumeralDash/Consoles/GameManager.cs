using System.Linq;
using SadConsole.Input;
using NumeralDash.World;
using NumeralDash.Consoles.SpecialScreens;
using SadConsole.Readers;

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
    readonly TheDrawFont _drawFont;

    public GameManager(int width, int height) : base(width, height)
    {
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

        // draw font
        string fontFileName = "DESTRUCX.TDF";
        var fontEnumerable = TheDrawFont.ReadFonts(@"Fonts/" + fontFileName);
        if (fontEnumerable is null) throw new FontLoadingException(fontFileName);
        _drawFont = fontEnumerable.ToArray()[3];

        // special screens
        _startScreen = new(Width - SideWindowWidth - 1, Height, _drawFont);
        _gameOverScreen = new(Width - SideWindowWidth - 1, Height, _drawFont);
        _errorScreen = new(Width - SideWindowWidth - 1, Height, _drawFont);
        Children.Add(_startScreen);
        Children.Add(_gameOverScreen);
        Children.Add(_errorScreen);
        
        // connect borders
        this.ConnectLines();

        // replace starting console
        Game.Instance.Screen = this;
        Game.Instance.DestroyDefaultStartingConsole();
        IsFocused = true;
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
        if (keyboard.HasKeysPressed && keyboard.IsKeyPressed(Keys.F5))
        {
            Game.Instance.ToggleFullScreen();
            return true;
        }

        // keyboard handling when special screens are being shown
        else if (_startScreen.IsVisible || _gameOverScreen.IsVisible || _errorScreen.IsVisible)
        {
            if (keyboard.HasKeysPressed && keyboard.IsKeyPressed(Keys.Enter))
            {
                if (_startScreen.IsVisible) 
                    ShowDungeon(_startScreen, _dungeon.Start);

                else if (_gameOverScreen.IsVisible) 
                    ShowDungeon(_gameOverScreen, _dungeon.Restart);

                else if (_errorScreen.IsVisible)
                    ShowDungeon(_errorScreen, _dungeon.Retry);
            }
        }

        // everything that happens during normal gameplay
        else if (keyboard.HasKeysDown || keyboard.HasKeysPressed)
        {
            _dungeon.ProcessKeyboard(keyboard);
        }

        // keyboard has been handled
        return true;

        void ShowDungeon(SpecialScreen s, Action act)
        {
            s.IsVisible = false;
            _dungeon.IsVisible = true;
            act();
        }
    }

    void OnGameOver(int level, TimeSpan timePlayed)
    {
        _gameOverScreen.DisplayStats(level, timePlayed);
    }

    void OnMapFailedToGenerate(AttemptCounters failedAttempts)
    {
        _errorScreen.IsVisible = true;
    }
}

public class FontLoadingException : Exception
{
    public FontLoadingException(string message) : base(message) { }
}