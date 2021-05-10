using Celeste;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;

namespace Kayden.WipePack
{
    public abstract class ImageSequenceWipe : ScreenWipe
    {
        public ImageSequenceWipe( Scene scene, bool wipeIn, Action onComplete = null ) : base( scene, wipeIn, onComplete ) { }

        public abstract Texture2D GetTextureForCurrentFrame();

        public override void BeforeRender( Scene scene )
        {
            Engine.Graphics.GraphicsDevice.SetRenderTarget( Celeste.Celeste.WipeTarget );
            Engine.Graphics.GraphicsDevice.Clear( Color.White );
            Draw.SpriteBatch.Begin();
            Draw.SpriteBatch.Draw( GetTextureForCurrentFrame(), new Vector2( 0, 0 ), Color.White );
            Draw.SpriteBatch.End();
        }

        public override void Render( Scene scene )
        {
            Draw.SpriteBatch.Begin( SpriteSortMode.Immediate, WipePackModule.SubtractBlendmode, SamplerState.LinearClamp, null, null, null, Engine.ScreenMatrix ); // Setup so the wipe works as a wipe
            Draw.SpriteBatch.Draw( Celeste.Celeste.WipeTarget, new Vector2( -1f, -1f ), Color.White ); // We draw the sprite at -1,-1 for extra safety. Image size should be 1922x1082 for this reason
            Draw.SpriteBatch.End(); // Stop drawing onto the wipe
        }
    }
    public class BeeImageSequenceWipe : ImageSequenceWipe
    {
        private readonly int sequenceInStart = 0;
        private readonly int sequenceInEnd = 56;
        private readonly int sequenceOutStart = 72;
        private readonly int sequenceOutEnd = 120;
        private readonly string spritePathRoot = "Wipes/BeeWipe/Hexagons 2_";
        public BeeImageSequenceWipe( Scene scene, bool wipeIn, Action onComplete = null ) : base( scene, wipeIn, onComplete ) { }

        public override Texture2D GetTextureForCurrentFrame()
        {
            int range = WipeIn ? ( sequenceInEnd - sequenceInStart ) : ( sequenceOutEnd - sequenceOutStart );
            int frameNumber = (int)( Percent * range ) + ( WipeIn ? sequenceInStart : sequenceOutStart );
            string targetTexture = spritePathRoot + ( frameNumber == 0 ? "00000" : frameNumber.ToString( "D5" ) );
            MTexture texture = GFX.Gui[ targetTexture ];
            return texture.Texture.Texture_Safe;
        }
    }
}
