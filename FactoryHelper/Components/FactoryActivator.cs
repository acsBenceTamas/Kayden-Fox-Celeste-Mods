using Celeste;
using Monocle;
using System;

namespace FactoryHelper.Components
{
    [Tracked]
    public class FactoryActivator : Component
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
        public Action OnStartOn = null;
        public Action OnStartOff = null;

        public FactoryActivator() : base(true, true)
        {
        }

        public void ForceActivate()
        {
            bool wasOn = IsOn;
            StartOn = false;
            ActivationCount = 1;
            HandleOnOff(wasOn);
            StateIsLocked = true;
        }

        public void ForceDeactivate()
        {
            bool wasOn = IsOn;
            StartOn = false;
            ActivationCount = 0;
            HandleOnOff(wasOn);
            StateIsLocked = true;
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

        public void HandleStartup(Scene scene)
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
                OnStartOn?.Invoke();
            }
            else
            {
                OnStartOff?.Invoke();
            }
        }
    }
}
