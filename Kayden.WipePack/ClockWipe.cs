using Celeste;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;

namespace Kayden.WipePack
{
    public class ClockWipe : ScreenWipe
    {
        private const int bigDiameter = 2203;
        private const int hourLength = 500;
        private const int minuteLength = 1000;
        private const int hourWidth = 60;
        private const int minuteWidth = 40;

        private const int clockSpeedMultiplier = 5000;

        private const int circleSegments = 32;

        private static readonly Color clockworkColor = Color.White;
        private static readonly Color cutoutColor = Color.Black;

        private static DateTime time;

        private static readonly Vector2 O = new Vector2( 1982 / 2, 1082 / 2 );

        private VertexPositionColor[] cutoutVerts;
        private VertexPositionColor[] clockworkVerts;

        public ClockWipe( Scene scene, bool wipeIn, Action onComplete = null ) : base( scene, wipeIn, onComplete )
        {
            if ( time == null )
            {
                time = DateTime.Now;
            }

            cutoutVerts = new VertexPositionColor[ circleSegments * 3 ];

            int triangleCount = circleSegments / 2 + 4;

            clockworkVerts = new VertexPositionColor[ triangleCount * 3 ];

            for ( int i = 0; i < cutoutVerts.Length; i++ )
            {
                cutoutVerts[ i ].Color = cutoutColor;
            }

            for ( int i = 0; i < clockworkVerts.Length; i++ )
            {
                clockworkVerts[ i ].Color = clockworkColor;
            }
        }

        public override void BeforeRender( Scene scene )
        {
            Engine.Graphics.GraphicsDevice.SetRenderTarget( Celeste.Celeste.WipeTarget );
            Engine.Graphics.GraphicsDevice.Clear( Color.White );

            time = time.AddSeconds( Engine.RawDeltaTime * clockSpeedMultiplier );

            PrepareCutoutVerts();
            PrepareClockworkVerts();
        }

        public override void Render( Scene scene )
        {
            Draw.SpriteBatch.Begin( SpriteSortMode.Deferred, WipePackModule.SubtractBlendmode, SamplerState.LinearClamp, null, null, null, Engine.ScreenMatrix );
            Draw.SpriteBatch.Draw( Celeste.Celeste.WipeTarget, new Vector2( -1f, -1f ), Color.White );
            Draw.SpriteBatch.End();
        }

        private void PrepareCutoutVerts()
        {
            Engine.Graphics.GraphicsDevice.SetRenderTarget( Celeste.Celeste.WipeTarget );

            float sizeMultiplier = Ease.CubeIn( WipeIn ? Percent : 1 - Percent );

            int i = 0;
            int segmentIndex = 0;
            float segmentAngle = (float)Math.PI * 2.0f / circleSegments;

            while ( i < cutoutVerts.Length )
            {
                cutoutVerts[ i ].Position = new Vector3( O, 0 );
                cutoutVerts[ i + 1 ].Position = new Vector3( 
                    O + new Vector2( 
                        (float)Math.Sin( segmentAngle * segmentIndex ) * bigDiameter * sizeMultiplier, 
                        (float)Math.Cos( segmentAngle * segmentIndex ) * bigDiameter * sizeMultiplier 
                    ), 
                    0 
                );
                cutoutVerts[ i + 2 ].Position = new Vector3(
                    O + new Vector2(
                        (float)Math.Sin( segmentAngle * ( segmentIndex + 1 ) ) * bigDiameter * sizeMultiplier,
                        (float)Math.Cos( segmentAngle * ( segmentIndex + 1 ) ) * bigDiameter * sizeMultiplier
                    ),
                    0
                );
                i += 3;
                segmentIndex++;
            }

            GFX.DrawVertices( Matrix.Identity, cutoutVerts, cutoutVerts.Length );
        }

        private void PrepareClockworkVerts()
        {
            Engine.Graphics.GraphicsDevice.SetRenderTarget( Celeste.Celeste.WipeTarget );

            float sizeMultiplier = Ease.CubeIn( WipeIn ? Percent : 1 - Percent );
            float lengthMultiplier = 1.0f;

            if ( Percent > 0.8f )
            {
                lengthMultiplier = 1.0f - Calc.Map( Percent, 0.8f, 1f );
            }

            int i = 0;

            Vector2[] minuteVerts = GetArmVerts( minuteWidth * sizeMultiplier, minuteLength * sizeMultiplier * lengthMultiplier, time.Minute * (float)Math.PI * 2.0f / 60.0f );

            for ( int j = 0; j < minuteVerts.Length; j++ )
            {
                clockworkVerts[ i ].Position = new Vector3( minuteVerts[ j ], 0 );
                i++;
            }

            Vector2[] hourVerts = GetArmVerts( hourWidth * sizeMultiplier, hourLength * sizeMultiplier * lengthMultiplier, time.Hour * (float)Math.PI * 2.0f / 12.0f + time.Minute * (float)Math.PI * 2.0f / 60.0f / 12.0f, true );

            for ( int j = 0; j < minuteVerts.Length; j++ )
            {
                clockworkVerts[ i ].Position = new Vector3( hourVerts[ j ], 0 );
                i++;
            }

            GFX.DrawVertices( Matrix.Identity, clockworkVerts, clockworkVerts.Length );
        }

        private Vector2[] GetArmVerts( float width, float length, float angleRads, bool withHalfCircle = false )
        {
            float theta = angleRads - (float)Math.PI / 2;
            float halfWidth = width * 0.5f;
            Vector2 OA = new Vector2( halfWidth * (float)Math.Sin( theta ), halfWidth * -(float)Math.Cos( theta ) );
            Vector2 AB = OA * -2;
            Vector2 AC = new Vector2( length * (float)Math.Sin( angleRads ), length * -(float)Math.Cos( angleRads ) );

            Vector2 A = O + OA;
            Vector2 B = A + AB;
            Vector2 C = A + AC;
            Vector2 D = C + AB;

            if ( !withHalfCircle )
            {
                return new Vector2[] { A, C, B, B, C, D };
            }
            else
            {
                int count = ( circleSegments / 2 ) * 3 + 6;

                Vector2[] result = new Vector2[count];

                result[ 0 ] = A;
                result[ 1 ] = C;
                result[ 2 ] = B;
                result[ 3 ] = B;
                result[ 4 ] = C;
                result[ 5 ] = D;

                float segmentAngle = (float)Math.PI / ( circleSegments / 2 );
                int i = 6;
                int segmentOffset = 0;

                halfWidth *= 10;

                while ( i < count )
                {
                    float segmentAngleOffset = theta - segmentOffset* segmentAngle;

                    result[ i ] = O;
                    result[ i + 1 ] = O + new Vector2( halfWidth * (float)Math.Sin( segmentAngleOffset ), halfWidth * -(float)Math.Cos( segmentAngleOffset ) );
                    result[ i + 2 ] = O + new Vector2( halfWidth * (float)Math.Sin( segmentAngleOffset + segmentAngle ), halfWidth * -(float)Math.Cos( segmentAngleOffset + segmentAngle ) );
                    i += 3;
                    segmentOffset++;
                }

                return result;
            }
        }
    }
}
