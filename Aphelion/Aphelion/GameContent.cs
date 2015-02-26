using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;

namespace Aphelion
{
    public static class GameContent
    {
        static Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
        static Dictionary<string, SoundEffect> sounds = new Dictionary<string, SoundEffect>();
        static Dictionary<string, Song> songs = new Dictionary<string, Song>();
        static ContentManager content;

        public static void SetContentManager(ContentManager contentManager)
        {
            content = contentManager;
        }

        public static Texture2D Texture(string path)
        {
            path = @"Textures\" + path;

            //if (!path.EndsWith(".png"))
            //{
            //    path += ".png";
            //}

            if (textures.ContainsKey(path))
            {
                return textures[path];
            }
            
            Texture2D texture = content.Load<Texture2D>(path);
            textures[path] = texture;

            return texture;
        }

        public static SoundEffect Sound(string path)
        {
            path = @"Sounds\" + path;

            //if (!path.EndsWith(".wav"))
            //{
            //    path += ".wav";
            //}

            if (sounds.ContainsKey(path))
            {
                return sounds[path];
            }

            SoundEffect sound = content.Load<SoundEffect>(path);
            sounds[path] = sound;

            return sound;
        }
    }
}
