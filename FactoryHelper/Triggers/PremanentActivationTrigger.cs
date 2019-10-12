using Celeste;
using Celeste.Mod.Entities;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;
using FactoryHelper.Components;

namespace FactoryHelper.Triggers
{
    [CustomEntity("FactoryHelper/PermanentActivationTrigger")]
    class PremanentActivationTrigger : Trigger
    {
        private readonly FactoryActivatorComponent[] _activators;
        private readonly HashSet<string> _shouldStayPermanent = new HashSet<string>();
        private Level _level;

        public PremanentActivationTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            string [] _activationIds = data.Attr("activationIds").Split(',');
            _activators = new FactoryActivatorComponent[_activationIds.Length];
            for(int i = 0; i < _activationIds.Length; i++)
            {
                Add(_activators[i] = new FactoryActivatorComponent());
                _activators[i].ActivationId = _activationIds[i];
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            _level = Scene as Level;
            if (_activators.Length == 0)
            {
                RemoveSelf();
            }
            foreach (var activator in _activators)
            {
                activator.Added(scene);
                if (activator.Activated)
                {
                    _shouldStayPermanent.Add(activator.ActivationId);
                }
            }
        }

        public override void OnEnter(Player player)
        {
            Console.WriteLine(Collidable);
            base.OnEnter(player);
            foreach (var activator in _activators)
            {
                if (activator.Activated)
                {
                    _level.Session.SetFlag($"FactoryActivation:{activator.ActivationId}", true);
                }
            }
        }

        public override void OnLeave(Player player)
        {
            Console.WriteLine(Collidable);
            if (!CollideCheck(player))
            {
                foreach (var activator in _activators)
                {
                    if (!_shouldStayPermanent.Contains(activator.ActivationId))
                    {
                        _level.Session.SetFlag($"FactoryActivation:{activator.ActivationId}", false);
                    }
                }
            }
            base.OnLeave(player);
        }
    }
}
