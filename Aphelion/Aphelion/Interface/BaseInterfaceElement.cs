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
    public abstract class BaseInterfaceElement
    {
        public BaseInterfaceElement Parent;
        public string Tag;
        public Vector2 Position;

        public Vector2 CalculatedPosition
        {
            get
            {
                if (Parent != null)
                {
                    return Parent.CalculatedPosition + Position;
                }

                return Position;
            }
        }

        public abstract Vector2 CalculateDimensions();
        public abstract void Update(GameTime time);
        public abstract void Draw(SpriteBatch sprites);
    }
}
