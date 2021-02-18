using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.Utils;
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
        private static ILHook ILPlayerStarFlyCoroutine;

        public EveryTimeModule()
        {
            Instance = this;
        }

        public override void LoadSettings()
        {
            base.LoadSettings();
            if ( Settings.Rules == null )
            {
                Logger.Log( "EveryTimeRule", "Rules field could not be deserialized. Either not found or badly formatted." );
                Settings.Rules = new List<EveryTimeRule>();
            }
            if ( Settings.Rules.Count == 0 )
            {
                Logger.Log( "EveryTimeRule", "Rules field empty. Creating default rule set." );
                EveryTimeRule exampleRule = new EveryTimeRule();
                exampleRule.Causes.Add( "CollectBerry" );
                exampleRule.Effects.Add( "ExtraHair", "5" );
                exampleRule.Effects.Add( "Anxiety", "-10000" );
                Settings.Rules.Add( exampleRule );
                EveryTimeRule exampleRule2 = new EveryTimeRule();
                exampleRule2.Causes.Add( "Die" );
                exampleRule2.Effects.Add( "ExtraHair", "-1" );
                exampleRule2.Effects.Add( "Anxiety", "0.05" );
                Settings.Rules.Add( exampleRule2 );
                SaveSettings();
            }
            else
            {
                EveryTimeRule.CheckForErrors();
            }
        }

        public override void Load()
        {

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
            On.Celeste.Player.Duck += OnPlayerDuck;
            On.Celeste.Level.CompleteArea_bool_bool_bool += OnLevelCompleteArea;
            On.Celeste.HeartGem.Collect += OnHeartGemCollect;
            On.Celeste.Player.UseRefill += OnPlayerUseRefill;
            On.Celeste.Player.StartStarFly += OnPlayerStartStarFly;
            On.Celeste.Spring.OnCollide += OnSpringOnCollide;
            On.Celeste.Player.DreamDashBegin += OnPlayerDreamDashBegin;
            On.Celeste.CutsceneEntity.Start += OnCutsceneEntityStart;

            IL.Monocle.Engine.Update += ILEngineUpdate;
            IL.Celeste.Bumper.OnPlayer += ILBumperOnPlayer;
            ILPlayer_orig_Update = new ILHook( typeof( Celeste.Player ).GetMethod( "orig_Update", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance ), ILPlayerUpdate );
            ILPlayerStarFlyCoroutine = new ILHook( typeof( Celeste.Player ).GetMethod( "StarFlyCoroutine", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance ).GetStateMachineTarget(), ILPlayerStarFlyCoroutine_Actual );
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
            On.Celeste.Level.CompleteArea_bool_bool_bool -= OnLevelCompleteArea;
            On.Celeste.HeartGem.Collect -= OnHeartGemCollect;
            On.Celeste.Player.UseRefill -= OnPlayerUseRefill;
            On.Celeste.Spring.OnCollide -= OnSpringOnCollide;
            On.Celeste.Player.DreamDashBegin -= OnPlayerDreamDashBegin;
            On.Celeste.CutsceneEntity.Start -= OnCutsceneEntityStart;

            IL.Monocle.Engine.Update -= ILEngineUpdate;
            IL.Celeste.Bumper.OnPlayer -= ILBumperOnPlayer;
            ILPlayer_orig_Update.Dispose();
            ILPlayer_orig_Update = null;
            ILPlayerStarFlyCoroutine.Dispose();
            ILPlayerStarFlyCoroutine = null;
        }

        private void OnPlayerJump( On.Celeste.Player.orig_Jump orig, Celeste.Player self, bool particles, bool playSfx )
        {
            if ( Settings.Enabled && self.InControl )
            {
                ApplyRules( self.Scene, "Jump" );
            }
            orig( self, particles, playSfx );
        }

        private void OnPlayerDashBegin( On.Celeste.Player.orig_DashBegin orig, Celeste.Player self )
        {
            if ( Settings.Enabled && self.InControl )
            {
                ApplyRules( self.Scene, "Dash" );
            }
            orig( self );
        }

        private Celeste.PlayerDeadBody OnPlayerDie( On.Celeste.Player.orig_Die orig, Celeste.Player self, Vector2 direction, bool evenIfInvincible, bool registerDeathInStats )
        {
            PlayerDeadBody result = orig( self, direction, evenIfInvincible, registerDeathInStats );
            if ( Settings.Enabled && result != null )
            {
                ApplyRules( self.Scene, "Die" );
            }
            return result;
        }

        private void OnPlayerSprite_ctor( On.Celeste.PlayerSprite.orig_ctor orig, Celeste.PlayerSprite self, PlayerSpriteMode mode )
        {
            orig( self, mode );
            if ( Settings.Enabled )
            {
                self.HairCount = Math.Max( 1, self.HairCount + Session.ExtraHair);
            }
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
            if ( Settings.Enabled )
            {
                var wasOnGroundField = typeof( Celeste.Player ).GetField( "onGround", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance );
                bool wasOnGround = (bool)wasOnGroundField.GetValue( self );
                orig( self );
                var isOnGroundField = typeof( Celeste.Player ).GetField( "onGround", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance );
                bool isOnGround = (bool)isOnGroundField.GetValue( self );

                if ( self.InControl && isOnGround && ( wasOnGround != isOnGround ) )
                {
                    ApplyRules( self.Scene, "Land" );
                }

                if ( Engine.Scene.OnInterval( Calc.LerpClamp( 0.05f, 0, Math.Min( 0.99f, Session.AnxietyBonus ) ) ) )
                {
                    Session.AnxietyStutter = Calc.Random.NextFloat();
                }
            }
            else
            {
                orig( self );
            }
        }

        private void OnLevelLoadLevel( On.Celeste.Level.orig_LoadLevel orig, Celeste.Level self, Celeste.Player.IntroTypes playerIntro, bool isFromLoader )
        {
            orig( self, playerIntro, isFromLoader );

            Session.SpawnedOshiros.Clear();
            Session.SpawnedBadelineChasers.Clear();

            if ( Settings.Enabled )
            {
                Celeste.Player player = self.Tracker.GetEntity<Celeste.Player>();

                if ( !isFromLoader )
                {
                    ApplyRules( self, "ScreenChange" );

                    if ( self.NewLevel )
                    {
                        ApplyRules( self, "NewRoom" );
                    }
                }

                if ( player != null )
                {
                    int chaserCount = self.Tracker.CountEntities<BadelineOldsite>() + self.Tracker.CountEntities<EveryTimeCustomChaser>();
                    int added = 0;
                    while ( added < Session.ChaserCount )
                    {
                        if ( chaserCount >= 7 )
                        {
                            break;
                        }
                        EveryTimeCustomChaser chaser = new EveryTimeCustomChaser( player.Position + Vector2.UnitY * 32, chaserCount++ );
                        self.Add( chaser );
                        Session.SpawnedBadelineChasers.Add( chaser );
                        added++;
                    }

                    player.Add( new Coroutine( OshiroSpawnRoutine( player, Session.OshiroCount ) ) );
                }
            }
        }

        private void OnStrawberryOnCollect( On.Celeste.Strawberry.orig_OnCollect orig, Celeste.Strawberry self )
        {
            orig( self );
            if ( Settings.Enabled )
            {
                ApplyRules( self.Scene, "CollectBerry" );
            }
        }

        private void OnKeyOnPlayer( On.Celeste.Key.orig_OnPlayer orig, Celeste.Key self, Celeste.Player player )
        {
            orig( self, player );
            if ( Settings.Enabled )
            {
                ApplyRules( self.Scene, "CollectKey" );
            }
        }

        private void OnKeyRegisterUsed( On.Celeste.Key.orig_RegisterUsed orig, Celeste.Key self )
        {
            orig( self );
            if ( Settings.Enabled )
            {
                ApplyRules( self.Scene, "UseKey" );
            }
        }

        private void OnPlayerDuck( On.Celeste.Player.orig_Duck orig, Celeste.Player self )
        {
            orig( self );
            if ( Settings.Enabled )
            {
                ApplyRules( self.Scene, "Duck" );
            }
        }

        private Celeste.ScreenWipe OnLevelCompleteArea( On.Celeste.Level.orig_CompleteArea_bool_bool_bool orig, Celeste.Level self, bool spotlightWipe, bool skipScreenWipe, bool skipCompleteScreen )
        {
            Celeste.ScreenWipe wipe = orig( self, spotlightWipe, skipScreenWipe, skipCompleteScreen );
            Session.Reset();
            return wipe;
        }

        private void OnHeartGemCollect( On.Celeste.HeartGem.orig_Collect orig, Celeste.HeartGem self, Celeste.Player player )
        {
            orig( self, player );
            if ( Settings.Enabled )
            {
                ApplyRules( self.Scene, "CollectHeart" );
            }
        }

        private bool OnPlayerUseRefill( On.Celeste.Player.orig_UseRefill orig, Celeste.Player self, bool twoDashes )
        {
            bool refilled = orig( self, twoDashes );
            if ( refilled && Settings.Enabled )
            {
                ApplyRules( self.Scene, "Refill" );
            }
            return refilled;
        }

        private bool OnPlayerStartStarFly( On.Celeste.Player.orig_StartStarFly orig, Celeste.Player self )
        {
            bool startedFly = orig( self );
            if ( startedFly && Settings.Enabled )
            {
                ApplyRules( self.Scene, "Feather" );
            }
            return startedFly;
        }

        private void OnSpringOnCollide( On.Celeste.Spring.orig_OnCollide orig, Celeste.Spring self, Celeste.Player player )
        {
            orig( self, player );
            if ( Settings.Enabled )
            {
                var canUseField = typeof( Celeste.Spring ).GetField( "playerCanUse", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance );
                if ( !( player.StateMachine.State == 9 || !(bool)canUseField.GetValue( self ) ) )
                {
                    ApplyRules( player.Scene, "Spring" );
                }
            }
        }

        private void OnPlayerDreamDashBegin( On.Celeste.Player.orig_DreamDashBegin orig, Celeste.Player self )
        {
            orig( self );
            if ( Settings.Enabled )
            {
                ApplyRules( self.Scene, "DreamDash" );
            }
        }

        private void OnCutsceneEntityStart( On.Celeste.CutsceneEntity.orig_Start orig, Celeste.CutsceneEntity self )
        {
            orig( self );
            if ( Settings.Enabled )
            {
                foreach ( AngryOshiro oshiro in Session.SpawnedOshiros )
                {
                    oshiro.RemoveSelf();
                }
                foreach ( EveryTimeCustomChaser chaser in Session.SpawnedBadelineChasers )
                {
                    chaser.Add( new Coroutine( chaser.KillSelfRoutine() ) );
                }
                Session.SpawnedOshiros.Clear();
                Session.SpawnedBadelineChasers.Clear();
                Session.OshiroCount = 0;
                Session.ChaserCount = 0;
                Distort.Anxiety = 0f;
            }
        }

        private void ILEngineUpdate( ILContext il )
        {
            ILCursor c = new ILCursor( il );
            if ( c.TryGotoNext(
                MoveType.After,
                x => x.MatchCall<Engine>( "get_RawDeltaTime" ),
                x => x.MatchLdsfld<Engine>( "TimeRate" ),
                x => x.MatchMul(),
                x => x.MatchLdsfld<Engine>( "TimeRateB" ),
                x => x.MatchMul()
                ) )
            {
                c.EmitDelegate<Func<float>>( () => ( Session != null && Settings.Enabled && Engine.Scene is Celeste.Level ) ? Session.TimeDilation : 1f );
                c.Emit( OpCodes.Mul );
            }
        }

        private void ILBumperOnPlayer( ILContext il )
        {
            ILCursor c = new ILCursor( il );
            if ( c.TryGotoNext(
                MoveType.After,
                x => x.MatchLdfld<Celeste.Bumper>("respawnTimer"),
                x => x.MatchLdcR4(0),
                x => x.MatchCgtUn(),
                x => x.MatchLdcI4(0),
                x => x.MatchCeq(),
                x => x.MatchStloc(3),
                x => x.MatchLdloc(3),
                x => x.Match( OpCodes.Brfalse )
                ) )
            {
                c.EmitDelegate<Action>( () => {
                    if ( Settings.Enabled )
                    {
                        ApplyRules( Engine.Scene, "Bumper" );
                    }
                } );
            }
        }

        private void ILPlayerStarFlyCoroutine_Actual( ILContext il )
        {
            ILCursor c = new ILCursor( il );
            if ( c.TryGotoNext(
                MoveType.Before,
                x => x.MatchStfld<Celeste.PlayerSprite>( "HairCount" )
            ) )
            {
                c.EmitDelegate<Func<int, int>>( ( i ) => ( Session != null && Settings.Enabled ) ? Session.ExtraHair + i : i );
            }
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
                c.EmitDelegate<Func<int, int>>( (i) => ( Session != null && Settings.Enabled ) ? Session.ExtraHair + i : i );
            }
        }

        public static IEnumerator OshiroSpawnRoutine( Celeste.Player player, int count )
        {
            while ( count > 0 )
            {
                yield return 0.5f;
                Celeste.AngryOshiro oshiro = new Celeste.AngryOshiro( new Vector2( player.SceneAs<Celeste.Level>().Bounds.Left - 64, player.Position.Y ), false );
                player.Scene.Add( oshiro );
                Session.SpawnedOshiros.Add( oshiro );
                count--;
            }
        }

        public static void ApplyRules( Scene scene, string cause )
        {
            foreach ( EveryTimeRule rule in Settings.Rules )
            {
                if ( rule.Causes.Contains( cause ) )
                {
                    rule.Apply( scene );
                }
            }
        }
    }
}
