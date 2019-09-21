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
                Console.WriteLine(entityData.Name);
                switch (entityData.Name)
                {
                    case "FactoryHelper/PistonUp":
                        level.Add(new Piston(entityData, offset, "Up"));
                        return true;
                    case "FactoryHelper/PistonDown":
                        level.Add(new Piston(entityData, offset, "Down"));
                        return true;
                    case "FactoryHelper/PistonLeft":
                        level.Add(new Piston(entityData, offset, "Left"));
                        return true;
                    case "FactoryHelper/PistonRight":
                        level.Add(new Piston(entityData, offset, "Right"));
                        return true;
                    case "FactoryHelper/FactoryActivationTrigger":
                        level.Add(new FactoryActivationTrigger(entityData, offset));
                        return true;
                    case "FactoryHelper/DashFuseBox":
                        Console.WriteLine("FuseBox Added");
                        level.Add(new DashFuseBox(entityData, offset));
                        return true;
                    default:
                        return false;
                }
            }
            return false;
        }
    }
}
