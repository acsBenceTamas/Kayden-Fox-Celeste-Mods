using Celeste.Mod;
using Microsoft.Xna.Framework.Graphics;

namespace Kayden.WipePack
{
    class WipePackModule : EverestModule
    {
        public static readonly BlendState SubtractBlendmode = new BlendState
        {
            ColorSourceBlend = Blend.One,
            ColorDestinationBlend = Blend.One,
            ColorBlendFunction = BlendFunction.ReverseSubtract,
            AlphaSourceBlend = Blend.One,
            AlphaDestinationBlend = Blend.One,
            AlphaBlendFunction = BlendFunction.Add
        };

        public override void Load()
        {
        }

        public override void Unload()
        {
        }
    }
}
