using Celeste;
using Celeste.Mod;
using System.Collections.Generic;

namespace FactoryHelper
{
    public class FactoryHelperSession : EverestModuleSession
    {
        public HashSet<EntityID> Batteries = new HashSet<EntityID>();
    }
}