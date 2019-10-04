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
        public static SpriteBank SpriteBank { get; private set; }

        public static FactoryHelperModule Instance;

        public FactoryHelperModule()
        {
            Instance = this;
        }

        public override Type SettingsType => null;

        public override void LoadContent(bool firstLoad)
        {
            base.LoadContent(firstLoad);
            SpriteBank = new SpriteBank(GFX.Game, "Graphics/FactoryHelper/Sprites.xml");
        }

        public override void Load()
        {
            Everest.Events.Level.OnLoadEntity += LevelOnLoadEntity;
            FactoryHelperHooks.Load();
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
                    case "FactoryHelper/PistonUp":
                        level.Add(new Piston(entityData, offset, Piston.Direction.Up));
                        return true;
                    case "FactoryHelper/PistonDown":
                        level.Add(new Piston(entityData, offset, Piston.Direction.Down));
                        return true;
                    case "FactoryHelper/PistonLeft":
                        level.Add(new Piston(entityData, offset, Piston.Direction.Left));
                        return true;
                    case "FactoryHelper/PistonRight":
                        level.Add(new Piston(entityData, offset, Piston.Direction.Right));
                        return true;
                    case "FactoryHelper/FactoryActivationTrigger":
                        level.Add(new FactoryActivationTrigger(entityData, offset));
                        return true;
                    case "FactoryHelper/WindTunnel":
                        level.Add(new WindTunnel(entityData, offset));
                        return true;
                    case "FactoryHelper/DashFuseBox":
                        level.Add(new DashFuseBox(entityData, offset));
                        return true;
                    case "FactoryHelper/PowerLine":
                        level.Add(new PowerLine(entityData, offset));
                        return true;
                    case "FactoryHelper/RustyLamp":
                        level.Add(new RustyLamp(entityData, offset));
                        return true;
                    case "FactoryHelper/BoomBox":
                        level.Add(new BoomBox(entityData, offset));
                        return true;
                    case "FactoryHelper/RustySpikeUp":
                        level.Add(new RustySpike(entityData, offset, RustySpike.Directions.Up));
                        return true;
                    case "FactoryHelper/RustySpikeDown":
                        level.Add(new RustySpike(entityData, offset, RustySpike.Directions.Down));
                        return true;
                    case "FactoryHelper/RustySpikeLeft":
                        level.Add(new RustySpike(entityData, offset, RustySpike.Directions.Left));
                        return true;
                    case "FactoryHelper/RustySpikeRight":
                        level.Add(new RustySpike(entityData, offset, RustySpike.Directions.Right));
                        return true;
                    case "FactoryHelper/ElectrifiedWallUp":
                        level.Add(new ElectrifiedWall(entityData, offset, RustySpike.Directions.Up));
                        return true;
                    case "FactoryHelper/ElectrifiedWallDown":
                        level.Add(new ElectrifiedWall(entityData, offset, RustySpike.Directions.Down));
                        return true;
                    case "FactoryHelper/ElectrifiedWallLeft":
                        level.Add(new ElectrifiedWall(entityData, offset, RustySpike.Directions.Left));
                        return true;
                    case "FactoryHelper/ElectrifiedWallRight":
                        level.Add(new ElectrifiedWall(entityData, offset, RustySpike.Directions.Right));
                        return true;
                    case "FactoryHelper/Conveyor":
                        level.Add(new Conveyor(entityData, offset));
                        return true;
                    case "FactoryHelper/ThrowBox":
                        level.Add(new ThrowBox(entityData, offset));
                        return true;
                    case "FactoryHelper/ThrowBoxSpawner":
                        level.Add(new ThrowBoxSpawner(entityData, offset));
                        return true;
                    case "FactoryHelper/PressurePlate":
                        level.Add(new PressurePlate(entityData, offset));
                        return true;
                    case "FactoryHelper/KillerDebris":
                        level.Add(new KillerDebris(entityData, offset));
                        return true;
                    case "FactoryHelper/DashNegator":
                        level.Add(new DashNegator(entityData, offset));
                        return true;
                    case "FactoryHelper/DoorRusty":
                        level.Add(new DoorRusty(entityData, offset));
                        return true;
                    default:
                        return false;
                }
            }
            return false;
        }
    }
}
