using Celeste;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryHelper.Components
{
    [Tracked]
    class FactoryActivatorComponent : Component
    {
        public bool Activated { get; private set; }
        public bool StartOn { get; set; }
        public bool IsOn
        {
            get
            {
                return Activated != StartOn;
            }
        }

        public string ActivationId;

        public Action OnTurnOn = null;
        public Action OnTurnOff = null;
        public Action OnStartOn = null;
        public Action OnStartOff = null;

        public FactoryActivatorComponent() : base(true, true)
        {
        }

        public FactoryActivatorComponent(bool active, bool visible) : base(active, visible)
        {
        }

        public void Activate()
        {
            Activated = true;
            HandleOnOff();
        }

        public void Deactivate()
        {
            Activated = false;
            HandleOnOff();
        }

        private void HandleOnOff()
        {
            if (IsOn)
            {
                OnTurnOn?.Invoke();
            }
            else
            {
                OnTurnOff?.Invoke();
            }
        }

        public void StartScene(Scene scene)
        {
            if (ActivationId == null)
            {
                Activated = false;
            }
            else
            {
                Level level = scene as Level;
                Activated = level.Session.GetFlag($"FactoryActivation:{ActivationId}");
            }
            if (IsOn)
            {
                OnStartOn?.Invoke();
            }
            else
            {
                OnStartOff?.Invoke();
            }
        }
    }
}
