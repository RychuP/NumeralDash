using System;
using SadConsole;
using SadRogue.Primitives;

namespace NumeralDash.Consoles
{
    class Status : SadConsole.Console
    {
        public Status(int sizeX, int sizeY) : base(sizeX, sizeY)
        {

        }

        public void Display(string txt)
        {
            this.Clear();
            this.Print(2, 0, txt);
        }

        public void Display(string[] txt)
        {
            this.Clear();
            int length = txt.Length < Height ? txt.Length : Height;
            for (int i = 0; i < length; i++)
            {
                this.Print(2, i, txt[i]);
            }
        }
    }
}
