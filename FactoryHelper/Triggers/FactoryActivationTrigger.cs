using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;
using FactoryHelper.Components;
using Celeste.Mod.Entities;

namespace FactoryHelper.Triggers
{
    [CustomEntity("FactoryHelper/FactoryActivationTrigger")]
    public class FactoryActivationTrigger : Trigger
    {
        public FactoryActivator Activator { get; }

        private readonly bool _resetOnLeave;
        private readonly bool _persistent;
        private readonly HashSet<string> _activationIds = new HashSet<string>();
        private bool _hasFired;

        public FactoryActivationTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            string[] activationIds = data.Attr("activationIds", "").Split(',');

            _persistent = data.Bool("persistent", false);
            _resetOnLeave = _persistent ? false : data.Bool("resetOnLeave", false);
            Add(Activator = new FactoryActivator());
            Activator.ActivationId = data.Attr("ownActivationId") == string.Empty ? null : data.Attr("ownActivationId");
            Activator.StartOn = Activator.ActivationId == null;

            foreach (string activationId in activationIds)
            {
                if (activationId != "")
                {
                    _activationIds.Add(activationId);
                }
            }
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            if (Activator.IsOn && (!_hasFired || _resetOnLeave))
            {
                SetSessionTags(true);
                SendOutSignals(true);
                _hasFired = true;
            }
        }

        public override void OnLeave(Player player)
        {
            base.OnLeave(player);
            if (Activator.IsOn && _resetOnLeave)
            {
                SetSessionTags(false);
                SendOutSignals(false);
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Activator.HandleStartup(scene);

        }

        private void SendOutSignals(bool activating = true)
        {
            foreach (FactoryActivator activator in Scene.Tracker.GetComponents<FactoryActivator>())
            {
                if (_activationIds.Contains(activator.ActivationId))
                {
                    if (activating)
                    {
                        activator.Activate();
                    }
                    else
                    {
                        activator.Deactivate();
                    }
                }
            }
        }

        private void SetSessionTags(bool activating = true)
        {
            if (_persistent)
            {
                Level level = (Scene as Level);
                foreach (string activationId in _activationIds)
                {
                    level.Session.SetFlag($"FactoryActivation:{activationId}", activating);
                }
            }
        }
    }
}
