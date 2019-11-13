using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Microsoft.Xna.Framework;
using On.Celeste;

namespace TrollLand
{
    public class TrollLandHooks
    {
        public static void Load()
        {
            On.Celeste.Player.Die += OnDeath;
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
    }
}
