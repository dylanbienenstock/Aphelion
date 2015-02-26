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
    public class ImageButton : BaseInterfaceElement
    {
        public Texture2D Image;
        public ButtonState State;
        public event EventHandler OnIdle;
        public event EventHandler OnHover;
        public event EventHandler OnPress;
        public event EventHandler OnRelease;

        private MouseState lastMouseState;

        public ImageButton()
        {
            Position = Vector2.Zero;
            Image = GameContent.Texture(@"icons\accept");
            State = ButtonState.Idle;

            lastMouseState = Mouse.GetState();
        }

        public override Vector2 CalculateDimensions()
        {
            return new Vector2(Image.Width, Image.Height);
        }

        public override void Update(GameTime time)
        {
            MouseState mouseState = Mouse.GetState();
            Rectangle mouseRect = new Rectangle((int)mouseState.X, (int)mouseState.Y, 1, 1);
            Vector2 buttonSize = new Vector2(Image.Width, Image.Height);

            if (State != ButtonState.Disabled)
            {
                if (State != ButtonState.Pressed)
                {
                    if (mouseRect.Intersects(new Rectangle((int)CalculatedPosition.X, (int)CalculatedPosition.Y, (int)buttonSize.X, (int)buttonSize.Y)))
                    {
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
            sprites.End();

            sprites.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null);

            sprites.Draw(Image, CalculatedPosition, Color.White);

            if (State == ButtonState.Hovered || State == ButtonState.Pressed)
            {
                sprites.Draw(Image, new Rectangle((int)CalculatedPosition.X, (int)CalculatedPosition.Y, Image.Width, Image.Height), Color.FromNonPremultiplied(100, 100, 100, 100));
            }

            if (State == ButtonState.Pressed)
            {
                sprites.Draw(Image, new Rectangle((int)CalculatedPosition.X, (int)CalculatedPosition.Y, Image.Width, Image.Height), Color.FromNonPremultiplied(100, 100, 100, 200));
            }

            sprites.End();

            sprites.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
        }
    }
}