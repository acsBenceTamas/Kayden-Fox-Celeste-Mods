using Celeste;
using Microsoft.Xna.Framework;
using System;

namespace TrollLand
{
    public class TrollLandHooks
    {
        public static void Load()
        {
            On.Celeste.Player.Die += OnDeath;
            On.Celeste.Dialog.Clean += OnCleanDialogue;
        }

        public static void Unload()
        {
            On.Celeste.Player.Die -= OnDeath;
        }

        private static Celeste.PlayerDeadBody OnDeath(On.Celeste.Player.orig_Die orig, Celeste.Player self, Vector2 direction, bool evenIfInvincible, bool registerDeathInStats)
        {
            Celeste.PlayerDeadBody result = orig(self, direction, evenIfInvincible, registerDeathInStats);
            if (result != null)
            {
                string level = self.SceneAs<Celeste.Level>().Session.Level;
                if (TrollLandModule.Session.DeathCountPerLevel.ContainsKey(level))
                {
                    TrollLandModule.Session.DeathCountPerLevel[level] += 1;
                }
                else
                {
                    TrollLandModule.Session.DeathCountPerLevel[level] = 1;
                }
            }
            return result;
        }

        private static string OnCleanDialogue(On.Celeste.Dialog.orig_Clean orig, string name, Language language)
        {
            if (TrollLandModule.Session != null)
            {
                bool shouldSaySoftlock = false;
                if (!(shouldSaySoftlock = "MENU_PAUSE_RETRY".Equals(name, StringComparison.InvariantCultureIgnoreCase)))
                {
                    shouldSaySoftlock = "MENU_PAUSE_SKIP_CUTSCENE".Equals(name, StringComparison.InvariantCultureIgnoreCase);
                }
                if (TrollLandModule.Session.InSoftLock && shouldSaySoftlock)
                {
                    return Dialog.Clean("KAYDEN_FOX_TROLL_LAND_STAY_IN_SOFT_LOCK", language);
                }
            }
            return orig(name, language);         
        }
    }
}
