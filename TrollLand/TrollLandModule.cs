using Celeste.Mod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrollLand
{
    public class TrollLandModule : EverestModule
    {
        public static TrollLandModule Instance;

        public TrollLandModule()
        {
            Instance = this;
        }

        public override Type SessionType => typeof(TrollLandSession);
        public static TrollLandSession Session => (TrollLandSession)Instance._Session;

        public override void Load()
        {
            TrollLandHooks.Load();
        }

        public override void Unload()
        {
            TrollLandHooks.Unload();
        }
    }
}
