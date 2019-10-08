using Celeste;
using Celeste.Mod;
using System.Collections.Generic;

namespace FactoryHelper
{
    public class FactoryHelperSaveData : EverestModuleSaveData
    {
        public HashSet<EntityID> RustBerries = new HashSet<EntityID>();
    }
}