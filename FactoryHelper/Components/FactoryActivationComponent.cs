using FactoryHelper.Entities;
using Monocle;

namespace FactoryHelper.Components
{
    [Tracked]
    class FactoryActivationComponent : Component
    {
        public string ActivationId { get; private set; }

        public FactoryActivationComponent(bool active, bool visible) : base(active, visible)
        {
        }

        public FactoryActivationComponent(string activationId) : this(false, true)
        {
            ActivationId = activationId;
        }

        public void SetActivationState(bool state)
        {
            Active = state;
            if (Entity is PowerLine)
            {
                (Entity as PowerLine).Activated = state;
            }
            else if (Entity is RustyLamp)
            {
                (Entity as RustyLamp).Activated = state;
            }
        }
    }
}
