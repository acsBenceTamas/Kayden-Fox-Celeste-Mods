using Celeste;
using Microsoft.Xna.Framework;
using FactoryHelper.Components;
using Monocle;
using FactoryHelper.Entities;
using System.Collections;
using FactoryHelper.Cutscenes;

namespace FactoryHelper
{
    public static class FactoryHelperHooks
    {
        public static void Load()
        {
            On.Celeste.Player.ctor += ctor;
            On.Celeste.Level.LoadLevel += LoadLevel;
            On.Celeste.Player.Die += PlayerDie;
            On.Celeste.LevelExit.Routine += RespawnRoutine;
            On.Celeste.Player.Pickup += Pickup;
            On.Celeste.Lookout.LookRoutine += LookRoutine;
            On.Celeste.LevelEnter.Go += LevelEnterGo;
            On.Celeste.DashBlock.Break_Vector2_Vector2_bool_bool += DashBlockBreak;
            On.Celeste.DashBlock.RemoveAndFlagAsGone += DashBlockRemoveAndFlagAsGone;
        }

        private static void DashBlockRemoveAndFlagAsGone( On.Celeste.DashBlock.orig_RemoveAndFlagAsGone orig, Celeste.DashBlock self )
        {
            if ( self is FactoryActivatorDashBlock )
            {
                self.RemoveSelf();
                return;
            }
            orig( self );
        }

        private static void DashBlockBreak( On.Celeste.DashBlock.orig_Break_Vector2_Vector2_bool_bool orig, Celeste.DashBlock self, Vector2 from, Vector2 direction, bool playSound, bool playDebrisSound )
        {
            orig( self, from, direction, playSound, playDebrisSound );
            if ( self is FactoryActivatorDashBlock )
            {
                ( self as FactoryActivatorDashBlock ).OnBreak();
            }
        }

        private static void LevelEnterGo(On.Celeste.LevelEnter.orig_Go orig, Celeste.Session session, bool fromSaveData)
        {
            if (!fromSaveData && session.StartedFromBeginning && session.Area.Mode == AreaMode.Normal && session.Area.ChapterIndex == 1 && session.Area.GetLevelSet() == "KaydenFox/FactoryMod")
            {
                Engine.Scene = new FactoryIntroVignette(session);
            }
            else
            {
                orig(session, fromSaveData);
            }
        }

        private static IEnumerator LookRoutine(On.Celeste.Lookout.orig_LookRoutine orig, Celeste.Lookout self, Celeste.Player player)
        {
            SteamWall steamWall = self.Scene.Tracker.GetEntity<SteamWall>();
            if (steamWall != null)
            {
                steamWall.Halted = true;
            }
            yield return orig(self, player);
            if (steamWall != null)
            {
                steamWall.Halted = false;
            }

        }

        private static bool Pickup(On.Celeste.Player.orig_Pickup orig, Celeste.Player self, Celeste.Holdable pickup)
        {
            if (self.Holding == null || !(self.Holding.Entity is ThrowBox))
            {
                return orig(self, pickup);
            }
            return false;
        }

        private static IEnumerator RespawnRoutine(On.Celeste.LevelExit.orig_Routine orig, Celeste.LevelExit self)
        {
            FactoryHelperSession factorySession = FactoryHelperModule.Session;
            if (factorySession.SpecialBoxPosition != null)
            {
                factorySession.OriginalSession.Level = factorySession.SpecialBoxLevel;
                factorySession.OriginalSession.RespawnPoint = factorySession.SpecialBoxPosition;
                Engine.Scene = new Celeste.LevelLoader(factorySession.OriginalSession);
                factorySession.SpecialBoxPosition = null;
            }
            else
            {
                yield return orig(self);
            }
        }

        private static Celeste.PlayerDeadBody PlayerDie(On.Celeste.Player.orig_Die orig, Celeste.Player self, Vector2 direction, bool evenIfInvincible, bool registerDeathInStats)
        {
            Celeste.Session session = ( self.Scene as Celeste.Level ).Session;

            Celeste.PlayerDeadBody playerDeadBody = orig(self, direction, evenIfInvincible, registerDeathInStats);

            if (playerDeadBody != null)
            {
                Celeste.Strawberry goldenStrawb = null;
                foreach ( Celeste.Follower follower in self.Leader.Followers)
                {
                    if ( follower.Entity is Celeste.Strawberry && ( follower.Entity as Celeste.Strawberry ).Golden && !( follower.Entity as Celeste.Strawberry ).Winged)
                    {
                        goldenStrawb = ( follower.Entity as Celeste.Strawberry );
                    }
                }
                Vector2? specialBoxLevel = (FactoryHelperModule.Instance._Session as FactoryHelperSession).SpecialBoxPosition;
                if (goldenStrawb == null && specialBoxLevel != null)
                {
                    playerDeadBody.DeathAction = delegate
                    {
                        Engine.Scene = new Celeste.LevelExit( Celeste.LevelExit.Mode.Restart, session);
                    };
                }
            }
            return playerDeadBody;
        }

        private static void LoadLevel(On.Celeste.Level.orig_LoadLevel orig, Celeste.Level self, Celeste.Player.IntroTypes playerIntro, bool isFromLoader)
        {
            orig(self, playerIntro, isFromLoader);
            if (playerIntro != Celeste.Player.IntroTypes.Transition || isFromLoader)
            {
                Celeste.Player player = self.Tracker.GetEntity<Celeste.Player>();

                foreach ( Celeste.EntityID key in (FactoryHelperModule.Instance._Session as FactoryHelperSession).Batteries)
                {
                    self.Add(new Battery(player, key));
                }
            }
        }

        private static void ctor(On.Celeste.Player.orig_ctor orig, Celeste.Player self, Vector2 position, PlayerSpriteMode spriteMode)
        {
            orig(self, position, spriteMode);
            var conveyorMover = new ConveyorMover();
            conveyorMover.OnMove = (amount) =>
            {
                if (self.StateMachine.State != 1)
                {
                    self.MoveH(amount * Engine.DeltaTime);
                }
            };
            self.Add(conveyorMover);
        }
    }
}
