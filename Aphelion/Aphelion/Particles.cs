using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Aphelion
{
    public struct Particle
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public int Life;
        public Color Color;
    }

    public static class Particles // TO DO: Fix the weirdness when particles die
    {
        private static List<Particle> particles = new List<Particle>();

        public static void Add(Particle particle)
        {
            particles.Add(particle);
        }

        public static void Update(GameTime time)
        {
            for (int i = 0; i < particles.Count; i++)
            {
                Particle particle = particles[i];

                particle.Position += particle.Velocity;
                particle.Life -= time.ElapsedGameTime.Milliseconds;

                particles[i] = particle;

                if (particle.Life <= 0)
                {
                    particles.Remove(particle);
                }
            }
        }

        public static void Draw(SpriteBatch sprites)
        {
            foreach (Particle particle in particles)
            {
                if (particle.Life > 0) // I don't even know anymore
                {
                    sprites.Draw(GameContent.Texture("pixel"), particle.Position, particle.Color);
                }
            }
        }
    }
}
