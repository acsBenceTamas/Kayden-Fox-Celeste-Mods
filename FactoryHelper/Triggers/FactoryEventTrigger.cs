using Celeste;
using Celeste.Mod.Entities;
using FactoryHelper.Cutscenes;
using Microsoft.Xna.Framework;
using Monocle;

namespace FactoryHelper.Triggers
{
    [CustomEntity("FactoryHelper/EventTrigger")]
    class FactoryEventTrigger : Trigger
    {
        private string _eventName;

        public FactoryEventTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            _eventName = data.Attr("event");
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (_eventName == "factory_phone_intro")
            {
                Scene.Add(new CS01_FactoryHelper_Intro());
            }
        }
    }
}
