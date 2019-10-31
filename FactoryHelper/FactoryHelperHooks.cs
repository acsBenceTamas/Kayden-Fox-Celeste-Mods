using Celeste;
using Microsoft.Xna.Framework;
using FactoryHelper.Components;
using Monocle;
using FactoryHelper.Entities;
using System;
using System.Collections;

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
        }

        private static IEnumerator LookRoutine(On.Celeste.Lookout.orig_LookRoutine orig, Lookout self, Player player)
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

        private static bool Pickup(On.Celeste.Player.orig_Pickup orig, Player self, Holdable pickup)
        {
            if (self.Holding == null)
            {
                return orig(self, pickup);
            }
            return false;
        }

        private static IEnumerator RespawnRoutine(On.Celeste.LevelExit.orig_Routine orig, LevelExit self)
        {
            FactoryHelperSession factorySession = (FactoryHelperModule.Instance._Session as FactoryHelperSession);
            if (factorySession.SpecialBoxPosition != null)
            {
                factorySession.OriginalSession.Level = factorySession.SpecialBoxLevel;
                factorySession.OriginalSession.RespawnPoint = factorySession.SpecialBoxPosition;
                Engine.Scene = new LevelLoader(factorySession.OriginalSession);
                factorySession.SpecialBoxPosition = null;
            }
            else
            {
                yield return orig(self);
            }
        }

        private static PlayerDeadBody PlayerDie(On.Celeste.Player.orig_Die orig, Player self, Vector2 direction, bool evenIfInvincible, bool registerDeathInStats)
        {
            Session session = (self.Scene as Level).Session;

            PlayerDeadBody playerDeadBody = orig(self, direction, evenIfInvincible, registerDeathInStats);

            if (playerDeadBody != null)
            {
                Strawberry goldenStrawb = null;
                foreach (Follower follower in self.Leader.Followers)
                {
                    if (follower.Entity is Strawberry && (follower.Entity as Strawberry).Golden && !(follower.Entity as Strawberry).Winged)
                    {
                        goldenStrawb = (follower.Entity as Strawberry);
                    }
                }
                Vector2? specialBoxLevel = (FactoryHelperModule.Instance._Session as FactoryHelperSession).SpecialBoxPosition;
                if (goldenStrawb == null && specialBoxLevel != null)
                {
                    playerDeadBody.DeathAction = delegate
                    {
                        Engine.Scene = new LevelExit(LevelExit.Mode.Restart, session);
                    };
                }
            }
            return playerDeadBody;
        }

        private static void LoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader)
        {
            orig(self, playerIntro, isFromLoader);
            if (playerIntro != Player.IntroTypes.Transition || isFromLoader)
            {
                Player player = self.Tracker.GetEntity<Player>();

                foreach (EntityID key in (FactoryHelperModule.Instance._Session as FactoryHelperSession).Batteries)
                {
                    self.Add(new Battery(player, key));
                }
            }
        }

        private static void ctor(On.Celeste.Player.orig_ctor orig, Player self, Vector2 position, PlayerSpriteMode spriteMode)
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
