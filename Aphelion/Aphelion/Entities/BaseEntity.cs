using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.GamerServices;

namespace Aphelion.Entities
{
    [Serializable]
    public abstract class BaseEntity
    {
        // TO DO: Implement these
        //public int ID;
        //public string Tag;
        public NetworkSide Side;

        public BaseEntity()
        {
            EntityManager.Add(this);
        }

        public abstract void Update();
        public abstract void Draw(SpriteBatch sprites);
    }
}
