using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Aphelion.Entities;

namespace Aphelion
{
    public static class HUD
    {
        private static Vector2 targetPos;
        private static bool drawTarget = false;
        private static MouseState lastMouseState = Mouse.GetState();

        public static void Draw(SpriteBatch sprites, Aphelion aphelion)
        {
            sprites.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null);

            DrawInfo(sprites, aphelion);
            DrawRadar(sprites, aphelion);

            sprites.End();
        }

        private static void DrawInfo(SpriteBatch sprites, Aphelion aphelion)
        {
            if (NetworkHelper.LocalPlayer != null) // TO DO: Make it so it doesn't have to wait for the server to create localplayer
            {
                int velocity = (int)Math.Round(NetworkHelper.LocalPlayer.Velocity.Length() * (1000 / 15));
                string info = string.Format("Position: ({0}, {1})\nVelocity: {2} pixels/s", Math.Round(NetworkHelper.LocalPlayer.Position.X / 10) * 10, Math.Round(NetworkHelper.LocalPlayer.Position.Y / 10) * 10, velocity);

                TextRenderer.SaveState();
                TextRenderer.SetScale(2);
                TextRenderer.SetColor(Color.White);
                TextRenderer.DrawString(info, new Vector2(8, 8));
                Vector2 circlePos = new Vector2(8 + 25, TextRenderer.MeasureString(info).Y + 16 + 25);
                Vector2 velocityDirection = NetworkHelper.LocalPlayer.Velocity;
                velocityDirection.Normalize();
                Vector2 direction = new Vector2((float)Math.Cos(NetworkHelper.LocalPlayer.RenderAngle), (float)Math.Sin(NetworkHelper.LocalPlayer.RenderAngle));
                TextRenderer.SetScale(1);
                Vector2 circleV = circlePos + velocityDirection * 25;
                Vector2 circleA = circlePos + direction * 25;
                circleV = new Vector2((int)Math.Round(circleV.X), (int)Math.Round(circleV.Y));
                circleA = new Vector2((int)Math.Round(circleA.X), (int)Math.Round(circleA.Y));
                TextRenderer.DrawString("V", circleV + new Vector2(8, -3));
                TextRenderer.DrawString("A", circleA + new Vector2(8, -3));
                Utility.DrawCircle(sprites, Color.White, circlePos, 25);
                Utility.DrawCircle(sprites, Color.White, circleV, 4);
                Utility.DrawCircle(sprites, Color.White, circleA, 4);

                TextRenderer.RestoreState();
            }
        }

        private static void DrawRadar(SpriteBatch sprites, Aphelion aphelion) // TO DO: Give the radar it's own file (Radar.cs)
        {
            int ratio = (int)Math.Pow(2, 9);
            Vector2 radarOrigin = aphelion.ScreenBounds - new Vector2(128, 128) - new Vector2(16, 16);
            Utility.DrawDottedCircle(sprites, Color.White, radarOrigin, 128, 16);

            #region Sun & Planets

            int sunRadius = 6958;
            Vector2 sunPos = Vector2.Zero;

            int mercuryRadius = 24;
            int mercuryDistance = 8368;
            Vector2 mercuryPos = new Vector2(sunRadius + mercuryDistance, 0);

            int venusRadius = 60;
            int venusDistance = 8368;
            Vector2 venusPos = new Vector2(mercuryPos.X + venusDistance, 0);

            Vector2 center = radarOrigin - new Vector2(2, 3);
            TextRenderer.SaveState();
            TextRenderer.SetScale(1);
            TextRenderer.SetColor(Color.Red);
            Utility.DrawCircle(sprites, Color.Red, radarOrigin, sunRadius / ratio);
            Utility.DrawCircle(sprites, Color.Orange, radarOrigin, sunRadius / ratio - 2);
            TextRenderer.DrawString("S", center);
            TextRenderer.SetColor(Color.Gray);
            TextRenderer.DrawString("M", center + mercuryPos / ratio);
            TextRenderer.SetColor(Color.DarkOrange);
            TextRenderer.DrawString("V", center + venusPos / ratio);
            TextRenderer.RestoreState();

            #endregion

            if (NetworkHelper.LocalPlayer != null)
            {
                Vector2 localPlayerPos = radarOrigin + NetworkHelper.LocalPlayer.Position / ratio;

                sprites.Draw(GameContent.Texture("pixel"), new Rectangle((int)localPlayerPos.X - 1, (int)localPlayerPos.Y - 1, 3, 3), Color.White);
            }

            foreach (Player player in NetworkHelper.Players)
            {
                Vector2 playerPos = radarOrigin + player.Position / ratio;

                sprites.Draw(GameContent.Texture("pixel"), new Rectangle((int)playerPos.X - 1, (int)playerPos.Y - 1, 3, 3), Color.LawnGreen);
            }

            // TargetPos System

            MouseState mouseState = Mouse.GetState();
            Vector2 mousePos = new Vector2(mouseState.X, mouseState.Y);

            if (Vector2.Distance(radarOrigin, mousePos) <= 128)
            {

                aphelion.IsMouseVisible = false;

                Utility.DrawLine(sprites, Color.White, mousePos + new Vector2(0, 4), mousePos + new Vector2(0, 4 + 16));
                Utility.DrawLine(sprites, Color.White, mousePos - new Vector2(0, 4), mousePos - new Vector2(0, 4 + 16));
                Utility.DrawLine(sprites, Color.White, mousePos + new Vector2(4, 0), mousePos + new Vector2(4 + 16, 0));
                Utility.DrawLine(sprites, Color.White, mousePos - new Vector2(4, 0), mousePos - new Vector2(4 + 16, 0));

                if (mouseState.LeftButton == ButtonState.Pressed/* && lastMouseState.LeftButton == ButtonState.Released*/)
                {
                    targetPos = ((aphelion.ScreenBounds - new Vector2(128, 128) - new Vector2(16, 16)) - new Vector2(mouseState.X, mouseState.Y)) * -ratio;
                    drawTarget = true;
                }
                else if (mouseState.RightButton == ButtonState.Pressed && lastMouseState.RightButton == ButtonState.Released)
                {
                    drawTarget = false;
                }
            }
            else
            {
                aphelion.IsMouseVisible = true;
            }

            lastMouseState = mouseState;

            if (targetPos != null && drawTarget)
            {
                Vector2 targetDrawPos = radarOrigin + targetPos / ratio;

                Utility.DrawLine(sprites, Color.White, targetDrawPos + new Vector2(0, 2), targetDrawPos + new Vector2(0, 5));
                Utility.DrawLine(sprites, Color.White, targetDrawPos - new Vector2(0, 2), targetDrawPos - new Vector2(0, 5));
                Utility.DrawLine(sprites, Color.White, targetDrawPos + new Vector2(2, 0), targetDrawPos + new Vector2(5, 0));
                Utility.DrawLine(sprites, Color.White, targetDrawPos - new Vector2(2, 0), targetDrawPos - new Vector2(5, 0));

                if (NetworkHelper.LocalPlayer != null)
                {
                    Vector2 localPlayerPos = radarOrigin + NetworkHelper.LocalPlayer.Position / ratio;

                    if (Vector2.Distance(NetworkHelper.LocalPlayer.Position, targetPos) > 64)
                    {
                        Vector2 tangent = (targetDrawPos - localPlayerPos); // TO DO: Figure out if this is actually what a tangent is. I'm only in Algebra 2 ¯\_(ツ)_/¯ 
                        tangent.Normalize();
                        Utility.DrawLine(sprites, Color.White, targetDrawPos - tangent * 5, localPlayerPos + tangent * 3);
                    }

                    // SHADY CODE
                    sprites.End();

                    sprites.Begin(SpriteSortMode.Deferred, null, SamplerState.LinearClamp, null, null, null, aphelion.MainCamera.GetTransformation());

                    if (Vector2.Distance(NetworkHelper.LocalPlayer.Position, targetPos) > 64)
                    {
                        Vector2 tangent2 = (targetPos - NetworkHelper.LocalPlayer.Position);
                        tangent2.Normalize();
                        Utility.DrawLine(sprites, Color.White, NetworkHelper.LocalPlayer.Position + tangent2 * 25, targetPos - tangent2 * 30);
                    }

                    Utility.DrawLine(sprites, Color.White, targetPos + new Vector2(0, 4), targetPos + new Vector2(0, 4 + 16));
                    Utility.DrawLine(sprites, Color.White, targetPos - new Vector2(0, 4), targetPos - new Vector2(0, 4 + 16));
                    Utility.DrawLine(sprites, Color.White, targetPos + new Vector2(4, 0), targetPos + new Vector2(4 + 16, 0));
                    Utility.DrawLine(sprites, Color.White, targetPos - new Vector2(4, 0), targetPos - new Vector2(4 + 16, 0));

                    sprites.End();

                    sprites.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null);
                    // END SHADY CODE
                }
            }
        }
    }
}
