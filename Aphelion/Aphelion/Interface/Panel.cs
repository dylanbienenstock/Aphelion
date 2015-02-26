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
    public class Panel : BaseInterfaceElement
    {
        public Vector2 Dimensions;
        public bool AutoSize;
        public int BorderScale;
        public Color BorderColor;
        public Color Color;
        public Texture2D Texture;

        private List<BaseInterfaceElement> elements = new List<BaseInterfaceElement>();

        public Panel()
        {
            Position = Vector2.Zero;
            BorderScale = 1;
            BorderColor = Color.Gray;
            Color = Color.FromNonPremultiplied(new Vector4(1, 1, 1, 0.25f));
            Texture = GameContent.Texture("pixel");
        }

        public override Vector2 CalculateDimensions()
        {
            if (AutoSize)
            {
                Vector2 minPosition = new Vector2(elements[0].Position.X, elements[0].Position.Y);
                Vector2 maxPosition = new Vector2(elements[0].Position.X + elements[0].CalculateDimensions().X, elements[0].Position.Y + elements[0].CalculateDimensions().Y);

                foreach (BaseInterfaceElement element in elements)
                {
                    if (element.Position.X < minPosition.X)
                    {
                        minPosition.X = element.Position.X;
                    }
                    else if (element.Position.X + element.CalculateDimensions().X > maxPosition.X)
                    {
                        maxPosition.X = element.Position.X + element.CalculateDimensions().X;
                    }

                    if (element.Position.Y < minPosition.Y)
                    {
                        minPosition.Y = element.Position.Y;
                    }
                    else if (element.Position.Y + element.CalculateDimensions().Y > maxPosition.Y)
                    {
                        maxPosition.Y = element.Position.Y + element.CalculateDimensions().Y;
                    }
                }

                return new Vector2(minPosition.X + maxPosition.X, minPosition.Y + maxPosition.Y);
            }

            return Dimensions;
        }

        public override void Update(GameTime time)
        {
            try
            {
                foreach (BaseInterfaceElement element in elements)
                {
                    element.Update(time);
                }
            }
            catch
            {

            }
        }

        public override void Draw(SpriteBatch sprites)
        {
            sprites.Draw(Texture, new Rectangle((int)CalculatedPosition.X, (int)CalculatedPosition.Y, (int)CalculateDimensions().X, (int)CalculateDimensions().Y), Color);

            sprites.Draw(Texture, new Rectangle((int)CalculatedPosition.X, (int)CalculatedPosition.Y - BorderScale, (int)CalculateDimensions().X, BorderScale), BorderColor);
            sprites.Draw(Texture, new Rectangle((int)CalculatedPosition.X, (int)CalculatedPosition.Y + (int)CalculateDimensions().Y, (int)CalculateDimensions().X, BorderScale), BorderColor);
            sprites.Draw(Texture, new Rectangle((int)CalculatedPosition.X - BorderScale, (int)CalculatedPosition.Y, BorderScale, (int)CalculateDimensions().Y), BorderColor);
            sprites.Draw(Texture, new Rectangle((int)CalculatedPosition.X + (int)CalculateDimensions().X, (int)CalculatedPosition.Y, BorderScale, (int)CalculateDimensions().Y), BorderColor);

            try
            {
                foreach (BaseInterfaceElement element in elements)
                {
                    element.Draw(sprites);
                }
            }
            catch
            {

            }
        }

        public void Add(BaseInterfaceElement element)
        {
            element.Parent = this;
            elements.Add(element);
        }
    }
}
