using System;
using Microsoft.Xna.Framework;
using FactoryHelper.Entities;
using Celeste.Mod;
using Celeste;
using Monocle;
using FactoryHelper.Triggers;

namespace FactoryHelper
{
    class FactoryHelperModule : EverestModule
    {
        public SpriteBank SpriteBank { get; private set; }

        public static FactoryHelperModule Instance;

        public FactoryHelperModule()
        {
            Instance = this;
        }

        public override Type SettingsType => null;

        public override void Load()
        {
            Everest.Events.Level.OnLoadEntity += LevelOnLoadEntity;
        }

        public override void Unload()
        {
            Everest.Events.Level.OnLoadEntity -= LevelOnLoadEntity;
        }

        private bool LevelOnLoadEntity(Level level, LevelData levelData, Vector2 offset, EntityData entityData)
        {
            if (entityData.Name.StartsWith("FactoryHelper"))
            {
                switch (entityData.Name)
                {
                    case "FactoryHelper/Piston":
                        level.Add(new Piston(entityData, offset));
                        return true;
                    case "FactoryHelper/FactoryActivationTrigger":
                        level.Add(new FactoryActivationTrigger(entityData, offset));
                        return true;
                    case "FactoryHelper/DashFuseBox":
                        Console.WriteLine("FuseBox Added");
                        level.Add(new DashFuseBox(entityData, offset, new EntityID(entityData.Level.Name, entityData.ID)));
                        return true;
                    default:
                        return false;
                }
            }
            return false;
        }
    }
}
