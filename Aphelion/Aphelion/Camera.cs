using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Aphelion
{
    public class Camera
    {
        public Vector2 Size;
        public float Zoom;
        public float Rotation;
        public Vector2 Position;
        public Matrix Transform;

        public Camera()
        {
            Size = new Vector2(1000, 600);
            Zoom = 1.0f;
            Rotation = 0.0f;
            Position = new Vector2(Size.X / 2, Size.Y / 2);
        }

        public Matrix GetTransformation()
        {
            Matrix position = Matrix.CreateTranslation(new Vector3(-Position.X, -Position.Y, 0));
            Matrix rotation = Matrix.CreateRotationZ(Rotation);
            Matrix scale = Matrix.CreateScale(new Vector3(Zoom, Zoom, 1));
            Matrix translation = Matrix.CreateTranslation(Size.X / 2, Size.Y / 2, 0);

            return position * rotation * scale * translation;
        }
    }
}
