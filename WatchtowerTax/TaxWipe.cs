using Celeste;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;

namespace WatchtowerTax
{
    public class TaxWipe : ScreenWipe
    {
        private const int wordWidth = 600;
        private const int wordHeight = 600;
        private const int wordSpacing = 40;
        private const float finalSizeMultiplier = 1.5f;

        private readonly Vector2 center = new Vector2( 1920 / 2, 1080 / 2 );

        private const int height = 1080;

        private VertexPositionColor[] verts = new VertexPositionColor[ 42 ];
        private Vector2[] refVerts = new Vector2[ 26 ];
        private int[] vertIndeces;
        private bool hasDrawn;

        public static readonly BlendState SubtractBlendmode = new BlendState
        {
            ColorSourceBlend = Blend.One,
            ColorDestinationBlend = Blend.One,
            ColorBlendFunction = BlendFunction.ReverseSubtract,
            AlphaSourceBlend = Blend.One,
            AlphaDestinationBlend = Blend.One,
            AlphaBlendFunction = BlendFunction.Add
        };

        public TaxWipe( Scene scene, bool wipeIn, Action onComplete = null ) : base( scene, wipeIn, onComplete )
        {
            // T
            refVerts[ 0 ] = new Vector2( 0, 0 );
            refVerts[ 1 ] = new Vector2( wordWidth, 0 );
            refVerts[ 2 ] = new Vector2( 0, wordHeight / 6 );
            refVerts[ 3 ] = new Vector2( wordWidth, wordHeight / 6 );
            refVerts[ 4 ] = new Vector2( wordWidth / 3, wordHeight / 6 );
            refVerts[ 5 ] = new Vector2( 2 * wordWidth / 3, wordHeight / 6 );
            refVerts[ 6 ] = new Vector2( wordWidth / 3, wordHeight );
            refVerts[ 7 ] = new Vector2( 2 * wordWidth / 3, wordHeight );
            // A
            refVerts[ 8 ] = new Vector2( wordWidth + wordSpacing + wordWidth / 3, 0 );
            refVerts[ 9 ] = new Vector2( wordWidth + wordSpacing + 2 * wordWidth / 3, 0 );
            refVerts[ 10 ] = new Vector2( wordWidth + wordSpacing + wordWidth / 3, 3 * wordHeight / 6 );
            refVerts[ 11 ] = new Vector2( wordWidth + wordSpacing + 2 * wordWidth / 3, 3 * wordHeight / 6 );
            refVerts[ 12 ] = new Vector2( wordWidth + wordSpacing + wordWidth / 3, 4 * wordHeight / 6 );
            refVerts[ 13 ] = new Vector2( wordWidth + wordSpacing + 2 * wordWidth / 3, 4 * wordHeight / 6 );
            refVerts[ 14 ] = new Vector2( wordWidth + wordSpacing, wordHeight );
            refVerts[ 15 ] = new Vector2( wordWidth + wordSpacing + wordWidth / 3, wordHeight );
            refVerts[ 16 ] = new Vector2( wordWidth + wordSpacing + 2 * wordWidth / 3, wordHeight );
            refVerts[ 17 ] = new Vector2( wordWidth + wordSpacing + wordWidth, wordHeight );
            // X
            refVerts[ 18 ] = new Vector2( 2*(wordWidth + wordSpacing), 0 );
            refVerts[ 19 ] = new Vector2( 2*(wordWidth + wordSpacing) + wordWidth / 4, 0 );
            refVerts[ 20 ] = new Vector2( 2*(wordWidth + wordSpacing) + 3 * wordWidth / 4, 0 );
            refVerts[ 21 ] = new Vector2( 2*(wordWidth + wordSpacing) + wordWidth, 0 );
            refVerts[ 22 ] = new Vector2( 2 * ( wordWidth + wordSpacing ), wordHeight );
            refVerts[ 23 ] = new Vector2( 2 * ( wordWidth + wordSpacing ) + wordWidth / 4, wordHeight );
            refVerts[ 24 ] = new Vector2( 2 * ( wordWidth + wordSpacing ) + 3 * wordWidth / 4, wordHeight );
            refVerts[ 25 ] = new Vector2( 2 * ( wordWidth + wordSpacing ) + wordWidth, wordHeight );

            for ( int i = 0; i < refVerts.Length; i++ )
            {
                refVerts[ i ] -= center - new Vector2( 0, ( height - wordHeight ) / 2 );
            }

            vertIndeces = 
                new int[] { 
                    0, 1, 2, // 1
                    1, 3, 2, // 2
                    4, 5, 6, // 3
                    5, 7, 6, // 4
                    8, 15, 14, // 5
                    8, 9, 15, // 6
                    8, 9, 16, // 7
                    9, 17, 16, // 8
                    10, 11, 12, // 9
                    11, 13, 12, // 10
                    18, 25, 24, // 11
                    18, 19, 25, // 12
                    20, 23, 22, // 13
                    20, 21, 23, // 14
            };

            for ( int j = 0; j < verts.Length; j++ )
            {
                verts[ j ].Color = Color.Black;
            }

            if ( WipeIn )
            {
                WatchtowerTaxModule.Instance.doTaxWipe = false;
            }
        }

        public override void BeforeRender( Scene scene )
        {
            hasDrawn = true;
            Engine.Graphics.GraphicsDevice.SetRenderTarget( Celeste.Celeste.WipeTarget );
            Engine.Graphics.GraphicsDevice.Clear( Color.White );
            if ( Percent > 0.8f )
            {
                float rectPercent = Calc.Map( Percent, 0.8f, 1f );
                Draw.SpriteBatch.Begin();
                Draw.Rect( ( 1920f - rectPercent * 1922f ) * 0.5f, ( 1080f - rectPercent * 1082f ) * 0.5f, rectPercent * 1922f, rectPercent * 1082f, WipeIn ? Color.Black : Color.White );
                Draw.SpriteBatch.End();
            }
            float sizeMultiplier = Ease.CubeIn( WipeIn ? Percent : 1 - Percent );

            for ( int i = 0; i < verts.Length; i++ )
            {
                verts[ i ].Position = new Vector3( refVerts[ vertIndeces[ i ] ] * finalSizeMultiplier * sizeMultiplier + center, 0f );
            }

            GFX.DrawVertices( Matrix.Identity, verts, verts.Length );
        }

        public override void Render( Scene scene )
        {
            Draw.SpriteBatch.Begin( SpriteSortMode.Deferred, SubtractBlendmode, SamplerState.LinearClamp, null, null, null, Engine.ScreenMatrix );
            if ( ( WipeIn && Percent <= 0.01f ) || ( !WipeIn && Percent >= 0.99f ) )
            {
                Draw.Rect( -1f, -1f, 1922f, 1082f, Color.White );
            }
            else if ( hasDrawn )
            {
                Draw.SpriteBatch.Draw( Celeste.Celeste.WipeTarget, new Vector2( -1f, -1f ), Color.White );
            }
            Draw.SpriteBatch.End();
        }
    }
}
