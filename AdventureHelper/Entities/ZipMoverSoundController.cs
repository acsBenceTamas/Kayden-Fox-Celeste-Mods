using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.AdventureHelper.Entities
{
    class ZipMoverSoundController
    {
        public static Dictionary<string, SoundSource> activeSounds = new Dictionary<string, SoundSource>();

        public static void PlaySound(string colorCode, SoundType type, Solid block)
        {
            string name = $"{colorCode}-{type}";
            if (!activeSounds.ContainsKey(name))
            {
                var player = block.Scene.Tracker.Entities[typeof(Player)].FirstOrDefault();
                if (player != null)
                {
                    Vector2 position = player.Position;
                    SoundSource source = new SoundSource();
                    source.Position = position;
                    activeSounds.Add(name, source);
                    source.Play("event:/game/01_forsaken_city/zip_mover", null, 0f);
                }
            }
            else if (type == SoundType.Returning)
            {
                SoundSource source = activeSounds[name];
                if (!source.Playing)
                {
                    source.Play("event:/game/01_forsaken_city/zip_mover", null, 0f);
                }
            }
        }

        public static void StopSound(string colorCode, SoundType type)
        {
            string name = $"{colorCode}-{type}";
            if (activeSounds.ContainsKey(name))
            {
                var source = activeSounds[name];
                activeSounds.Remove(name);
                source.Stop();
            }
        }

        public enum SoundType
        {
            Returning,
            NonReturning
        }
    }
}
