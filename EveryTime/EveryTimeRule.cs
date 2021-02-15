using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace EveryTime
{
    public class EveryTimeRule
    {
        public enum Cause
        {
            Jump,
            Dash,
            Die,
            ScreenChange,
            Crouch,
            Refill,
            Feather,
            Bumper,
            Spring,
            DreamDash,
            Land,
            CollectBerry,
            CollectKey,
            UseKey,
            NewRoom
        }

        public enum Effect
        {
            SpeedUp,
            SpeedDown,
            ExtraHair,
            Hiccup,
            BadelineChaser,
            Oshiro,
            Anxiety
        }

        public HashSet<Cause> Causes = new HashSet<Cause>();
        public Dictionary<Effect, string> Effects = new Dictionary<Effect, string>();

        internal void Apply( Scene scene )
        {
            if ( Effects.Keys.Contains( Effect.SpeedUp ) )
            {
                EveryTimeModule.Session.TimeDilation += Math.Max( 0, float.Parse(Effects[ Effect.SpeedUp ]) );
            }

            if ( Effects.Keys.Contains( Effect.SpeedDown ) )
            {
                EveryTimeModule.Session.TimeDilation *= 1 - Math.Max( 0, float.Parse(Effects[ Effect.SpeedDown ] ) );
            }

            if ( Effects.Keys.Contains( Effect.ExtraHair ) )
            {
                int value = int.Parse( Effects[ Effect.ExtraHair ] );
                Player player = scene.Tracker.GetEntity<Player>();
                if ( player != null && player.Hair != null )
                {
                    player.Sprite.HairCount += value;
                    typeof( Player ).GetField( "startHairCount", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance ).SetValue( player, player.Sprite.HairCount );
                }
                EveryTimeModule.Session.ExtraHair += value;
            }

            if ( Effects.Keys.Contains( Effect.Hiccup ) )
            {
                Player player = scene.Tracker.GetEntity<Player>();
                if ( player != null && !player.Dead )
                {
                    player.HiccupJump();
                }
            }

            if ( Effects.Keys.Contains( Effect.BadelineChaser ) )
            {
                Player player = scene.Tracker.GetEntity<Player>();
                int value = int.Parse( Effects[ Effect.BadelineChaser ] );
                EveryTimeModule.Session.ChaserCount = Math.Max( value + EveryTimeModule.Session.ChaserCount, 0 ); ;
                if ( player != null )
                {
                    if ( value > 0 )
                    {
                        int chaserCount = 0;
                        List<Entity> existingBadelines = scene.Tracker.GetEntities<Celeste.BadelineOldsite>();
                        if ( existingBadelines != null )
                        {
                            chaserCount += existingBadelines.Count;
                        }
                        scene.Add( new Celeste.BadelineOldsite( player.Position + Vector2.UnitY * 32, chaserCount++ ) );
                    }
                    else if ( value < 0 && EveryTimeModule.Session.ChaserCount > 0 )
                    {
                        List<Entity> existingBadelines = scene.Tracker.GetEntities<Celeste.BadelineOldsite>();
                        if ( existingBadelines != null )
                        {
                            int highestIndex = existingBadelines.Count - 1;
                            foreach ( BadelineOldsite chaser in existingBadelines )
                            {
                                var indexField = typeof( BadelineOldsite ).GetField( "index", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance );
                                int index = (int) indexField.GetValue( chaser );
                                if ( index == highestIndex )
                                {
                                    chaser.RemoveSelf();
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            if ( Effects.Keys.Contains( Effect.Oshiro ) )
            {
                int value = int.Parse( Effects[ Effect.Oshiro ] );
                Player player = scene.Tracker.GetEntity<Player>();
                EveryTimeModule.Session.OshiroCount = Math.Max( value + EveryTimeModule.Session.OshiroCount, 0 );
                if ( player != null )
                {
                    if ( value > 0 )
                    {
                        player.Add( new Coroutine( EveryTimeModule.OshiroSpawnRoutine( player, value ) ) );
                    }
                    else if ( value < 0 && EveryTimeModule.Session.OshiroCount > 0 )
                    {
                        AngryOshiro oshiro = scene.Tracker.GetNearestEntity<AngryOshiro>( player.Position );
                        oshiro.RemoveSelf();
                    }
                }
            }

            if ( Effects.Keys.Contains( Effect.Anxiety ) )
            {
                EveryTimeModule.Session.AnxietyBonus = Math.Max( 0, EveryTimeModule.Session.AnxietyBonus + float.Parse( Effects[ Effect.Anxiety ] ) );
            }
        }
    }
}
