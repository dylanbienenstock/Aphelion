using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.GamerServices;

namespace Aphelion
{
    public static class Utility
    {
        public static Vector2 Round(this Vector2 vector2)
        {
            return new Vector2((float)Math.Round(vector2.X), (float)Math.Round(vector2.Y));
        }

        public static float ScaleX(float number, Aphelion game)
        {
            return number * (game.Window.ClientBounds.Width / 1920.0f);
        }

        public static int ScaleX(int number, Aphelion game)
        {
            return (int)Math.Round((number * (game.Window.ClientBounds.Width / 1920.0f)));
        }

        public static float ScaleY(float number, Aphelion game)
        {
            return number * (game.Window.ClientBounds.Height / 1080.0f);
        }

        public static int ScaleY(int number, Aphelion game)
        {
            return (int)Math.Round((number * (game.Window.ClientBounds.Height / 1080.0f)));
        }

        public static Vector2 ScaleVector2(Vector2 vector, Aphelion game)
        {
            return new Vector2(ScaleX(vector.X, game), ScaleY(vector.Y, game));
        }

        public static void DrawCircle(SpriteBatch sprites, Color color, Vector2 pos, int radius)
        {
            pos = pos.Round();

            for (float i = 0; i < Math.PI * 2; i += 1.0f / radius)
            {
                sprites.Draw(GameContent.Texture("pixel"), pos + new Vector2((float)Math.Sin(i) * radius, (float)Math.Cos(i) * radius), color);
            }
        }

        public static void DrawCircleOptimized(SpriteBatch sprites, Camera camera, Color color, Vector2 pos, int radius)
        {
            pos = pos.Round();

            for (float i = 0; i < Math.PI * 2; i += 1.0f / radius)
            {
                Vector2 pixelPos = pos + new Vector2((float)Math.Sin(i) * radius, (float)Math.Cos(i) * radius);
                Rectangle frustum = new Rectangle((int)(camera.Position.X - camera.Size.X / 2), (int)(camera.Position.Y - camera.Size.Y / 2), (int)camera.Size.X, (int)camera.Size.Y);

                if (frustum.Contains(new Point((int)pixelPos.X, (int)pixelPos.Y)))
                {
                    sprites.Draw(GameContent.Texture("pixel"), pixelPos, color);
                }
            }
        }

        public static void DrawDottedCircle(SpriteBatch sprites, Color color, Vector2 pos, int radius, int sparsity)
        {
            pos = pos.Round();

            for (float i = 0; i < (Math.PI * 2); i += (1.0f / radius) * sparsity)
            {
                if (i + (1.0f / radius) * sparsity < (Math.PI * 2))
                {
                    sprites.Draw(GameContent.Texture("pixel"), pos + new Vector2((float)Math.Sin(i) * radius, (float)Math.Cos(i) * radius), color);
                }
            }
        }

        public static void DrawDottedCircleOptimized(SpriteBatch sprites, Camera camera, Color color, Vector2 pos, int radius, int sparsity)
        {
            pos = pos.Round();

            for (float i = 0; i < (Math.PI * 2); i += (1.0f / radius) * sparsity)
            {
                if (i + (1.0f / radius) * sparsity < (Math.PI * 2))
                {
                    Vector2 pixelPos = pos + new Vector2((float)Math.Sin(i) * radius, (float)Math.Cos(i) * radius);
                    Rectangle frustum = new Rectangle((int)(camera.Position.X - camera.Size.X / 2), (int)(camera.Position.Y - camera.Size.Y / 2), (int)camera.Size.X, (int)camera.Size.Y);

                    if (frustum.Contains(new Point((int)pixelPos.X, (int)pixelPos.Y)))
                    {
                        sprites.Draw(GameContent.Texture("pixel"), pixelPos, color);
                    }
                }
            }
        }

        public static void DrawLine(SpriteBatch sprites, Color color, Vector2 start, Vector2 end)
        {
            start = start.Round();
            end = end.Round();

            Vector2 line = end - start;
            float angle = (float)Math.Atan2(line.Y, line.X);

            sprites.Draw(GameContent.Texture("pixel"), new Rectangle((int)start.X, (int)start.Y, (int)line.Length(), 1), null, color, angle, new Vector2(0, 0), SpriteEffects.None, 0);
        }
    }
}