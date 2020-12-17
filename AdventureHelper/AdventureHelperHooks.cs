using Microsoft.Xna.Framework;
using Celeste.Mod.AdventureHelper.Entities;

namespace Celeste.Mod.AdventureHelper
{
    class AdventureHelperHooks
    {
        public static void Load()
        {
            On.Celeste.CrushBlock.MoveHCheck += OnCrushBlockMoveHCheck;
            On.Celeste.CrushBlock.MoveVCheck += OnCrushBlockMoveVCheck;
        }

        public static void Unload()
        {
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
            GroupedFallingBlock fallingBlock = self.CollideFirst<GroupedFallingBlock>( self.Position + amount );
            if ( fallingBlock != null )
            {
                fallingBlock.Trigger();
            }
        }
    }
}
