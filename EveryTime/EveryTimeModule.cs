using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Collections;
using System.Collections.Generic;

namespace EveryTime
{
    public class EveryTimeModule : EverestModule
    {
        public static EveryTimeModule Instance;
        public override Type SettingsType => typeof( EveryTimeModuleSettings );
        public static EveryTimeModuleSettings Settings => (EveryTimeModuleSettings)Instance._Settings;
        public override Type SessionType => typeof( EveryTimeModuleSession );
        public static EveryTimeModuleSession Session => (EveryTimeModuleSession)Instance._Session;
        private static ILHook ILPlayer_orig_Update;

        readonly ILContext.Manipulator ILEngineUpdate = ( il ) =>
        {
            ILCursor c = new ILCursor( il );
            if (c.TryGotoNext(
                MoveType.After,
                x => x.MatchCall<Engine>( "get_RawDeltaTime" ),
                x => x.MatchLdsfld<Engine>( "TimeRate" ),
                x => x.MatchMul(),
                x => x.MatchLdsfld<Engine>( "TimeRateB" ),
                x => x.MatchMul()
                ) )
            {
                c.EmitDelegate<Func<float>>( () => ( Session != null && Settings.Enabled ) ? Session.TimeDilation : 1f );
                c.Emit( OpCodes.Mul );
            }
        };

        public EveryTimeModule()
        {
            Instance = this;
        }

        public override void LoadSettings()
        {
            base.LoadSettings();
        }

        public override void Load()
        {
            //if ( Settings.Rules.Count == 0 )
            //{
            //    EveryTimeRule exampleRule = new EveryTimeRule();
            //    exampleRule.Causes.Add( EveryTimeRule.Cause.Die );
            //    exampleRule.Effects.Add( EveryTimeRule.Effect.Anxiety, 0.05f.ToString() );
            //    Settings.Rules.Add( exampleRule );
            //    EveryTimeRule exampleRule2 = new EveryTimeRule();
            //    exampleRule2.Causes.Add( EveryTimeRule.Cause.Dash );
            //    exampleRule2.Effects.Add( EveryTimeRule.Effect.ExtraHair, 1.ToString() );
            //    Settings.Rules.Add( exampleRule2 );
            //    EveryTimeRule exampleRule3 = new EveryTimeRule();
            //    exampleRule3.Causes.Add( EveryTimeRule.Cause.Jump );
            //    exampleRule3.Effects.Add( EveryTimeRule.Effect.SpeedUp, 0.01f.ToString() );
            //    Settings.Rules.Add( exampleRule3 );
            //    SaveSettings();
            //}

            On.Celeste.Player.Jump += OnPlayerJump;
            On.Celeste.Player.DashBegin += OnPlayerDashBegin;
            On.Celeste.Player.Die += OnPlayerDie;
            On.Celeste.PlayerSprite.ctor += OnPlayerSprite_ctor;
            On.Celeste.Distort.Render += OnDistortRender;
            On.Celeste.Player.Update += OnPlayerUpdate;
            On.Celeste.Level.LoadLevel += OnLevelLoadLevel;
            On.Celeste.Strawberry.OnCollect += OnStrawberryOnCollect;
            On.Celeste.Key.OnPlayer += OnKeyOnPlayer;
            On.Celeste.Key.RegisterUsed += OnKeyRegisterUsed;

            IL.Monocle.Engine.Update += ILEngineUpdate;
            ILPlayer_orig_Update = new ILHook( typeof( Celeste.Player ).GetMethod( "orig_Update", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance ), ILPlayerUpdate );
        }

        public override void Unload()
        {
            On.Celeste.Player.Jump -= OnPlayerJump;
            On.Celeste.Player.DashBegin -= OnPlayerDashBegin;
            On.Celeste.Player.Die -= OnPlayerDie;
            On.Celeste.PlayerSprite.ctor -= OnPlayerSprite_ctor;
            On.Celeste.Distort.Render -= OnDistortRender;
            On.Celeste.Player.Update -= OnPlayerUpdate;
            On.Celeste.Level.LoadLevel -= OnLevelLoadLevel;
            On.Celeste.Strawberry.OnCollect -= OnStrawberryOnCollect;
            On.Celeste.Key.OnPlayer -= OnKeyOnPlayer;
            On.Celeste.Key.RegisterUsed -= OnKeyRegisterUsed;
            On.Celeste.Player.Duck -= OnPlayerDuck;

            IL.Monocle.Engine.Update -= ILEngineUpdate;
            ILPlayer_orig_Update.Dispose();
            ILPlayer_orig_Update = null;
        }

        private void OnPlayerJump( On.Celeste.Player.orig_Jump orig, Celeste.Player self, bool particles, bool playSfx )
        {
            if ( Settings.Enabled )
            {
                foreach ( EveryTimeRule rule in Settings.Rules )
                {
                    if ( rule.Causes.Contains( EveryTimeRule.Cause.Jump ) )
                    {
                        rule.Apply( self.Scene );
                    }
                }
            }
            orig( self, particles, playSfx );
        }

        private void OnPlayerDashBegin( On.Celeste.Player.orig_DashBegin orig, Celeste.Player self )
        {
            if ( Settings.Enabled && self.InControl )
            {
                foreach ( EveryTimeRule rule in Settings.Rules )
                {
                    if ( rule.Causes.Contains( EveryTimeRule.Cause.Dash ) )
                    {
                        rule.Apply( self.Scene );
                    }
                }
            }
            orig( self );
        }

        private Celeste.PlayerDeadBody OnPlayerDie( On.Celeste.Player.orig_Die orig, Celeste.Player self, Vector2 direction, bool evenIfInvincible, bool registerDeathInStats )
        {
            if ( Settings.Enabled )
            {
                foreach ( EveryTimeRule rule in Settings.Rules )
                {
                    if ( rule.Causes.Contains( EveryTimeRule.Cause.Die ) )
                    {
                        rule.Apply( self.Scene );
                    }
                }
            }
            return orig( self, direction, evenIfInvincible, registerDeathInStats );
        }

        private void OnPlayerSprite_ctor( On.Celeste.PlayerSprite.orig_ctor orig, Celeste.PlayerSprite self, PlayerSpriteMode mode )
        {
            orig( self, mode );
            self.HairCount += Session.ExtraHair;
        }

        private void OnDistortRender( On.Celeste.Distort.orig_Render orig, Texture2D source, Texture2D map, bool hasDistortion )
        {
            if ( !Celeste.Settings.Instance.DisableFlashes )
            {
                var anxietyField = typeof( Celeste.Distort ).GetField( "anxiety", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static );
                float anxiety = (float)anxietyField.GetValue( null );
                float anxietyBonus = Math.Max( 0, Session.AnxietyBonus * 0.5f + Session.AnxietyBonus * 0.5f * Session.AnxietyStutter );
                float anxietyForUse = anxiety + anxietyBonus;
                Celeste.GFX.FxDistort.Parameters[ "anxiety" ].SetValue( anxietyForUse );
                anxietyField.SetValue( null, anxietyForUse );
                orig( source, map, hasDistortion );
                Celeste.GFX.FxDistort.Parameters[ "anxiety" ].SetValue( anxiety );
                anxietyField.SetValue( null, anxiety );
            }
            else
            {
                orig( source, map, hasDistortion );
            }
        }

        private void OnPlayerUpdate( On.Celeste.Player.orig_Update orig, Celeste.Player self )
        {
            orig( self );
            if ( Settings.Enabled && Engine.Scene.OnInterval( Calc.LerpClamp( 0.05f, 0, Math.Min( 0.99f, Session.AnxietyBonus ) ) ) )
            {
                Session.AnxietyStutter = Calc.Random.NextFloat();
            }
        }

        private void OnLevelLoadLevel( On.Celeste.Level.orig_LoadLevel orig, Celeste.Level self, Celeste.Player.IntroTypes playerIntro, bool isFromLoader )
        {
            orig( self, playerIntro, isFromLoader );

            Celeste.Player player = self.Tracker.GetEntity<Celeste.Player>();
            if ( Settings.Enabled && player != null )
            {
                int ChaserCount = 0;
                List<Entity> existingBadelines = self.Tracker.GetEntities<Celeste.BadelineOldsite>();
                if ( existingBadelines != null )
                {
                    ChaserCount += existingBadelines.Count;
                }
                for ( int i = 0; i < Session.ChaserCount; i++ )
                {
                    self.Add( new Celeste.BadelineOldsite( player.Position + Vector2.UnitY * 32, ChaserCount++ ) );
                }

                player.Add( new Coroutine( OshiroSpawnRoutine( player, Session.OshiroCount ) ) );
            }

            if ( Settings.Enabled )
            {
                foreach ( EveryTimeRule rule in Settings.Rules )
                {
                    if ( rule.Causes.Contains( EveryTimeRule.Cause.ScreenChange ) )
                    {
                        rule.Apply( self );
                    }
                }

                if ( self.NewLevel )
                {
                    foreach ( EveryTimeRule rule in Settings.Rules )
                    {
                        if ( rule.Causes.Contains( EveryTimeRule.Cause.NewRoom ) )
                        {
                            rule.Apply( self );
                        }
                    }
                }
            }
        }

        private void OnStrawberryOnCollect( On.Celeste.Strawberry.orig_OnCollect orig, Celeste.Strawberry self )
        {
            orig( self );
            if ( Settings.Enabled )
            {
                foreach ( EveryTimeRule rule in Settings.Rules )
                {
                    if ( rule.Causes.Contains( EveryTimeRule.Cause.CollectBerry ) )
                    {
                        rule.Apply( self.Scene );
                    }
                }
            }
        }

        private void OnKeyOnPlayer( On.Celeste.Key.orig_OnPlayer orig, Celeste.Key self, Celeste.Player player )
        {
            orig( self, player );
            if ( Settings.Enabled )
            {
                foreach ( EveryTimeRule rule in Settings.Rules )
                {
                    if ( rule.Causes.Contains( EveryTimeRule.Cause.CollectKey ) )
                    {
                        rule.Apply( self.Scene );
                    }
                }
            }
        }

        private void OnKeyRegisterUsed( On.Celeste.Key.orig_RegisterUsed orig, Celeste.Key self )
        {
            orig( self );
            if ( Settings.Enabled )
            {
                foreach ( EveryTimeRule rule in Settings.Rules )
                {
                    if ( rule.Causes.Contains( EveryTimeRule.Cause.UseKey ) )
                    {
                        rule.Apply( self.Scene );
                    }
                }
            }
        }

        private void OnPlayerDuck( On.Celeste.Player.orig_Duck orig, Celeste.Player self )
        {
            throw new NotImplementedException();
        }

        private void ILPlayerUpdate( ILContext il )
        {
            ILCursor c = new ILCursor( il );
            if (
            c.TryGotoNext( MoveType.After,
                x => x.MatchLdarg( 0 ),
                x => x.MatchLdfld<Celeste.Player>( "Dashes" ),
                x => x.MatchLdcI4( 1 ),
                x => x.Match( OpCodes.Bgt_S ),
                x => x.MatchLdarg( 0 ),
                x => x.MatchLdfld<Celeste.Player>( "startHairCount" ),
                x => x.Match( OpCodes.Br_S ),
                x => x.MatchLdcI4( 5 ),
                x => x.MatchStfld<Celeste.PlayerSprite>( "HairCount" )
                ) )
            {
                c.Index -= 1;
                int index = c.EmitDelegate<Func<int, int>>( (i) => ( Session != null && Settings.Enabled ) ? Session.ExtraHair + i : i );
            }
        }

        public static IEnumerator OshiroSpawnRoutine( Celeste.Player player, int count )
        {
            while ( count > 0 )
            {
                yield return 0.5f;
                player.Scene.Add( new Celeste.AngryOshiro( new Vector2( player.SceneAs<Celeste.Level>().Bounds.Left - 64, player.Position.Y ), false ) );
                count -= 1;
            }
        }
    }
}
