using Celeste;
using Microsoft.Xna.Framework;
using FactoryHelper.Components;
using Monocle;
using FactoryHelper.Entities;

namespace FactoryHelper
{
    public static class FactoryHelperHooks
    {
        public static void Load()
        {
            On.Celeste.Player.ctor += ctor;
            On.Celeste.Level.LoadLevel += LoadLevel;
        }

        private static void LoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader)
        {
            orig(self, playerIntro, isFromLoader);
            if (playerIntro != Player.IntroTypes.Transition || isFromLoader)
            {
                Player player = self.Tracker.GetEntity<Player>();

                foreach (EntityID key in (FactoryHelperModule.Instance._SaveData as FactoryHelperSaveData).Batteries)
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
