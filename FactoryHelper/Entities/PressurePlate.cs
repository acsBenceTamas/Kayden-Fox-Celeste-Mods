using Celeste;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using FactoryHelper.Components;
using Monocle;
using System.Collections;

namespace FactoryHelper.Entities
{
    class PressurePlate : Solid
    {
        public Image CaseImage { get; private set; }

        private HashSet<string> _activationIds = new HashSet<string>();
        private Button _button;
        private bool _currentButtonState = false;
        private bool _previousButtonState = false;

        private class Button : Entity
        {
            public Image Image { get; private set; }

            public Button(Vector2 position) : base(position)
            {
                Add(Image = new Image(GFX.Game["objects/FactoryHelper/pressurePlate/plate_button0"]));
                Collider = new Hitbox(14, 7, 1, 6);
                Image.Position.X -= 2;
                Image.Position.Y -= 2;
            }
        }

        public PressurePlate(EntityData data, Vector2 offset) : this (data.Position + offset, data.Attr("activationIds"))
        {
        }

        public PressurePlate(Vector2 position, string activationIds) : base(position, 16, 6, false)
        {
            string[] activationIdArray = activationIds.Split(',');
            Collider.Position.Y += 10;

            foreach (string activationId in activationIdArray)
            {
                if (activationId != "")
                {
                    _activationIds.Add(activationId);
                }
            }
            Add(CaseImage = new Image(GFX.Game["objects/FactoryHelper/pressurePlate/plate_case0"]));
            CaseImage.Position -= new Vector2(2, 2);
            _button = new Button(position);
            Depth = 8020;
            _button.Depth = Depth + 1;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Add(_button);
        }

        public override void Removed(Scene scene)
        {
            scene.Remove(_button);
            base.Removed(scene);
        }

        public override void Update()
        {
            base.Update();
            var actors = Scene.Tracker.GetEntities<Actor>();
            _currentButtonState = false;
            foreach (var actor in actors)
            {
                if (Collide.Check(actor, _button))
                {
                    _currentButtonState = true;
                    break;
                }
            }
            if (_currentButtonState != _previousButtonState)
            {
                SendOutSignals(_currentButtonState);
            }

            int target = _currentButtonState ? 2 : -2;
            _button.Image.Position.Y = Calc.Approach(_button.Image.Position.Y, target, 40f * Engine.DeltaTime);

            _previousButtonState = _currentButtonState;
        }

        private void SendOutSignals(bool shouldActivate = true)
        {
            foreach (FactoryActivatorComponent activator in Scene.Tracker.GetComponents<FactoryActivatorComponent>())
            {
                if (_activationIds.Contains(activator.ActivationId))
                {
                    if (shouldActivate)
                    {
                        activator.Activate(false);
                    }
                    else
                    {
                        activator.Deactivate(false);
                    }
                }
            }
        }
    }
}
