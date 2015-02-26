using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Aphelion
{
    public static class Backdrop
    {
        public static void Draw(SpriteBatch sprites, Camera camera)
        {
            Rectangle frustum = new Rectangle((int)(camera.Position.X - camera.Size.X / 2), (int)(camera.Position.Y - camera.Size.Y / 2), (int)camera.Size.X, (int)camera.Size.Y);

            for (int i = 1; i <= 8; i++)
            {
                int systemRadius = (int)Math.Pow(2, 16);
                int starMovementFactor = i * 32;
                Camera starCamera = new Camera();
                starCamera.Size = camera.Size;
                starCamera.Position = new Vector2((camera.Position.X / starMovementFactor) % 1024, (camera.Position.Y / starMovementFactor) % 1024);

                sprites.Begin(SpriteSortMode.Deferred, null, SamplerState.LinearClamp, null, null, null, starCamera.GetTransformation());

                for (int x = -(int)Math.Ceiling(camera.Size.X / 1024) * 1024; x < Math.Ceiling(camera.Size.X / 1024) * 1024; x += 1024)
                {
                    for (int y = -(int)Math.Ceiling(camera.Size.Y / 1024) * 1024; y < Math.Ceiling(camera.Size.Y / 1024) * 1024; y += 1024)
                    {
                        sprites.Draw(GameContent.Texture("environment\\background\\stars" + i), (new Vector2(x, y)).Round(), Color.White);
                    }
                }

                sprites.End();
            }
        }
    }
}
