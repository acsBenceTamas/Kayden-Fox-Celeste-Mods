using Celeste;
using Microsoft.Xna.Framework;
using System;

namespace FactoryHelper.Triggers
{
    class FactoryActivationTrigger : Trigger
    {
        private bool _resetOnLeave;
        private string _activationId;
        public FactoryActivationTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            _resetOnLeave = data.Bool("resetOnLeave", false);
            string activationId = data.Attr("activationId", "");

            if (activationId != "")
            {
                _activationId = $"FactoryActivation:{activationId}";
                Console.WriteLine($"Trigger ID: {_activationId}");
            }
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            (Scene as Level).Session.SetFlag(_activationId, true);
            Console.WriteLine($"Activating ID: {_activationId}");
        }

        public override void OnLeave(Player player)
        {
            base.OnLeave(player);
            if (_resetOnLeave)
            {
                (Scene as Level).Session.SetFlag(_activationId, false);
                Console.WriteLine($"Deactivating ID: {_activationId}");
            }
        }
    }
}
