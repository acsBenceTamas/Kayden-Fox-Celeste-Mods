using Celeste.Mod;

namespace KaydenSpriteMod
{
    [SettingName( "Kayden Sprite Mod" )]
    public class KaydenSpriteSettings : EverestModuleSettings
    {
        [SettingInGame( false )]
        public bool Enabled { get; set; } = true;

        public KaydenSpriteSettings()
        {
        }
    }
}