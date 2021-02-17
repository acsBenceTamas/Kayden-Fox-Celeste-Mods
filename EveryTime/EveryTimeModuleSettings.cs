using Celeste.Mod;
using System.Collections.Generic;

namespace EveryTime
{
    public class EveryTimeModuleSettings : EverestModuleSettings
    {
        [SettingInGame( false )]
        public bool Enabled { get; set; } = false;
        public List<EveryTimeRule> Rules { get; set; }
    }
}