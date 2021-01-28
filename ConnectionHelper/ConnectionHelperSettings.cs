using Celeste.Mod;
using Microsoft.Xna.Framework.Input;

namespace ConnectionHelper
{
    class ConnectionHelperSettings : EverestModuleSettings
    {
        [DefaultButtonBinding( Buttons.RightShoulder, Keys.Q )]
        public ButtonBinding ReleaseCompanionButton { get; set; }

        public ConnectionHelperSettings()
        { }
    }
}
