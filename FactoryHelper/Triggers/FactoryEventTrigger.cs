using Celeste;
using Celeste.Mod.Entities;
using FactoryHelper.Cutscenes;
using FactoryHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace FactoryHelper.Triggers
{
    [CustomEntity("FactoryHelper/EventTrigger")]
    class FactoryEventTrigger : Trigger
    {
        private readonly string _eventName;

        public FactoryEventTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            _eventName = data.Attr("event");
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            Level level = Scene as Level;
            if (_eventName == "factory_entrance" && !level.Session.GetFlag("FactoryHelper_factory_entrance_trigger"))
            {
                level.Session.SetFlag("FactoryHelper_factory_entrance_trigger");
                Scene.Add(new CS01_FactoryHelper_Entrance(player));
                RemoveSelf();
            }
            else if (_eventName == "factory_midway" && !level.Session.GetFlag("FactoryHelper_factory_midway_trigger"))
            {
                level.Session.SetFlag("FactoryHelper_factory_midway_trigger");
                Scene.Add(new CS01_FactoryHelper_MidWay(player));
                RemoveSelf();
            }
            else if (_eventName == "factory_machine_heart" && !level.Session.GetFlag("FactoryHelper_factory_machine_heart_trigger"))
            {
                MachineHeart machineHeart = level.Tracker.GetEntity<MachineHeart>();
                level.Session.SetFlag("FactoryHelper_factory_machine_heart_trigger");
                if (machineHeart != null)
                {
                    Scene.Add(new CS01_FactoryHelper_MachineHeart(player, machineHeart));
                }
                RemoveSelf();
            }
            else if (_eventName == "factory_ending" && !level.Session.GetFlag("FactoryHelper_factory_ending_trigger"))
            {
                level.Session.SetFlag("FactoryHelper_factory_ending_trigger");
                Scene.Add(new CS01_FactoryHelper_Ending(player));
                RemoveSelf();
            }
        }
    }
}
