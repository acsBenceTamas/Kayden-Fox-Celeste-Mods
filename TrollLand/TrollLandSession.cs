using Celeste.Mod;
using System;
using System.Collections.Generic;

namespace TrollLand
{
    public class TrollLandSession : EverestModuleSession
    {
        public Dictionary<string, int> DeathCountPerLevel = new Dictionary<string, int>();

        public int DeathCountForLevel(string level)
        {
            if (!DeathCountPerLevel.TryGetValue(level, out int value))
            {
                DeathCountPerLevel[level] = 0;
            }
            return value;
        }
    }
}