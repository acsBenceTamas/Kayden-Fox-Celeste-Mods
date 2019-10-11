using Celeste;
using Celeste.Mod.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Monocle;
using FactoryHelper.Components;

namespace FactoryHelper.Triggers
{
    [CustomEntity("FactoryHelper/PermanentActivationTrigger")]
    class PremanentActivationTrigger : Trigger
    {
        private FactoryActivatorComponent[] _activators;

        public PremanentActivationTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            string [] _activationIds = data.Attr("activationIds").Split(',');
            _activators = new FactoryActivatorComponent[_activationIds.Length];
            for(int i = 0; i < _activationIds.Length; i++)
            {
                Add(_activators[i] = new FactoryActivatorComponent());
                _activators[i].ActivationId = _activationIds[i];
                _activators[i].OnTurnOn = OnTurnOn;
            }
        }

        private void OnTurnOn()
        {
            Console.WriteLine("Turned on");
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (_activators.Length == 0)
            {
                RemoveSelf();
            }
            foreach (var activator in _activators)
            {
                activator.Added(scene);
            }
        }

        public override void Removed(Scene scene)
        {
            Level level = scene as Level;
            Console.WriteLine($"Player exists: {level.Tracker.GetEntity<Player>() != null}");
            if (CollideCheck<Player>())
            {
                foreach (var activator in _activators)
                {
                    Console.WriteLine($"Activator with ID {activator.ActivationId} is {activator.Activated} ({activator.ActivationCount})");
                    if (activator.Activated)
                    {
                        Console.WriteLine("Activating permanently");
                        level.Session.SetFlag($"FactoryActivation:{activator.ActivationId}", true);
                    }
                    foreach (FactoryActivatorComponent otherActivator in Scene.Tracker.GetComponents<FactoryActivatorComponent>())
                    {
                        if (otherActivator != activator)
                        {
                            otherActivator.Added(level);
                        }
                    }
                }
            }
            base.Removed(scene);
        }
    }
}
