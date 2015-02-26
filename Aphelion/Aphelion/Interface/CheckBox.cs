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
    public class CheckBox : BaseInterfaceElement
    {
        public bool Checked;
        public int Scale;
        public Color Color;
        public bool Shadow;
        public TextButtonHoverMode HoverMode;
        public ButtonState State;
        public event EventHandler OnIdle;
        public event EventHandler OnHover;
        public event EventHandler OnPress;
        public event EventHandler OnRelease;

        private MouseState lastMouseState;

        public CheckBox()
        {
            Checked = false;
            Position = Vector2.Zero;
            Scale = 1;
            Color = Color.White;
            Shadow = true;
            State = ButtonState.Idle;
            lastMouseState = Mouse.GetState();
        }

        public override Vector2 CalculateDimensions()
        {
            return new Vector2(Scale * 7, Scale * 7);
        }

        public override void Update(GameTime time)
        {
            MouseState mouseState = Mouse.GetState();
            Rectangle mouseRect = new Rectangle((int)mouseState.X, (int)mouseState.Y, 1, 1);
            Vector2 buttonSize = new Vector2(Scale * 7, Scale * 7);

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

                            Checked = !Checked;
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
            Color drawColor = Color;

            if (State == ButtonState.Hovered)
            {
                drawColor = Color.FromNonPremultiplied(drawColor.ToVector4() - new Vector4(0.2f, 0.2f, 0.2f, 0));
            }
            else if (State == ButtonState.Pressed)
            {
                drawColor = Color.FromNonPremultiplied(drawColor.ToVector4() - new Vector4(0.45f, 0.45f, 0.45f, 0));
            }

            if (Shadow)
            {
                Color shadowColor = Color.FromNonPremultiplied(89, 89, 89, 255); // TO DO: Fix this

                sprites.Draw(GameContent.Texture("pixel"), new Rectangle((int)CalculatedPosition.X + Scale, (int)CalculatedPosition.Y + Scale, Scale * 7, Scale), shadowColor);
                sprites.Draw(GameContent.Texture("pixel"), new Rectangle((int)CalculatedPosition.X + Scale, (int)CalculatedPosition.Y + Scale, Scale, Scale * 7), shadowColor);
                sprites.Draw(GameContent.Texture("pixel"), new Rectangle((int)CalculatedPosition.X + Scale, (int)CalculatedPosition.Y + Scale + Scale * 7 - Scale, Scale * 7, Scale), shadowColor);
                sprites.Draw(GameContent.Texture("pixel"), new Rectangle((int)CalculatedPosition.X + Scale + Scale * 7 - Scale, (int)CalculatedPosition.Y + Scale, Scale, Scale * 7), shadowColor);

                if (Checked)
                {
                    sprites.Draw(GameContent.Texture("pixel"), new Rectangle((int)CalculatedPosition.X + Scale + Scale * 2, (int)CalculatedPosition.Y + Scale + Scale * 2, Scale * 7 - Scale * 4, Scale * 7 - Scale * 4), shadowColor);
                }
            }

            sprites.Draw(GameContent.Texture("pixel"), new Rectangle((int)CalculatedPosition.X, (int)CalculatedPosition.Y, Scale * 7, Scale), drawColor);
            sprites.Draw(GameContent.Texture("pixel"), new Rectangle((int)CalculatedPosition.X, (int)CalculatedPosition.Y, Scale, Scale * 7), drawColor);
            sprites.Draw(GameContent.Texture("pixel"), new Rectangle((int)CalculatedPosition.X, (int)CalculatedPosition.Y + Scale * 7 - Scale, Scale * 7, Scale), drawColor);
            sprites.Draw(GameContent.Texture("pixel"), new Rectangle((int)CalculatedPosition.X + Scale * 7 - Scale, (int)CalculatedPosition.Y, Scale, Scale * 7), drawColor);

            if (Checked)
            {
                sprites.Draw(GameContent.Texture("pixel"), new Rectangle((int)CalculatedPosition.X + Scale * 2, (int)CalculatedPosition.Y + Scale * 2, Scale * 7 - Scale * 4, Scale * 7 - Scale * 4), drawColor);
            }
        }
    }
}