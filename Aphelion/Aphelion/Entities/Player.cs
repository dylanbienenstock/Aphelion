using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aphelion;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Aphelion.Entities
{
    [Serializable]
    public class Player : BaseEntity
    {
        public bool IsLocalPlayer;
        public string Name;
        public Vector2 Position;
        public Vector2 ServerPosition;
        [NonSerialized]
        public Vector2 RenderPosition;
        public Vector2 Velocity;
        public float Angle;
        [NonSerialized]
        public float RenderAngle;
        public bool MovingForward;
        public bool MovingBackward;
        public bool TurningLeft;
        public bool TurningRight;
        public bool Boosting;
        // TO DO: Add public int Ping

        public Player(bool isLocalPlayer, string name, Vector2 position, Vector2 velocity, float angle)
        {
            IsLocalPlayer = isLocalPlayer;
            Name = name;
            Position = position;
            ServerPosition = position;
            RenderPosition = position;
            Velocity = velocity;
            Angle = angle;
            MovingForward = false;
            MovingBackward = false;
            TurningLeft = false;
            TurningRight = false;
            Boosting = false;
        }

        public override void Draw(SpriteBatch sprites)
        {
            Texture2D defaultShip = GameContent.Texture("ships\\default\\base");
            Texture2D thrust = GameContent.Texture("ships\\default\\forward");
            KeyboardState keyboardState = Keyboard.GetState();

            sprites.Draw(defaultShip, new Rectangle((int)RenderPosition.X, (int)RenderPosition.Y, defaultShip.Width, defaultShip.Height), null, Color.White, RenderAngle + MathHelper.ToRadians(90), new Vector2(defaultShip.Width / 2, defaultShip.Height / 2), SpriteEffects.None, 0);

            if (IsLocalPlayer)
            {
                if (keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.W) && !keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.S))
                {
                    sprites.Draw(thrust, new Rectangle((int)RenderPosition.X, (int)RenderPosition.Y, thrust.Width, thrust.Height), null, Color.White, RenderAngle + MathHelper.ToRadians(90), new Vector2(thrust.Width / 2, thrust.Height / 2), SpriteEffects.None, 0);

                    if (keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift))
                    {
                        Vector2 direction = new Vector2((float)Math.Cos(RenderAngle), (float)Math.Sin(RenderAngle));
                        Color[] colors = new Color[] { Color.Red, Color.Orange };
                        Random random = new Random();

                        for (int i = 0; i < 3; i++)
                        {
                            Particle particle = new Particle();
                            particle.Life = random.Next(500, 1250);
                            particle.Color = colors[random.Next(colors.Length)];
                            particle.Position = RenderPosition - (defaultShip.Height / 2) * direction + new Vector2(random.Next(-2, 2), random.Next(-2, 2));
                            particle.Velocity = Velocity - direction * 0.05f + new Vector2(((float)random.NextDouble() - 0.5f) * 2.0f, ((float)random.NextDouble() - 0.5f) * 2.0f);
                            Particles.Add(particle);
                        }
                    }
                }
                else if (!keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.W) && keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.S))
                {
                    sprites.Draw(GameContent.Texture("ships\\default\\backward"), new Rectangle((int)RenderPosition.X, (int)RenderPosition.Y, thrust.Width, thrust.Height), null, Color.White, RenderAngle + MathHelper.ToRadians(90), new Vector2(thrust.Width / 2, thrust.Height / 2), SpriteEffects.None, 0);
                }

                if (keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.A) && !keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.D))
                {
                    sprites.Draw(GameContent.Texture("ships\\default\\left"), new Rectangle((int)RenderPosition.X, (int)RenderPosition.Y, thrust.Width, thrust.Height), null, Color.White, RenderAngle + MathHelper.ToRadians(90), new Vector2(thrust.Width / 2, thrust.Height / 2), SpriteEffects.None, 0);
                }
                else if (!keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.A) && keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.D))
                {
                    sprites.Draw(GameContent.Texture("ships\\default\\right"), new Rectangle((int)RenderPosition.X, (int)RenderPosition.Y, thrust.Width, thrust.Height), null, Color.White, RenderAngle + MathHelper.ToRadians(90), new Vector2(thrust.Width / 2, thrust.Height / 2), SpriteEffects.None, 0);
                }
            }
            else
            {
                Utility.DrawCircle(sprites, Color.LawnGreen, RenderPosition, 25);

                TextRenderer.SaveState();
                TextRenderer.SetScale(1);
                TextRenderer.SetColor(Color.White);
                TextRenderer.DrawString(Name, RenderPosition.Round() + new Vector2(16, -3));
                TextRenderer.RestoreState();

                if (MovingForward && !MovingBackward)
                {
                    sprites.Draw(thrust, new Rectangle((int)RenderPosition.X, (int)RenderPosition.Y, thrust.Width, thrust.Height), null, Color.White, RenderAngle + MathHelper.ToRadians(90), new Vector2(thrust.Width / 2, thrust.Height / 2), SpriteEffects.None, 0);

                    if (Boosting)
                    {
                        Vector2 direction = new Vector2((float)Math.Cos(RenderAngle), (float)Math.Sin(RenderAngle));
                        Color[] colors = new Color[] { Color.Red, Color.Orange };
                        Random random = new Random();
                        
                        for (int i = 0; i < 3; i++)
                        {
                            Particle particle = new Particle();
                            particle.Life = random.Next(500, 1250);
                            particle.Color = colors[random.Next(colors.Length)];
                            particle.Position = RenderPosition - (defaultShip.Height / 2) * direction + new Vector2(random.Next(-2, 2), random.Next(-2, 2));
                            particle.Velocity = Velocity - direction * 0.05f + new Vector2(((float)random.NextDouble() - 0.5f) * 2.0f, ((float)random.NextDouble() - 0.5f) * 2.0f);
                            Particles.Add(particle);
                        }
                    }
                }
                else if (!MovingForward && MovingBackward)
                {
                    sprites.Draw(GameContent.Texture("ships\\default\\backward"), new Rectangle((int)RenderPosition.X, (int)RenderPosition.Y, thrust.Width, thrust.Height), null, Color.White, RenderAngle + MathHelper.ToRadians(90), new Vector2(thrust.Width / 2, thrust.Height / 2), SpriteEffects.None, 0);
                }

                if (TurningLeft && !TurningRight)
                {
                    sprites.Draw(GameContent.Texture("ships\\default\\left"), new Rectangle((int)RenderPosition.X, (int)RenderPosition.Y, thrust.Width, thrust.Height), null, Color.White, RenderAngle + MathHelper.ToRadians(90), new Vector2(thrust.Width / 2, thrust.Height / 2), SpriteEffects.None, 0);
                }
                else if (!TurningLeft && TurningRight)
                {
                    sprites.Draw(GameContent.Texture("ships\\default\\right"), new Rectangle((int)RenderPosition.X, (int)RenderPosition.Y, thrust.Width, thrust.Height), null, Color.White, RenderAngle + MathHelper.ToRadians(90), new Vector2(thrust.Width / 2, thrust.Height / 2), SpriteEffects.None, 0);
                }
            }
        }

        public override void Update()
        {
            float posLerpFactor = 0.25f; // TO DO: Make this congruent with the player's ping?
            float angLerpFactor = 0.2f;

            if (!IsLocalPlayer)
            {
                RenderPosition = new Vector2((float)Math.Round(MathHelper.Lerp(RenderPosition.X, Position.X, posLerpFactor)), (float)Math.Round(MathHelper.Lerp(RenderPosition.Y, Position.Y, posLerpFactor)));
                RenderAngle = MathHelper.Lerp(RenderAngle, Angle, angLerpFactor);
            }
            else
            {
                RenderPosition = new Vector2((float)Math.Round(Position.X), (float)Math.Round(Position.Y));
                RenderAngle = Angle;
            }
        }
    }
}
