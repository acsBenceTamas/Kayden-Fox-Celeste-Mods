using Celeste.Mod;

namespace WatchtowerTax
{
    [SettingName( "Watchtower Tax Settings" )]
    public class WatchtowerTaxSettings : EverestModuleSettings
    {
        public bool Enabled { get; set; } = true;
    }
}