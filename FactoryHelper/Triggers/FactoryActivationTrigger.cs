using Celeste;
using Microsoft.Xna.Framework;
using System;
using Monocle;
using System.Collections.Generic;

namespace FactoryHelper.Triggers
{
    class FactoryActivationTrigger : Trigger
    {
        public bool Activated
        {
            get
            {
                if (_ownActivationId == null)
                {
                    return true;
                }
                else
                {
                    Level level = Scene as Level;
                    return level.Session.GetFlag(_ownActivationId) || level.Session.GetFlag("Persistent" + _ownActivationId);
                }
            }
        }

        private bool _resetOnLeave;
        private bool _persistent;
        private HashSet<string> _activationIds = new HashSet<string>();
        private string _ownActivationId;

        public FactoryActivationTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            string[] activationIds = data.Attr("activationIds", "").Split(',');

            _persistent = data.Bool("persistent", false);
            _resetOnLeave = _persistent ? false : data.Bool("resetOnLeave", false);
            _ownActivationId = data.Attr("ownActivationId") == string.Empty ? null : $"FactoryActivation:{data.Attr("ownActivationId")}";

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
            if (Activated)
            {
                (Scene as Level).Session.SetFlag(activationId, true);
            }
        }

        private void Deactivate(string activationId)
        {
            if (!_persistent)
            {
                (Scene as Level).Session.SetFlag(activationId, false);
            }
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
            foreach (string activationId in _activationIds)
            {
                Deactivate(activationId);
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
