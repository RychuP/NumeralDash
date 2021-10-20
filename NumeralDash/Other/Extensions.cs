using System;
using NumeralDash.World;
using SadConsole;
using SadRogue.Primitives;

namespace NumeralDash.Other
{
    public static class Extensions
    {
        static char spaceCharAlternative = 'a';

        public static void PrintDraw(this ICellSurface c, int y, string text, TheDrawFont f, HorizontalAlignment alignment, int padding = 0)
        {
            int spaceWidth = f.IsCharacterSupported(' ') ? f.GetCharacter(' ').Width : f.GetCharacter(spaceCharAlternative).Width,
                textLength = 0,
                printWidth = c.Width - padding * 2;
            string tempText = string.Empty;

            foreach (var item in text)
            {
                char currentChar = item;
                int charWidth = 0;

                if (f.IsCharacterSupported(item))
                {
                    var charInfo = f.GetCharacter(currentChar);
                    charWidth = charInfo.Width;
                }
                else
                {
                    currentChar = ' ';
                    charWidth = spaceWidth;
                }

                textLength += charWidth;

                if (textLength > printWidth)
                {
                    textLength -= charWidth;
                    break;
                }

                tempText += currentChar;
            }

            

            int x = alignment switch
            {
                HorizontalAlignment.Center => (printWidth - textLength) / 2,
                HorizontalAlignment.Right => printWidth - textLength,
                _ => 0
            };

            PrintDraw(c, x + padding, y, tempText, f);
        }

        public static void PrintDraw(this ICellSurface c, int x, int y, string text, TheDrawFont f)
        {
            int xPos = x;
            int yPos = y;
            int tempHeight = 0;

            foreach (var item in text)
            {
                if (f.IsCharacterSupported(item))
                {
                    var charInfo = f.GetCharacter(item);

                    if (xPos + charInfo.Width >= c.Width)
                    {
                        yPos += tempHeight + 1;
                        xPos = x;
                    }

                    if (yPos >= c.Height)
                        break;

                    var surfaceCharacter = f.GetSurface(item);
                    surfaceCharacter.Copy(c, xPos, yPos);

                    if (surfaceCharacter.Height > tempHeight)
                        tempHeight = surfaceCharacter.Height;

                    xPos += charInfo.Width;
                }
                else if (item == ' ')
                {
                    if (f.IsCharacterSupported(spaceCharAlternative))
                        xPos += f.GetCharacter(spaceCharAlternative).Width;
                }
            }
        }
    }
}
