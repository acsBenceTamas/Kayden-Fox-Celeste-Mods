using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.AdventureHelper.Entities;
using System;
using On.Celeste;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;

namespace Celeste.Mod.AdventureHelper
{
    class AdventureHelperHooks
    {
        public static void Load()
        {
            // On.Celeste.DreamBlock.Added += DreamBlockAdded;
            //On.Monocle.Entity.Removed += EntityRemoved;
            //On.Celeste.DreamBlock.Render += DreamBlockRender;
            //On.Celeste.DreamBlock.ctor_EntityData_Vector2 += DreamBlockConstructor;
            On.Celeste.CrushBlock.MoveHCheck += OnCrushBlockMoveHCheck;
            On.Celeste.CrushBlock.MoveVCheck += OnCrushBlockMoveVCheck;
        }

        public static void Unload()
        {
            //On.Celeste.DreamBlock.Added -= DreamBlockAdded;
            //On.Monocle.Entity.Removed -= EntityRemoved;
            //On.Celeste.DreamBlock.Render -= DreamBlockRender;
            On.Celeste.CrushBlock.MoveHCheck -= OnCrushBlockMoveHCheck;
            On.Celeste.CrushBlock.MoveVCheck -= OnCrushBlockMoveVCheck;
        }

        private static bool OnCrushBlockMoveVCheck( On.Celeste.CrushBlock.orig_MoveVCheck orig, CrushBlock self, float amount )
        {
            CrushBlockCollideCheckGroupedFallingBlock( self, amount * Vector2.UnitY );
            return orig( self, amount );
        }

        private static bool OnCrushBlockMoveHCheck( On.Celeste.CrushBlock.orig_MoveHCheck orig, CrushBlock self, float amount )
        {
            CrushBlockCollideCheckGroupedFallingBlock( self, amount * Vector2.UnitX );
            return orig( self, amount );
        }

        private static void CrushBlockCollideCheckGroupedFallingBlock( CrushBlock self, Vector2 amount )
        {
            Console.WriteLine( $"Tryina check {self.Position + amount}" );
            GroupedFallingBlock fallingBlock = self.CollideFirst<GroupedFallingBlock>( self.Position + amount );
            if ( fallingBlock != null )
            {
                fallingBlock.Trigger();
            }
        }

        private static void DreamBlockConstructor(On.Celeste.DreamBlock.orig_ctor_EntityData_Vector2 orig, DreamBlock self, EntityData data, Vector2 offset)
        {
            orig(self, data, offset);
            if (data.Nodes.Length > 0)
            {
                AdventureHelperModule.Session.DreamBlocksNotToCombine.Add(self);
            }
        }

        private static void DreamBlockRender(On.Celeste.DreamBlock.orig_Render orig, DreamBlock self )
        {
            if (!AdventureHelperModule.Settings.CombineDreamBlocks || AdventureHelperModule.Session.DreamBlocksNotToCombine.Contains(self))
            {
                orig(self);
            }
        }

        private static void EntityRemoved(On.Monocle.Entity.orig_Removed orig, Monocle.Entity self, Monocle.Scene scene)
        {
            if (AdventureHelperModule.Settings.CombineDreamBlocks)
            {
                if ( self is DreamBlock )
                {
                    AdventureHelperModule.Session.DreamBlocksToCombine.Remove( self as DreamBlock );
                }
            }
            orig(self, scene);
        }

        private static void DreamBlockAdded(On.Celeste.DreamBlock.orig_Added orig, DreamBlock self, Monocle.Scene scene)
        {
            orig(self, scene);
            if (AdventureHelperModule.Settings.CombineDreamBlocks && !AdventureHelperModule.Session.DreamBlocksNotToCombine.Contains(self))
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
