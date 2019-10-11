using Celeste;
using Microsoft.Xna.Framework;
using System;
using Monocle;
using System.Collections.Generic;
using FactoryHelper.Components;

namespace FactoryHelper.Triggers
{
    class FactoryActivationTrigger : Trigger
    {
        public FactoryActivatorComponent Activator { get; }

        private readonly bool _resetOnLeave;
        private readonly bool _persistent;
        private readonly HashSet<string> _activationIds = new HashSet<string>();
        private bool _hasFired;

        public FactoryActivationTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            string[] activationIds = data.Attr("activationIds", "").Split(',');

            _persistent = data.Bool("persistent", false);
            _resetOnLeave = _persistent ? false : data.Bool("resetOnLeave", false);
            Add(Activator = new FactoryActivatorComponent());
            Activator.ActivationId = data.Attr("ownActivationId") == string.Empty ? null : data.Attr("ownActivationId");
            Activator.StartOn = false;

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
            if (Activator.IsOn && (_resetOnLeave || !_hasFired))
            {
                SetSessionTags(true);
                SendOutSignals(true);
                _hasFired = true;
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Activator.Added(scene);

        }

        private void SendOutSignals(bool activating = true)
        {
            foreach (FactoryActivatorComponent activator in Scene.Tracker.GetComponents<FactoryActivatorComponent>())
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

        public override void OnLeave(Player player)
        {
            base.OnLeave(player);
            if (Activator.IsOn && _resetOnLeave)
            {
                SetSessionTags(false);
                SendOutSignals(false);
            }
        }
    }
}
