using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.AdventureHelper.Entities;

namespace Celeste.Mod.AdventureHelper
{
    class AdventureHelperHooks
    {
        public static void Load()
        {
            On.Celeste.DreamBlock.Added += DreamBlockAdded;
            On.Monocle.Entity.Removed += EntityRemoved;
            On.Celeste.DreamBlock.ctor_EntityData_Vector2 += DreamBlockConstructor;
        }

        private static void EntityRemoved(On.Monocle.Entity.orig_Removed orig, Monocle.Entity self, Monocle.Scene scene)
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

        private static void DreamBlockConstructor(On.Celeste.DreamBlock.orig_ctor_EntityData_Vector2 orig, DreamBlock self, EntityData data, Vector2 offset)
        {
            orig(self, data, offset);
            if (AdventureHelperModule.Settings.CombineDreamBlocks)
            {
                if (data.Nodes.Length == 0)
                {
                    AdventureHelperModule.Session.DreamBlocksToCombine.Add(self);
                }
            }
        }

        private static void DreamBlockAdded(On.Celeste.DreamBlock.orig_Added orig, DreamBlock self, Scene scene)
        {
            if (AdventureHelperModule.Settings.CombineDreamBlocks)
            {
                DreamBlockCombiner combiner = self.Scene.Tracker.GetEntity<DreamBlockCombiner>();
                if (combiner == null)
                {
                    self.Scene.Add(combiner = new DreamBlockCombiner());
                }
            }
        }
    }
}
