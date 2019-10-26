using System;
using Celeste.Mod;
using Celeste;
using Monocle;

namespace FactoryHelper
{
    class FactoryHelperModule : EverestModule
    {
        public static SpriteBank SpriteBank { get; private set; }

        public static FactoryHelperModule Instance;

        public FactoryHelperModule()
        {
            Instance = this;
        }

        public override Type SettingsType => null;
        public override Type SaveDataType => typeof(FactoryHelperSaveData);
        public static FactoryHelperSaveData SaveData => (FactoryHelperSaveData)Instance._SaveData;
        public override Type SessionType => typeof(FactoryHelperSession);
        public static FactoryHelperSession Session => (FactoryHelperSession)Instance._Session;

        public override void LoadContent(bool firstLoad)
        {
            base.LoadContent(firstLoad);
            SpriteBank = new SpriteBank(GFX.Game, "Graphics/FactoryHelper/Sprites.xml");
        }

        public override void Load()
        {
            FactoryHelperHooks.Load();
        }

        public override void Unload()
        {
        }
    }
}
