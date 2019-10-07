using Celeste;
using Microsoft.Xna.Framework;
using FactoryHelper.Components;
using Monocle;
using FactoryHelper.Entities;
using System;

namespace FactoryHelper
{
    public static class FactoryHelperHooks
    {
        public static void Load()
        {
            On.Celeste.Player.ctor += ctor;
            On.Celeste.Level.LoadLevel += LoadLevel;
            On.Celeste.Player.Die += PlayerDie;
        }

        private static PlayerDeadBody PlayerDie(On.Celeste.Player.orig_Die orig, Player self, Vector2 direction, bool evenIfInvincible, bool registerDeathInStats)
        {
            Session session = (self.Scene as Level).Session;

            PlayerDeadBody playerDeadBody = orig(self, direction, evenIfInvincible, registerDeathInStats);

            Strawberry goldenStrawb = null;
            foreach (Follower follower in self.Leader.Followers)
            {
                if (follower.Entity is Strawberry && (follower.Entity as Strawberry).Golden && !(follower.Entity as Strawberry).Winged)
                {
                    goldenStrawb = (follower.Entity as Strawberry);
                }
            }
            string specialBoxLevel = (FactoryHelperModule.Instance._Session as FactoryHelperSession).SpecialBoxLevel;
            if (goldenStrawb == null && specialBoxLevel != null)
            {
                playerDeadBody.DeathAction = delegate
                {
                    Engine.Scene = new LevelExit(LevelExit.Mode.GoldenBerryRestart, session)
                    {
                        GoldenStrawberryEntryLevel = specialBoxLevel
                    };
                };
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
            var conveyorMover = new ConveyorMoverComponent();
            conveyorMover.OnMove = (amount) => self.MoveH(amount * Engine.DeltaTime);
            self.Add(conveyorMover);
        }
    }
}
