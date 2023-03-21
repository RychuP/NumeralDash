﻿using NumeralDash.Entities;
using NumeralDash.Consoles.SideWindowParts;
using NumeralDash.Rules;
using NumeralDash.World;

namespace NumeralDash.Consoles;

class SideWindow : Console
{
    // settings
    const int horizontalBorder = 1,
        verticalBorder = 0;

    /// <summary>
    /// Items to be displayed in the window.
    /// </summary>
    readonly Row[] _rows = new Row[]
    {
        new Row(Item.ShortNames.Rule),
        new Row(Item.ShortNames.Timer),
        new Row(Item.ShortNames.Next, Item.ShortNames.Inv),
        new Row(Item.ShortNames.Last, Item.ShortNames.Level),
        new Row(Item.ShortNames.Total, Item.ShortNames.Remain)
    };

    public SideWindow(int sizeX, int sizeY, Dungeon dungeon) : base(sizeX, sizeY)
    {
        // coordinate for each row
        int y = verticalBorder;

        // display items
        foreach (var row in _rows)
        {
            row.Display(this, y, horizontalBorder);
            y += row.Height;
        }

        // hook events
        dungeon.LevelChanged += Dungeon_OnLevelChanged;
        dungeon.Player.InventoryChanged += Player_OnInventoryChanged;
        dungeon.Player.DepositMade += Player_OnDepositMade;
        dungeon.TimeElapsed += Dungeon_OnTimeElapsed;
        dungeon.GameOver += Dungeon_OnGameOver;
        dungeon.MapFailedToGenerate += Dungeon_OnMapFailedToGenerate;
    }

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

    #region Event Handlers

    void Player_OnInventoryChanged(Number n)
    {
        PrintItemContent(Item.ShortNames.Inv, n.ToString(), n.Color);
    }

    void OnNextNumberChanged(Number n, int remainingNumbers)
    {
        var text = (n == Number.Finished) ? "Exit" : n.ToString();
        PrintItemContent(Item.ShortNames.Next, text, n.Color);
        PrintItemContent(Item.ShortNames.Remain, remainingNumbers.ToString(), Color.White);
    }

    void OnRuleChanged(ICollectionRule r)
    {
        // display rule description
        PrintItemContent(Item.ShortNames.Rule, r.Description, r.Color);

        // hook event handlers to the new rule
        r.NextNumberChanged += OnNextNumberChanged;

        // display rule info
        OnNextNumberChanged(r.NextNumber, r.Numbers.Length);
    }

    void Player_OnDepositMade(Number n, int totalNumbers)
    {
        PrintItemContent(Item.ShortNames.Last, n.ToString(), n.Color);
        PrintItemContent(Item.ShortNames.Total, totalNumbers.ToString(), Color.White);
    }

    void Dungeon_OnLevelChanged(ICollectionRule rule, int level, string[] s)
    {
        ClearItems();
        PrintItemContent(Item.ShortNames.Level, level.ToString(), DefaultForeground);
        OnRuleChanged(rule);
    }

    void Dungeon_OnTimeElapsed(TimeSpan t)
    {
        PrintItemContent(Item.ShortNames.Timer, t.ToString(), Color.LightSkyBlue);
    }

    void Dungeon_OnGameOver(int level, TimeSpan timePlayed)
    {
        ClearItems();
    }

    void Dungeon_OnMapFailedToGenerate(AttemptCounters failedAttempts)
    {
        ClearItems();
    }

    #endregion
}
