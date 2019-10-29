using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.AdventureHelper.Entities;
using System;

namespace Celeste.Mod.AdventureHelper
{
    class AdventureHelperHooks
    {
        public static void Load()
        {
            On.Celeste.DreamBlock.Added += DreamBlockAdded;
            On.Monocle.Entity.Removed += EntityRemoved;
            On.Celeste.DreamBlock.Render += DreamBlockRender;
        }
        public static void Unload()
        {
            On.Celeste.DreamBlock.Added -= DreamBlockAdded;
            On.Monocle.Entity.Removed -= EntityRemoved;
            On.Celeste.DreamBlock.Render -= DreamBlockRender;
        }

        private static void DreamBlockRender(On.Celeste.DreamBlock.orig_Render orig, DreamBlock self)
        {
            if (!AdventureHelperModule.Settings.CombineDreamBlocks)
            {
                orig(self);
            }
        }

        private static void EntityRemoved(On.Monocle.Entity.orig_Removed orig, Entity self, Scene scene)
        {
            if (AdventureHelperModule.Settings.CombineDreamBlocks)
            {
                if (self is DreamBlock)
                {
                    AdventureHelperModule.Session.DreamBlocksToCombine.Remove(self as DreamBlock);
                }
            }
            orig(self, scene);
        }

        private static void DreamBlockAdded(On.Celeste.DreamBlock.orig_Added orig, DreamBlock self, Scene scene)
        {
            orig(self, scene);
            if (AdventureHelperModule.Settings.CombineDreamBlocks)
            {
                AdventureHelperModule.Session.DreamBlocksToCombine.Add(self);
                DreamBlockCombiner combiner = self.Scene.Tracker.GetEntity<DreamBlockCombiner>();
                if (combiner == null)
                {
                    self.Scene.Add(combiner = new DreamBlockCombiner());
                }
            }
        }
    }
}
