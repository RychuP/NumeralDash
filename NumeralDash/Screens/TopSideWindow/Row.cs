﻿using System.Collections.Generic;

namespace NumeralDash.Screens.TopSideWindow;

class Row
{
    public int Height = 4;
    readonly List<Item> _items = new();

    public Row(params Item.ShortNames[] itemNames)
    {
        foreach (var n in itemNames)
            _items.Add(new Item(n));
    }

    // initial display of the items
    public void Display(ScreenSurface c, int y, int border)
    {
        int plusSignsCount = _items.Count - 1;
        int plusSignsDisplayed = 0;
        int itemWidth = (c.Surface.Width - border * 2 - plusSignsCount) / _items.Count;
        int x = border;

        foreach (var item in _items)
        {
            // display item title
            item.Width = itemWidth;
            item.Position = (x, y);
            item.Display(c);

            // display divider between items if any
            if (plusSignsCount > 0 && plusSignsDisplayed < plusSignsCount)
            {
                x += itemWidth;
                c.Surface.Print(x, y, "+"); // (char)197);
                c.Surface.Print(x, y + 1, (char)179); //  "|");
                c.Surface.Print(x, y + 2, (char)179); //  "|");
                c.Surface.Print(x, y + 3, (char)179);  // "|");
                plusSignsDisplayed++;
            }

            // shift x for the next item
            x++;
        }
    }

    public void Clear(ScreenSurface c)
    {
        foreach (var item in _items)
            item.Clear(c);
    }

    public bool Contains(Item.ShortNames n) => 
        GetItem(n) is not null;

    public Item? GetItem(Item.ShortNames n) => 
        _items.Find(i => i.Name == n);
}