﻿using System;
using NumeralDash.World;
using SadConsole;
using SadRogue.Primitives;

namespace NumeralDash.Other
{
    public static class Extensions
    {
        readonly static char spaceCharAlternative = 'a';

        /// <summary>
        /// Prints text using THeDrawFont and horizontal alignment specified. Calculates x coordinate. Truncates string to fit in one line.
        /// </summary>
        /// <param name="y">Y coordinate of the surface.</param>
        /// <param name="padding">Amount of regular font characters used as horizontal padding on both sides of the output.</param>
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

        /// <summary>
        /// Prints the text using TheDrawFont.
        /// </summary>
        /// <param name="x">X coordinate of the surface.</param>
        /// <param name="y">Y coordinate of the surface.</param>
        /// <param name="text">Text to print.</param>
        /// <param name="f">Font to use.</param>
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

        public static void PrintCenter(this ICellSurface c, int y, string text)
        {
            c.Print(0, y, text.Align(HorizontalAlignment.Center, c.Width));
        }
    }
}
