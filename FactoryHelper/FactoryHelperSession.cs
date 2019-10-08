using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace FactoryHelper
{
    public class FactoryHelperSession : EverestModuleSession
    {
        public HashSet<EntityID> Batteries = new HashSet<EntityID>();

        public string SpecialBoxLevel;
    }
}