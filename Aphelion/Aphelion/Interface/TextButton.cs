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
    public enum TextButtonHoverMode
    {
        Darken,
        PointAt // Used for the title menu
    }

    public class TextButton : BaseInterfaceElement
    {
        public string Text;
        public int Scale;
        public Color Color;
        public Texture2D Font;
        public bool Shadow;
        public TextButtonHoverMode HoverMode;
        public ButtonState State;
        public event EventHandler OnIdle;
        public event EventHandler OnHover;
        public event EventHandler OnPress;
        public event EventHandler OnRelease;

        private MouseState lastMouseState;

        public TextButton()
        {
            Text = string.Empty;
            Position = Vector2.Zero;
            Scale = 1;
            Color = Color.White;
            Font = TextRenderer.GetFont();
            Shadow = true;
            HoverMode = TextButtonHoverMode.Darken;
            State = ButtonState.Idle;
            lastMouseState = Mouse.GetState();
        }

        public override Vector2 CalculateDimensions()
        {
            TextRenderer.SaveState();

            TextRenderer.SetScale(Scale);

            Vector2 dimensions = TextRenderer.MeasureString(Text);

            TextRenderer.RestoreState();

            return dimensions;
        }

        public override void Update(GameTime time)
        {
            TextRenderer.SaveState();

            TextRenderer.SetScale(Scale);

            MouseState mouseState = Mouse.GetState();
            Rectangle mouseRect = new Rectangle((int)mouseState.X, (int)mouseState.Y, 1, 1);
            Vector2 buttonSize = TextRenderer.MeasureString(Text);

            TextRenderer.RestoreState();

            if (State != ButtonState.Disabled)
            {
                if (State != ButtonState.Pressed)
                {
                    if (mouseRect.Intersects(new Rectangle((int)CalculatedPosition.X, (int)CalculatedPosition.Y, (int)buttonSize.X, (int)buttonSize.Y)))
                    {
                        //System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Hand;

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
                        if (mouseRect.Intersects(new Rectangle((int)CalculatedPosition.X, (int)CalculatedPosition.Y, (int)buttonSize.X, (int)buttonSize.Y)))
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
            string drawText = Text;
            Color drawColor = Color;

            if (HoverMode == TextButtonHoverMode.Darken && State == ButtonState.Hovered)
            {
                drawColor = Color.FromNonPremultiplied(drawColor.ToVector4() - new Vector4(0.2f, 0.2f, 0.2f, 0));
            }
            else if (HoverMode == TextButtonHoverMode.PointAt && (State == ButtonState.Hovered || State == ButtonState.Pressed))
            {
                drawText = '>' + Text;
            }

            if (State == ButtonState.Pressed)
            {
                drawColor = Color.FromNonPremultiplied(drawColor.ToVector4() - new Vector4(0.45f, 0.45f, 0.45f, 0));
            }

            TextRenderer.SaveState();

            TextRenderer.SetFont(Font);
            TextRenderer.SetScale(Scale);
            TextRenderer.SetSpriteBatch(sprites);

            if (Shadow)
            {
                TextRenderer.SetColor(Color.FromNonPremultiplied(Color.Gray.R, Color.Gray.G, Color.Gray.B, 100));

                TextRenderer.DrawString(drawText, CalculatedPosition + new Vector2(Scale, Scale));
            }

            TextRenderer.SetColor(drawColor);

            TextRenderer.DrawString(drawText, CalculatedPosition);

            TextRenderer.RestoreState();
        }
    }
}