using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using NumeralDash.Entities;
using NumeralDash.Rules;
using SadConsole;
using SadRogue.Primitives;

namespace NumeralDash.Consoles
{
    class SideWindow : SadConsole.Console
    {
        // settings
        const int borderLeft = 1,
            borderRight = 1,
            borderTop = 1,
            borderBottom = 0,
            contentBorderTop = 1;

        record Item(string Name, string Title)
        {
            public Point Position;
        }

        /// <summary>
        /// Items to be displayed in the window.
        /// </summary>
        Item[] _items = new Item[]
        {
            new Item("Rule", "Rule"),
            new Item("Next", "Next"),
            new Item("Last", "Last"),
            new Item("Inv", "Inventory"),
            new Item("All", "Total Collected"),
            new Item("Remain", "Total Remaining"),
        };

        public SideWindow(int sizeX, int sizeY, Dungeon dungeon) : base(sizeX, sizeY)
        {
            // display items
            int x = borderLeft, y = borderTop;
            foreach (var item in _items)
            {
                // print item title
                string title = $" {item.Title}: ".Align(HorizontalAlignment.Center, Width, '-');
                title = title[borderLeft..^borderRight];
                this.Print(x, y, title);

                // save position for the item content
                item.Position = (x, y + contentBorderTop + 1);

                // calculate position for the next item
                int sizeOfThePrevItem = contentBorderTop + 1 /* content */ + borderBottom;
                y += sizeOfThePrevItem + borderTop + 1 /* beginning of the next content */;
            }

            // show initial values
            OnInventoryChanged()
            OnNextNumberChanged(dungeon.Rule.NextNumber);
            OnRuleChanged(dungeon.Rule);

            // hook events
            dungeon.Player.InventoryChanged += OnInventoryChanged;
            dungeon.Player.DepositMade += OnDepositMade;
            dungeon.Rule.NextNumberChanged += OnNextNumberChanged;
        }

        public void PrintItemContent(string itemName, string s, Color c)
        {
            var item = Array.Find(_items, i => i.Name == itemName);
            if (item is Item i)
            {
                s = s.Align(HorizontalAlignment.Center, Width - borderLeft - borderRight);
                ColoredString cs = s.CreateColored(c);
                this.Print(i.Position.X, i.Position.Y, cs);
            }
        }

        void OnInventoryChanged(Number n)
        {
            PrintItemContent("Inv", n.ToString(), n.Color);
        }

        void OnNextNumberChanged(Number n)
        {
            PrintItemContent("Next", n.ToString(), n.Color);
        }

        void OnRuleChanged(IRule r)
        {
            PrintItemContent("Rule", r.Description, r.Color);
        }

        void OnDepositMade(Number n)
        {
            PrintItemContent("Last", n.ToString(), n.Color);
        }
    }
}
