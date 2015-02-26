using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Aphelion
{
    public static class TextRenderer
    {
        private static int textScale = 1;
        private static Color textColor = Color.White;
        private static SpriteBatch textSpriteBatch;
        private static Texture2D textFont;
        private static int savedScale;
        private static Color savedColor;
        private static SpriteBatch savedSpriteBatch;
        private static Texture2D savedFont;

        public static char[] Characters = new char[] // Every character is 5 x 7 pixels
        {
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J',
            'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T',
            'U', 'V', 'W', 'X', 'Y', 'Z', 'a', 'b', 'c', 'd',
            'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n',
            'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x',
            'y', 'z', '0', '1', '2', '3', '4', '5', '6', '7',
            '8', '9', ' ', '.', '!', '?', ',', '$', (char)162, // Cent symbol
            '+', '-', '*', '/', '%', '#', '@', '&', '_', '(',
            ')', ':', ';', '>', '<', '`', '/', '\\', '\"', '\''
        };

        #region Drawing

        public static void DrawStringRaw(string text, Vector2 position, int scale, Color color, SpriteBatch sprites, Texture2D font)
        {
            int offsetX = 0;
            int offsetY = 0;

            for (int i = 0; i < text.Length; i++)
            {
                int sourceOffset = Array.IndexOf(Characters, text.ToCharArray()[i]);

                if (text.ToCharArray()[i] != '\n')
                {
                    sprites.Draw(font, position + new Vector2(offsetX * (6 * scale), offsetY * (8 * scale)), new Rectangle(sourceOffset * 5, 0, 5, 8), color, 0.0f, Vector2.Zero, scale, SpriteEffects.None, 1.0f);
                }
                else
                {
                    offsetX = -1;
                    offsetY++;
                }

                offsetX++;
            }
        }

        public static void DrawString(string text, Vector2 position)
        {
            //text = text.ToUpper();
            DrawStringRaw(text, position, textScale, textColor, textSpriteBatch, textFont);
        }

        #endregion

        #region Setting

        public static void SetScale(int scale)
        {
            textScale = scale;
        }

        public static void SetColor(Color color)
        {
            textColor = color;
        }

        public static void SetSpriteBatch(SpriteBatch sprites)
        {
            textSpriteBatch = sprites;
        }

        public static void SetFont(Texture2D font)
        {
            textFont = font;
        }

        #endregion

        #region Getting

        public static int GetScale()
        {
            return textScale;
        }

        public static Color GetColor()
        {
            return textColor;
        }

        public static SpriteBatch GetSpriteBatch()
        {
            return textSpriteBatch;
        }

        public static Texture2D GetFont()
        {
            return textFont;
        }

        #endregion

        public static void SaveState()
        {
            savedScale = textScale;
            savedColor = textColor;
            savedSpriteBatch = textSpriteBatch;
            savedFont = textFont;
        }

        public static void RestoreState()
        {
            textScale = savedScale;
            textColor = savedColor;
            textSpriteBatch = savedSpriteBatch;
            textFont = savedFont;
        }

        public static Vector2 MeasureString(string measure)
        {
            Vector2 size = new Vector2(0, 7 * textScale);
            List<string> lines = new List<string>();
            string builtLine = string.Empty;

            foreach (char character in measure.ToArray())
            {
                if (character != '\n')
                {
                    builtLine += character;
                }
                else
                {
                    lines.Add(builtLine);
                    builtLine = string.Empty;
                    size.Y += (7 + 1) * textScale;
                }
            }

            lines.Add(builtLine);

            foreach (string line in lines)
            {
                size.X = Math.Max(size.X, line.Length * (5 + 1) * textScale);
            }

            return size;
        }

        public static char KeyToChar(Keys key)
        {
            KeyboardState keyboard = Keyboard.GetState();
            char keyChar = (char)1;
            bool shift = keyboard.IsKeyDown(Keys.LeftShift) || keyboard.IsKeyDown(Keys.RightShift) || keyboard.IsKeyDown(Keys.CapsLock);
            switch (key)
            {
                case Keys.A: if (shift) { keyChar = 'A'; } else { keyChar = 'a'; } break;
                case Keys.B: if (shift) { keyChar = 'B'; } else { keyChar = 'b'; } break;
                case Keys.C: if (shift) { keyChar = 'C'; } else { keyChar = 'c'; } break;
                case Keys.D: if (shift) { keyChar = 'D'; } else { keyChar = 'd'; } break;
                case Keys.E: if (shift) { keyChar = 'E'; } else { keyChar = 'e'; } break;
                case Keys.F: if (shift) { keyChar = 'F'; } else { keyChar = 'f'; } break;
                case Keys.G: if (shift) { keyChar = 'G'; } else { keyChar = 'g'; } break;
                case Keys.H: if (shift) { keyChar = 'H'; } else { keyChar = 'h'; } break;
                case Keys.I: if (shift) { keyChar = 'I'; } else { keyChar = 'i'; } break;
                case Keys.J: if (shift) { keyChar = 'J'; } else { keyChar = 'j'; } break;
                case Keys.K: if (shift) { keyChar = 'K'; } else { keyChar = 'k'; } break;
                case Keys.L: if (shift) { keyChar = 'L'; } else { keyChar = 'l'; } break;
                case Keys.M: if (shift) { keyChar = 'M'; } else { keyChar = 'm'; } break;
                case Keys.N: if (shift) { keyChar = 'N'; } else { keyChar = 'n'; } break;
                case Keys.O: if (shift) { keyChar = 'O'; } else { keyChar = 'o'; } break;
                case Keys.P: if (shift) { keyChar = 'P'; } else { keyChar = 'p'; } break;
                case Keys.Q: if (shift) { keyChar = 'Q'; } else { keyChar = 'q'; } break;
                case Keys.R: if (shift) { keyChar = 'R'; } else { keyChar = 'r'; } break;
                case Keys.S: if (shift) { keyChar = 'S'; } else { keyChar = 's'; } break;
                case Keys.T: if (shift) { keyChar = 'T'; } else { keyChar = 't'; } break;
                case Keys.U: if (shift) { keyChar = 'U'; } else { keyChar = 'u'; } break;
                case Keys.V: if (shift) { keyChar = 'V'; } else { keyChar = 'v'; } break;
                case Keys.W: if (shift) { keyChar = 'W'; } else { keyChar = 'w'; } break;
                case Keys.X: if (shift) { keyChar = 'X'; } else { keyChar = 'x'; } break;
                case Keys.Y: if (shift) { keyChar = 'Y'; } else { keyChar = 'y'; } break;
                case Keys.Z: if (shift) { keyChar = 'Z'; } else { keyChar = 'z'; } break;
                case Keys.D0: if (shift) { keyChar = ')'; } else { keyChar = '0'; } break;
                case Keys.D1: if (shift) { keyChar = '!'; } else { keyChar = '1'; } break;
                case Keys.D2: if (shift) { keyChar = '@'; } else { keyChar = '2'; } break;
                case Keys.D3: if (shift) { keyChar = '#'; } else { keyChar = '3'; } break;
                case Keys.D4: if (shift) { keyChar = '$'; } else { keyChar = '4'; } break;
                case Keys.D5: if (shift) { keyChar = '%'; } else { keyChar = '5'; } break;
                case Keys.D6: if (shift) { keyChar = '^'; } else { keyChar = '6'; } break;
                case Keys.D7: if (shift) { keyChar = '&'; } else { keyChar = '7'; } break;
                case Keys.D8: if (shift) { keyChar = '*'; } else { keyChar = '8'; } break;
                case Keys.D9: if (shift) { keyChar = '('; } else { keyChar = '9'; } break;
                case Keys.NumPad0: keyChar = '0'; break;
                case Keys.NumPad1: keyChar = '1'; break;
                case Keys.NumPad2: keyChar = '2'; break;
                case Keys.NumPad3: keyChar = '3'; break;
                case Keys.NumPad4: keyChar = '4'; break;
                case Keys.NumPad5: keyChar = '5'; break;
                case Keys.NumPad6: keyChar = '6'; break;
                case Keys.NumPad7: keyChar = '7'; break;
                case Keys.NumPad8: keyChar = '8'; break;
                case Keys.NumPad9: keyChar = '9'; break;
                case Keys.OemTilde: if (shift) { keyChar = '~'; } else { keyChar = '`'; } break;
                case Keys.OemSemicolon: if (shift) { keyChar = ':'; } else { keyChar = ';'; } break;
                case Keys.OemQuotes: if (shift) { keyChar = '"'; } else { keyChar = '\''; } break;
                case Keys.OemQuestion: if (shift) { keyChar = '?'; } else { keyChar = '/'; } break;
                case Keys.OemPlus: if (shift) { keyChar = '+'; } else { keyChar = '='; } break;
                case Keys.OemPipe: if (shift) { keyChar = '|'; } else { keyChar = '\\'; } break;
                case Keys.OemPeriod: if (shift) { keyChar = '>'; } else { keyChar = '.'; } break;
                case Keys.OemOpenBrackets: if (shift) { keyChar = '{'; } else { keyChar = '['; } break;
                case Keys.OemCloseBrackets: if (shift) { keyChar = '}'; } else { keyChar = ']'; } break;
                case Keys.OemMinus: if (shift) { keyChar = '_'; } else { keyChar = '-'; } break;
                case Keys.OemComma: if (shift) { keyChar = '<'; } else { keyChar = ','; } break;
                case Keys.Space: keyChar = ' '; break;
            }

            return keyChar;
        }
    }
}