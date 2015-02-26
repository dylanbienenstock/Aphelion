using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.GamerServices;

namespace Aphelion.Interface
{
    public enum TextBoxInputMode
    {
        Anything,
        Alphabetical,
        Numerical
    }

    public class TextBox : BaseInterfaceElement
    {
        private string __Text;
        public string Text
        {
            set { __Text = value; }
            get
            {
                EventHandler handler = TextChanged;

                if (handler != null)
                {
                    handler(this, new EventArgs());
                }

                return __Text;
            }
        }

        public TextBoxInputMode Mode;
        public int BorderScale;
        public int Scale;
        public int MaxLength;
        public Color BorderColor;
        public Color BackgroundColor;
        public Color Color;
        public Texture2D Texture;
        public Texture2D Font;
        public bool Shadow;
        public bool Enabled;

        public ButtonState State;
        public event EventHandler OnIdle;
        public event EventHandler OnHover;
        public event EventHandler OnPress;
        public event EventHandler OnRelease;
        public event EventHandler TextChanged;

        private bool cursor;
        private MouseState lastMouseState;
        private KeyboardState lastKeyboardState;

        public TextBox()
        {
            Text = string.Empty;
            Mode = TextBoxInputMode.Anything;
            Position = Vector2.Zero;
            BorderScale = 1;
            Scale = 1;
            MaxLength = 8;
            BorderColor = Color.Gray;
            BackgroundColor = Color.FromNonPremultiplied(new Vector4(0, 0, 0, 0.25f));
            Color = Color.White;
            Texture = GameContent.Texture("pixel");
            Font = TextRenderer.GetFont();
            Shadow = true;
            Enabled = true;
            State = ButtonState.Idle;
            cursor = false;
            lastMouseState = Mouse.GetState();
            lastKeyboardState = Keyboard.GetState();
        }

        public override Vector2 CalculateDimensions()
        {
            TextRenderer.SaveState();

            TextRenderer.SetScale(Scale);

            Vector2 dimensions = TextRenderer.MeasureString(new string(' ', MaxLength)) + new Vector2(BorderScale * 4 + Scale, BorderScale * 4);

            TextRenderer.RestoreState();

            return dimensions;
        }

        public override void Update(GameTime time)
        {
            cursor = ((int)Math.Round((float)(time.TotalGameTime.Milliseconds / 250)) * 250) % 750 == 0;

            if (Enabled && InterfaceManager.Focus == this)
            {
                foreach (Keys key in Keyboard.GetState().GetPressedKeys())
                {
                    if (!lastKeyboardState.IsKeyDown(key))
                    {
                        if (Keyboard.GetState().IsKeyDown(Keys.Back))
                        {
                            Text = Text.Remove(Text.Length - 1);
                        }
                        else if (Text.Length < MaxLength && TextRenderer.KeyToChar(key) != (char)1)
                        {
                            if (Mode == TextBoxInputMode.Anything)
                            {
                                Text += TextRenderer.KeyToChar(key);
                            }
                            else if (Mode == TextBoxInputMode.Alphabetical && "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".Contains(TextRenderer.KeyToChar(key)))
                            {
                                Text += TextRenderer.KeyToChar(key);
                            }
                            else if (Mode == TextBoxInputMode.Numerical && "1234567890.".Contains(TextRenderer.KeyToChar(key)))
                            {
                                Text += TextRenderer.KeyToChar(key);
                            }
                        }
                    }
                }
            }

            lastKeyboardState = Keyboard.GetState();

            TextRenderer.SaveState();

            TextRenderer.SetScale(Scale);

            MouseState mouseState = Mouse.GetState();
            Rectangle mouseRect = new Rectangle((int)mouseState.X, (int)mouseState.Y, 1, 1);
            Vector2 textboxSize = CalculateDimensions();

            TextRenderer.RestoreState();

            if (State != ButtonState.Disabled)
            {
                if (State != ButtonState.Pressed)
                {
                    if (mouseRect.Intersects(new Rectangle((int)CalculatedPosition.X, (int)CalculatedPosition.Y, (int)textboxSize.X, (int)textboxSize.Y)))
                    {
                        //System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.IBeam;

                        if (State != ButtonState.Hovered)
                        {
                            EventHandler handler = OnHover;

                            if (handler != null)
                            {
                                handler(this, new EventArgs());
                            }

                            State = ButtonState.Hovered;
                        }
                        else
                        {
                            if (mouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed && lastMouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Released)
                            {
                                EventHandler handler = OnPress;

                                if (handler != null)
                                {
                                    handler(this, new EventArgs());
                                }

                                State = ButtonState.Pressed;
                            }
                        }
                    }
                    else
                    {
                        if (State != ButtonState.Idle)
                        {
                            EventHandler handler = OnIdle;

                            if (handler != null)
                            {
                                handler(this, new EventArgs());
                            }

                            State = ButtonState.Idle;
                        }
                    }
                }
                else
                {
                    if (mouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Released)
                    {
                        if (mouseRect.Intersects(new Rectangle((int)CalculatedPosition.X, (int)CalculatedPosition.Y, (int)textboxSize.X, (int)textboxSize.Y)))
                        {
                            EventHandler handler = OnRelease;

                            if (handler != null)
                            {
                                handler(this, new EventArgs());
                            }

                            State = ButtonState.Hovered;
                            InterfaceManager.SetFocus(this);
                        }
                        else
                        {
                            EventHandler handler = OnRelease;

                            if (handler != null)
                            {
                                handler(this, new EventArgs());
                            }

                            State = ButtonState.Idle;
                        }
                    }
                }
            }

            lastMouseState = mouseState;
        }

        public override void Draw(SpriteBatch sprites)
        {
            sprites.Draw(Texture, new Rectangle((int)CalculatedPosition.X + BorderScale, (int)CalculatedPosition.Y + BorderScale, (int)CalculateDimensions().X - BorderScale * 2, (int)CalculateDimensions().Y - BorderScale * 2), BackgroundColor);

            sprites.Draw(Texture, new Rectangle((int)CalculatedPosition.X + BorderScale, (int)CalculatedPosition.Y, (int)CalculateDimensions().X - BorderScale * 2, BorderScale), BorderColor);
            sprites.Draw(Texture, new Rectangle((int)CalculatedPosition.X + BorderScale, (int)CalculatedPosition.Y + (int)CalculateDimensions().Y - BorderScale, (int)CalculateDimensions().X - BorderScale * 2, BorderScale), BorderColor);
            sprites.Draw(Texture, new Rectangle((int)CalculatedPosition.X, (int)CalculatedPosition.Y + BorderScale, BorderScale, (int)CalculateDimensions().Y - BorderScale * 2), BorderColor);
            sprites.Draw(Texture, new Rectangle((int)CalculatedPosition.X + (int)CalculateDimensions().X - BorderScale, (int)CalculatedPosition.Y + BorderScale, BorderScale, (int)CalculateDimensions().Y - BorderScale * 2), BorderColor);

            TextRenderer.SaveState();

            TextRenderer.SetFont(Font);
            TextRenderer.SetScale(Scale);
            TextRenderer.SetSpriteBatch(sprites);

            TextRenderer.SetColor(Color);

            TextRenderer.DrawString(Text, new Vector2((int)CalculatedPosition.X + BorderScale * 2, (int)CalculatedPosition.Y + BorderScale * 2));

            if (Enabled && cursor && InterfaceManager.Focus == this)
            {
                sprites.Draw(Texture, new Rectangle((int)CalculatedPosition.X + (int)TextRenderer.MeasureString(Text).X + BorderScale * 2, (int)CalculatedPosition.Y + BorderScale * 2, Scale, 7 * Scale), Color.FromNonPremultiplied(200, 200, 200, 255));
            }

            TextRenderer.RestoreState();

            // Debug
            //sprites.Draw(GameContent.Texture("pixel"), CalculatedPosition, Color.Red);
            //sprites.Draw(GameContent.Texture("pixel"), CalculatedPosition + CalculateDimensions(), Color.Red);
        }
    }
}