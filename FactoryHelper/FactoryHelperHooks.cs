using System;
using Celeste;
using Microsoft.Xna.Framework;
using FactoryHelper.Components;
using Monocle;
using On.Celeste;

namespace FactoryHelper
{
    public static class FactoryHelperHooks
    {
        public static void Load()
        {
            On.Celeste.Player.ctor += ctor;
        }

        private static void ctor(On.Celeste.Player.orig_ctor orig, Celeste.Player self, Vector2 position, PlayerSpriteMode spriteMode)
        {
            orig(self, position, spriteMode);
            var conveyorMover = new ConveyorMoverComponent();
            conveyorMover.OnMove = (amount) => self.MoveH(amount * Engine.DeltaTime);
            self.Add(conveyorMover);
        }
    }
}
