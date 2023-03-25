using NumeralDash.Entities;
using NumeralDash.Consoles.SideWindowParts;
namespace NumeralDash.Consoles;

class SideWindow : Console
{
    #region Fields
    const int HorizontalBorder = 1;
    const int VerticalBorder = 0;

    /// <summary>
    /// Items to be displayed in the window.
    /// </summary>
    readonly Row[] _rows = new Row[]
    {
        new Row(Item.ShortNames.Rule),
        new Row(Item.ShortNames.Timer),
        new Row(Item.ShortNames.Score),
        new Row(Item.ShortNames.Next, Item.ShortNames.Inv),
        new Row(Item.ShortNames.Level, Item.ShortNames.Remain),
    };

    readonly Mask _mask;
    #endregion Fields

    #region Constructors
    public SideWindow(int width, int height, Dungeon dungeon) : base(width, height)
    {
        // coordinate for each row
        int y = VerticalBorder;

        // stats cover
        _mask = new(width, height);
        Children.Add(_mask);
        
        // display items
        foreach (var row in _rows)
        {
            row.Display(this, y, HorizontalBorder);
            y += row.Height;
        }

        dungeon.LevelChanged += Dungeon_OnLevelChanged;
        dungeon.RuleChanged += Dungeon_OnRuleChanged;
        dungeon.MapChanged += Dungeon_OnMapChanged;

        dungeon.TimeChanged += Dungeon_OnTimeChanged;
        dungeon.ScoreChanged += Dungeon_OnScoreChanged;
        dungeon.DepositMade += Dungeon_OnDepositMade;
        dungeon.GameOver += Dungeon_OnGameOver;
        dungeon.LevelCompleted += Dungeon_OnLevelCompleted;
        dungeon.Player.InventoryChanged += Player_OnInventoryChanged;
    }
    #endregion Constructors

    #region Properties

    #endregion Properties

    #region Methods
    public void PrintItemContent(Item.ShortNames itemName, string s, Color c)
    {
        var row = Array.Find(_rows, r => r.Contains(itemName));

        if (row is Row r)
        {
            var item = r.GetItem(itemName);
            if (item is Item i)
            {
                i.Display(this, s, c);
            }
        }
        else
        {
            throw new ArgumentException($"Unknown item {itemName}.");
        }
    }

    /// <summary>
    /// Erases all contents of the display items.
    /// </summary>
    public void ClearItems()
    {
        foreach (var row in _rows)
            row.Clear(this);
    }

    void Player_OnInventoryChanged(object? o, NumberEventArgs e)
    {
        string number = e.Number == Number.Empty ? string.Empty : e.Number.ToString();
        PrintItemContent(Item.ShortNames.Inv, number, e.Number.Color);
    }

    void PrintCollectionDetails(Number nextNumber, int remainingNumbers)
    {
        var text = (nextNumber == Number.Empty) ? "Exit" : nextNumber.ToString();
        PrintItemContent(Item.ShortNames.Next, text, nextNumber.Color);
        PrintItemContent(Item.ShortNames.Remain, remainingNumbers.ToString(), Color.White);
    }

    void Dungeon_OnRuleChanged(object? o, RuleEventArgs e)
    {
        PrintItemContent(Item.ShortNames.Rule, e.Rule.Title, e.Rule.Color);
        PrintCollectionDetails(e.Rule.NextNumber, e.Rule.Numbers.Count);
    }

    void Dungeon_OnScoreChanged(object? o, ScoreEventArgs e)
    {
        PrintItemContent(Item.ShortNames.Score, e.Score.ToString(), Color.Orange);
    }

    void Dungeon_OnMapChanged(object? o, MapEventArgs e)
    {
        _mask.IsVisible = false;
    }

    void Dungeon_OnDepositMade(object? o, DepositEventArgs e)
    {
        PrintCollectionDetails(e.NextNumber, e.NumbersCount);
    }

    void Dungeon_OnLevelChanged(object? o, LevelEventArgs e)
    {
        PrintItemContent(Item.ShortNames.Level, (e.Level + 1).ToString(), DefaultForeground);
    }

    void Dungeon_OnTimeChanged(object? o, TimeEventArgs e)
    {
        PrintItemContent(Item.ShortNames.Timer, e.Time.ToString(), Color.LightSkyBlue);
    }

    void Dungeon_OnGameOver(object? o, EventArgs e)
    {
        ClearItems();
        _mask.IsVisible = true;
    }

    void Dungeon_OnLevelCompleted(object? o, EventArgs e)
    {
        _mask.IsVisible = true;
    }

    void GameManager_OnGameAbandoned(object? o, EventArgs e)
    {
        ClearItems();
        _mask.IsVisible = true;
    }

    protected override void OnParentChanged(IScreenObject oldParent, IScreenObject newParent)
    {
        if (newParent is GameManager gm)
            gm.GameAbandoned += GameManager_OnGameAbandoned;
        base.OnParentChanged(oldParent, newParent);
    }
    #endregion Methods
}