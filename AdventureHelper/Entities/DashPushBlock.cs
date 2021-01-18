using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.AdventureHelper.Entities
{

    [ CustomEntity( "AdventureHelper/DashPushBlock" )]
    class DashPushBlock : Solid
    {
        private const float _flashTime = 0.5f;
        protected MTexture[,] _body;
        protected MTexture[,] _flash;
        protected float _flashPercent = 0.0f;
        protected SineWave _sine;

        public DashPushBlock( EntityData data, Vector2 offset ) : this( data.Position + offset, data.Width, data.Height )
        { }

        public DashPushBlock( Vector2 position, float width, float height ) : base( position, width, height, true )
        {
            _body = new MTexture[ 3, 3 ];
            _flash = new MTexture[ 3, 3 ];

            for ( int i = 0; i < 3; i++ )
            {
                for ( int j = 0; j < 3; j++ )
                {
                    _body[ i, j ] = GFX.Game[ "objects/AdventureHelper/dashpushblock/Body" ].GetSubtexture( i * 8, j * 8, 8, 8, null );
                    _flash[ i, j ] = GFX.Game[ "objects/AdventureHelper/dashpushblock/Cracks" ].GetSubtexture( i * 8, j * 8, 8, 8, null );
                }
            }

            Add( _sine = new SineWave( 0.6f, 0f ) );
            _sine.Randomize();

            OnDashCollide = OnDashed;
        }

        public override void Update()
        {
            base.Update();

            if ( _flashPercent > 0 )
            {
                _flashPercent -= Engine.DeltaTime / _flashTime;

                if ( _flashPercent < 0.0f )
                {
                    _flashPercent = 0.0f;
                }
            }
        }

        public override void Render()
        {
            base.Render();

            //float flashStrength = Math.Min( 0.2f + 0.2f * _sine.Value + _flashPercent, 1.0f );
            //Color flashColor = new Color( Color.White, flashStrength );

            int textureX = 0;
            while ( textureX < Width / 8f )
            {
                int textureY = 0;
                while ( textureY < Height / 8f )
                {
                    int tileX = ( textureX == 0 ) ? 0 : ( ( textureX == Width / 8f - 1f ) ? 2 : 1 );
                    int tileY = ( textureY == 0 ) ? 0 : ( ( textureY == Height / 8f - 1f ) ? 2 : 1 );

                    _body[ tileX, tileY ].Draw( new Vector2( X + ( textureX * 8 ), Y + ( textureY * 8 ) ) );
                    //_flash[ tileX, tileY ].Draw( new Vector2( X + ( textureX * 8 ), Y + ( textureY * 8 ) ), new Vector2( Width, Height ), flashColor );

                    textureY++;
                }
                textureX++;
            }
        }

        protected DashCollisionResults OnDashed( Player player, Vector2 direction )
        {
            if ( _flashPercent <= 0.0f )
            {
                Audio.Play( "event:/game/general/fallblock_shake", Center );

                MoveHCollideSolidsAndBounds( Scene as Level, direction.X * Width, false, null );
                MoveVCollideSolidsAndBounds( Scene as Level, direction.Y * Height, false, null );

                _flashPercent = 1.0f;

                return DashCollisionResults.Rebound;
            }

            return DashCollisionResults.NormalCollision;
        }
    }
}
