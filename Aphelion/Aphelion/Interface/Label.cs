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
    public class Label : BaseInterfaceElement
    {
        public string Text;
        public int Scale;
        public Color Color;
        public Texture2D Font;
        public bool Shadow;

        public Label()
        {
            Text = string.Empty;
            Position = Vector2.Zero;
            Scale = 1;
            Color = Color.White;
            Font = TextRenderer.GetFont();
            Shadow = true;
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

        }

        public override void Draw(SpriteBatch sprites)
        {
            TextRenderer.SaveState();

            TextRenderer.SetFont(Font);
            TextRenderer.SetScale(Scale);
            TextRenderer.SetSpriteBatch(sprites);

            if (Shadow)
            {
                TextRenderer.SetColor(Color.FromNonPremultiplied(Color.Gray.R, Color.Gray.G, Color.Gray.B, 100));

                TextRenderer.DrawString(Text, CalculatedPosition + new Vector2(Scale, Scale));
            }

            TextRenderer.SetColor(Color);

            TextRenderer.DrawString(Text, CalculatedPosition);

            TextRenderer.RestoreState();
        }
    }
}
