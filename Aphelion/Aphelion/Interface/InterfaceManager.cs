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
    public static class InterfaceManager
    {
        private static BaseInterfaceElement __Focus;
        public static BaseInterfaceElement Focus { get { return __Focus; } }

        static List<BaseInterfaceElement> elements = new List<BaseInterfaceElement>();
        static bool ignoreInput = false;

        public static void SetFocus(BaseInterfaceElement element)
        {
            __Focus = element;
        }

        public static void ClearFocus()
        {
            __Focus = null;
        }

        public static void IgnoreInput(bool enabled)
        {
            ignoreInput = enabled;
        }

        public static void Add(BaseInterfaceElement element)
        {
            elements.Add(element);
        }

        public static void Remove(BaseInterfaceElement element)
        {
            elements.Remove(element);
        }

        public static void RemoveAllWithTag(string tag)
        {
            Predicate<BaseInterfaceElement> elementRemovalPredicate = (element) => { return element.Tag == tag; };

            elements.RemoveAll(elementRemovalPredicate);
        }

        public static List<BaseInterfaceElement> GetAllElementsWithTag(string tag)
        {
            Predicate<BaseInterfaceElement> elementRetrievalPredicate = (element) => { return element.Tag == tag; };

            return elements.FindAll(elementRetrievalPredicate);
        }

        public static void Update(GameTime time)
        {
            if (!ignoreInput)
            {
                try
                {
                    foreach (BaseInterfaceElement element in elements.ToList())
                    {
                        element.Update(time);
                    }
                }
                catch
                {

                }
            }
        }

        public static void Draw(SpriteBatch sprites)
        {
            sprites.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, RasterizerState.CullNone);

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

            sprites.End();
        }
    }
}
