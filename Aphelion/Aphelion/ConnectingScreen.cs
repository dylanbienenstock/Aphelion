using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Lidgren.Network;

namespace Aphelion
{
    public static class ConnectingScreen
    {
        public static string Text = "CONNECTING";
        public static Color CurrentColor = Color.White;
        public static Color DestColor = Color.White;

        public static void Draw(Aphelion aphelion, SpriteBatch sprites, GameTime time)
        {
            sprites.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, RasterizerState.CullNone);

            sprites.Draw(GameContent.Texture("pixel"), new Rectangle(0, 0, (int)aphelion.ScreenBounds.X, (int)aphelion.ScreenBounds.Y), Color.Black);

            CurrentColor = Color.Lerp(CurrentColor, DestColor, 0.05f);

            TextRenderer.SaveState();
            TextRenderer.SetScale(2);
            TextRenderer.SetColor(Color.FromNonPremultiplied(CurrentColor.R, CurrentColor.G, CurrentColor.B, 155 - (int)(Math.Cos(time.TotalGameTime.TotalSeconds) * 100)));

            Vector2 textSize = TextRenderer.MeasureString(Text);
            TextRenderer.DrawString(Text, aphelion.ScreenBounds / 2 - textSize / 2);
            TextRenderer.RestoreState();

            for (int i = 0; i < 4; i++)
            {
                Utility.DrawCircle(sprites, Color.FromNonPremultiplied(CurrentColor.R, CurrentColor.G, CurrentColor.B, 155 - (int)(Math.Cos(time.TotalGameTime.TotalSeconds + i) * 100)), aphelion.ScreenBounds / 2, (int)textSize.X / 2 + 64 + (int)(Math.Cos(time.TotalGameTime.TotalSeconds + i) * 32));
            }

            sprites.End();
        }
    }
}
