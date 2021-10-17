using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using NumeralDash.Entities;
using NumeralDash.Rules;
using SadConsole;
using SadRogue.Primitives;
using Console = SadConsole.Console;

namespace NumeralDash.Consoles
{
    class SideWindow : Console
    {
        // settings
        const int border = 1;       // content border on all sides

        class Item
        {
            // settings
            const int contentBorderTop = 1;

            // properties
            public string Name;
            public string Title;
            public Point Position;
            public int Width;

            public Item(string name) : this(name, name) { }

            public Item(string name, string title)
            {
                Name = name;
                Title = title;
            }

            /// <summary>
            /// Displays initial look for the item.
            /// </summary>
            /// <param name="c"></param>
            public void Display(Console c)
            {
                // print title
                string title = $" {Title}: ".Align(HorizontalAlignment.Center, Width, '-');
                c.Print(Position.X, Position.Y, title);
            }

            /// <summary>
            /// Displays content for the item.
            /// </summary>
            /// <param name="c"></param>
            /// <param name="s"></param>
            /// <param name="k"></param>
            public void Display(Console c, string s, Color k)
            {
                s = s.Align(HorizontalAlignment.Center, Width);
                ColoredString cs = s.CreateColored(k);
                c.Print(Position.X, Position.Y + contentBorderTop + 1, cs);
            }

            public void Clear(Console c)
            {
                Display(c, string.Empty, c.DefaultForeground);
            }
        }

        class Row
        {
            public int Height = 4;
            List<Item> _items = new();

            public Row(Item item)
            {
                _items.Add(item);
            }

            public Row(Item i1, Item i2)
            {
                _items.Add(i1);
                _items.Add(i2);
            }

            // initial display of the items
            public void Display(Console c, int y, int border)
            {
                int plusSignsCount = _items.Count - 1;
                int plusSignsDisplayed = 0;
                int itemWidth = (c.Width - border * 2 - plusSignsCount) / _items.Count;
                int x = border;

                foreach(var item in _items)
                {
                    // display item title
                    item.Width = itemWidth;
                    item.Position = (x, y);
                    item.Display(c);

                    // display divider between items if any
                    if (plusSignsCount > 0 && plusSignsDisplayed < plusSignsCount)
                    {
                        x += itemWidth;
                        c.Print(x, y, "+");
                        c.Print(x, y + 1, "|");
                        c.Print(x, y + 2, "|");
                        c.Print(x, y + 3, "|");
                        plusSignsDisplayed++;
                    }

                    // shift x for the next item
                    x++;
                }
            }

            public void Clear(Console c)
            {
                foreach (var item in _items)
                {
                    item.Clear(c);
                }
            }

            public bool Contains(string itemName) => GetItem(itemName) is not null;

            public Item? GetItem(string name) => _items.Find(i => i.Name == name);
        }

        /// <summary>
        /// Items to be displayed in the window.
        /// </summary>
        Row[] _rows = new Row[]
        {
            new Row( new Item("Rule") ),
            new Row( new Item("Next") ),
            new Row( new Item("Inv"), new Item("Last") ),
            new Row( new Item("Timer") ),
            new Row( new Item("Level") ),
            new Row( new Item("Total"), new Item("Remain") )
        };

        public SideWindow(int sizeX, int sizeY, Dungeon dungeon) : base(sizeX, sizeY)
        {
            // coordinate for each row
            int y = border;

            // display items
            foreach (var row in _rows)
            {
                row.Display(this, y, border);
                y += row.Height;
            }

            // hook events
            dungeon.LevelChanged += OnLevelChanged;
            dungeon.Player.InventoryChanged += OnInventoryChanged;
            dungeon.Player.DepositMade += OnDepositMade;
            
        }

        public void PrintItemContent(string itemName, string s, Color c)
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
        void ClearItems()
        {
            foreach (var row in _rows)
            {
                row.Clear(this);
            }
        }

        #region Event Handlers

        void OnInventoryChanged(Number n)
        {
            PrintItemContent("Inv", n.ToString(), n.Color);
        }

        void OnNextNumberChanged(Number n, int remainingNumbers)
        {
            var text = (n == Number.Finished) ? "Proceed to exit." : n.ToString();
            PrintItemContent("Next", text, n.Color);
            PrintItemContent("Remain", remainingNumbers.ToString(), Color.White);
        }

        void OnRuleChanged(IRule r)
        {
            // display rule description
            PrintItemContent("Rule", r.Description, r.Color);

            // hook event handlers to the new rule
            r.NextNumberChanged += OnNextNumberChanged;

            // display rule info
            OnNextNumberChanged(r.NextNumber, r.Numbers.Length);
        }

        void OnDepositMade(Number n, int totalNumbers)
        {
            PrintItemContent("Last", n.ToString(), n.Color);
            PrintItemContent("Total", totalNumbers.ToString(), Color.White);
        }

        void OnLevelChanged(IRule rule, int level, string[] s)
        {
            ClearItems();
            PrintItemContent("Level", level.ToString(), DefaultForeground);
            OnRuleChanged(rule);
        }

        #endregion
    }
}
