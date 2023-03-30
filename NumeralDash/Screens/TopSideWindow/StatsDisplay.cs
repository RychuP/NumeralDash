using NumeralDash.Entities;

namespace NumeralDash.Screens.TopSideWindow;

class StatsDisplay : ScreenSurface
{
    #region Fields
    public const int Width = 27; 
    public const int Height = 20;
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

    
    #endregion Fields

    #region Constructors
    public StatsDisplay() : base(Width, Height)
    {
        Position = (Program.Width - Width - 1, 1);

        // coordinate for each row
        int y = VerticalBorder;

        // display items
        foreach (var row in _rows)
        {
            row.Display(this, y, HorizontalBorder);
            y += row.Height;
        }
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
        var text = nextNumber == Number.Empty ? "Exit" : nextNumber.ToString();
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

    void Dungeon_OnDepositMade(object? o, DepositEventArgs e)
    {
        PrintCollectionDetails(e.NextNumber, e.NumbersCount);
    }

    void Dungeon_OnLevelChanged(object? o, LevelEventArgs e)
    {
        PrintItemContent(Item.ShortNames.Level, (e.Level + 1).ToString(), Surface.DefaultForeground);
    }

    void Dungeon_OnTimeChanged(object? o, TimeEventArgs e)
    {
        PrintItemContent(Item.ShortNames.Timer, e.Time.ToString(), Color.LightSkyBlue);
    }

    protected override void OnParentChanged(IScreenObject oldParent, IScreenObject newParent)
    {
        if (newParent is GameManager gm)
        {
            gm.StartScreenShown += (o, e) => ClearItems();
            gm.Dungeon.LevelChanged += Dungeon_OnLevelChanged;
            gm.Dungeon.RuleChanged += Dungeon_OnRuleChanged;
            gm.Dungeon.TimeChanged += Dungeon_OnTimeChanged;
            gm.Dungeon.ScoreChanged += Dungeon_OnScoreChanged;
            gm.Dungeon.DepositMade += Dungeon_OnDepositMade;
            gm.Dungeon.GameOver += (o, e) => ClearItems();
            gm.Dungeon.Player.InventoryChanged += Player_OnInventoryChanged;
        }
        base.OnParentChanged(oldParent, newParent);
    }
    #endregion Methods
}