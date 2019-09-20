using Celeste;
using Microsoft.Xna.Framework;
using System;
using Monocle;
using System.Collections.Generic;

namespace FactoryHelper.Triggers
{
    class FactoryActivationTrigger : Trigger
    {
        private bool _resetOnLeave;
        private bool _persistent;
        private HashSet<string> _activationIds = new HashSet<string>();

        public FactoryActivationTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            string[] activationIds = data.Attr("activationIds", "").Split(',');

            _persistent = data.Bool("persistent", false);
            _resetOnLeave = _persistent ? false : data.Bool("resetOnLeave", false);

            foreach (string activationId in activationIds)
            {
                if (activationId != "")
                {
                    object persistenceString = _persistent ? "Persistent" : string.Empty;
                    _activationIds.Add($"{persistenceString}FactoryActivation:{activationId}");
                }
            }
        }

        private void Activate(string activationId)
        {
            (Scene as Level).Session.SetFlag(activationId, true);
        }

        private void Deactivate(string activationId)
        {
            (Scene as Level).Session.SetFlag(activationId, false);
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            foreach (string activationId in _activationIds)
            {
                Activate(activationId);
            }
        }

        public override void OnLeave(Player player)
        {
            base.OnLeave(player);
            if (_resetOnLeave)
            {
                foreach (string activationId in _activationIds)
                {
                    Deactivate(activationId);
                }
            }
        }

        private void HandleRest()
        {
            if (!_persistent)
            {
                foreach (string activationId in _activationIds)
                {
                    Deactivate(activationId);
                }
            }
        }

        public override void SceneEnd(Scene scene)
        {
            HandleRest();
            base.SceneEnd(scene);
        }

        public override void Removed(Scene scene)
        {
            HandleRest();
            base.Removed(scene);
        }
    }
}
