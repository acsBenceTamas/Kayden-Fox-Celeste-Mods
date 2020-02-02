using Celeste;
using Celeste.Mod;
using Monocle;

namespace EtherHelper
{
    public class EtherHelperModule : EverestModule
    {
        public static SpriteBank SpriteBank { get; private set; }

        public static EtherHelperModule Instance;

        public EtherHelperModule()
        {
            Instance = this;
        }
        public override void LoadContent(bool firstLoad)
        {
            base.LoadContent(firstLoad);
            SpriteBank = new SpriteBank(GFX.Game, "Graphics/EtherHelper/Sprites.xml");
        }

        public override void Load()
        {
            return;
        }

        public override void Unload()
        {
            return;
        }
    }
}
