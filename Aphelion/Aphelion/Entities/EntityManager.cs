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
    public static class EntityManager
    {
        private static NetworkSide side = NetworkSide.Client;
        private static List<BaseEntity> entities = new List<BaseEntity>();

        public static void Update()
        {
            foreach (BaseEntity entity in entities)
            {
                entity.Side = side;
                entity.Update();
            }
        }

        public static void Draw(SpriteBatch sprites)
        {
            foreach (BaseEntity entity in entities)
            {
                entity.Draw(sprites);
            }
        }

        public static void Add(BaseEntity entity)
        {
            entities.Add(entity);
        }

        public static void Remove(BaseEntity entity)
        {
            entities.Remove(entity);
        }

        //public static void RemoveAllWithTag(string tag)
        //{
        //    Predicate<BaseEntity> entityRemovalPredicate = (BaseEntity entity) => { return entity.Tag == tag; };

        //    entities.RemoveAll(entityRemovalPredicate);
        //}
    }
}
