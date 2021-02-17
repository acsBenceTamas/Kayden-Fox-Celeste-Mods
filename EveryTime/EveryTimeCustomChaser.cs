using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using MonoMod.Utils;
using System.Collections;

namespace EveryTime
{
    [Tracked( false )]
    public class EveryTimeCustomChaser : BadelineOldsite
    {
        public EveryTimeCustomChaser( Vector2 position, int index ) : base( position, index )
        {
        }

        public override void Added( Scene scene )
        {
            Action<Scene> baseAdded = typeof( Entity ).GetMethod( "Added" ).CreateDelegate<Action<Scene>>( this );
            baseAdded( scene );
            Add( new Coroutine( StartChasingRoutine( scene as Level ) ) );
        }

        public IEnumerator KillSelfRoutine()
        {
            typeof( BadelineOldsite ).GetField( "following", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance ).SetValue( this, false );
            typeof( BadelineOldsite ).GetField( "ignorePlayerAnim", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance ).SetValue( this, true );
            Sprite.Play( "laugh" );
            Sprite.Scale.X = 1f;
            Collidable = false;
            yield return 1f;
            Audio.Play( "event:/char/badeline/disappear", Position );
            SceneAs<Level>().Displacement.AddBurst( Center, 0.5f, 24f, 96f, 0.4f );
            SceneAs<Level>().Particles.Emit( P_Vanish, 12, Center, Vector2.One * 6f );
            RemoveSelf();
            EveryTimeModule.Session.SpawnedBadelineChasers.Remove( this );
        }
    }
}
