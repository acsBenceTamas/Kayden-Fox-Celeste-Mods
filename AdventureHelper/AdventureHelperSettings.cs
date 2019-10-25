namespace Celeste.Mod.AdventureHelper
{
    [SettingName("Adventure Helper Settings")]
    internal class AdventureHelperSettings : EverestModuleSettings
    {
        [SettingName("Combine Dream Blocks")]
        [SettingInGame(false)]
        public bool CombineDreamBlocks { get; set; } = false;
    }
}