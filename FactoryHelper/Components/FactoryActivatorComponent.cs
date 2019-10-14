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
        public bool Activated => ActivationCount > 0;
        public int ActivationCount { get; private set; } = 0;
        public bool StartOn { get; set; }
        public bool IsOn
        {
            get
            {
                return Activated != StartOn;
            }
        }
        public bool StateIsLocked { get; set; } = false;

        public string ActivationId;

        public Action OnTurnOn = null;
        public Action OnTurnOff = null;
        public Action<Scene> OnStartOn = null;
        public Action<Scene> OnStartOff = null;

        public FactoryActivatorComponent() : base(true, true)
        {
        }

        public void Activate(bool lockState = true)
        {
            if (!StateIsLocked)
            {
                bool wasOn = IsOn;
                ActivationCount++;
                HandleOnOff(wasOn);
                StateIsLocked = lockState;
            }
        }

        public void Deactivate(bool lockState = true)
        {
            if (!StateIsLocked)
            {
                bool wasOn = IsOn;
                ActivationCount--;
                HandleOnOff(wasOn);
                StateIsLocked = lockState;
            }
        }

        private void HandleOnOff(bool wasOn)
        {
            if (IsOn != wasOn)
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
        }

        public void Added(Scene scene)
        {
            if (ActivationId == null)
            {
                ActivationCount = 0;
            }
            else
            {
                Level level = scene as Level;
                ActivationCount += level.Session.GetFlag($"FactoryActivation:{ActivationId}") ? 1 : 0;
            }
            if (IsOn)
            {
                OnStartOn?.Invoke(scene);
            }
            else
            {
                OnStartOff?.Invoke(scene);
            }
        }
    }
}
