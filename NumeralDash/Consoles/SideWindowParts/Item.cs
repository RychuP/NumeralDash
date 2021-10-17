using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SadConsole;
using SadRogue.Primitives;
using Console = SadConsole.Console;

namespace NumeralDash.Consoles.SideWindowParts
{
    class Item
    {
        // statics
        public static Dictionary<ShortNames, string> LongNames = new()
        {
            {ShortNames.Rule, "Collection Rule"},
            {ShortNames.Next, "Next Number"},
            {ShortNames.Level, "Dungeon Level"}
        };

        public enum ShortNames
        {
            Rule,
            Next,
            Inv,
            Last,
            Timer,
            Level,
            Total,
            Remain
        }

        // settings
        const int contentBorderTop = 1;

        // properties
        public ShortNames Name;
        public string Title;
        public Point Position;          // top left corner point of the item in the SideWindow coordinates
        public int Width;

        public Item(ShortNames n)
        {
            Name = n;
            Title = LongNames.ContainsKey(n) ? LongNames[n] : n.ToString();
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
}
