using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace FactoryHelper
{
    public class FactoryHelperSession : EverestModuleSession
    {
        public HashSet<EntityID> Batteries = new HashSet<EntityID>();
        public HashSet<EntityID> PermanentlyRemovedActivatorDashBlocks = new HashSet<EntityID>();

        public Vector2? SpecialBoxPosition;
        public Session OriginalSession;
        public string SpecialBoxLevel;
    }
}