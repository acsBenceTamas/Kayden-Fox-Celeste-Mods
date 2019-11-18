using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using TrollLand.Cutscenes;

namespace TrollLand.Triggers
{
    [CustomEntity("TrollLand/SoftlockTrigger")]
    class SoftlockTrigger : Trigger
    {
        public SoftlockTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            TrollLandModule.Session.InSoftLock = true;
        }

        public override void OnStay(Player player)
        {
            base.OnEnter(player);
            Softlock softlock = Scene.Tracker.GetEntity<Softlock>();
            TrollLandModule.Session.InSoftLock = true;
            if (softlock == null)
            {
                Scene.Add(new Softlock());
            }
        }

        public override void OnLeave(Player player)
        {
            base.OnEnter(player);
            TrollLandModule.Session.InSoftLock = false;
        }

        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);
            TrollLandModule.Session.InSoftLock = false;
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            TrollLandModule.Session.InSoftLock = false;
        }
    }
}
