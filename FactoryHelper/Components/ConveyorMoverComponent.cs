using FactoryHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryHelper.Components
{
    [Tracked]
    class ConveyorMoverComponent : Component
    {
        public Action<float> OnMove;
        public bool IsOnConveyor = false;

        public ConveyorMoverComponent() : base(true, true)
        {
        }

        public void Move(float amount)
        {
            OnMove?.Invoke(amount);
        }

        public override void Update()
        {
            base.Update();
            bool foundConveyor = false;
            foreach (Conveyor conveyor in Scene.Tracker.GetEntities<Conveyor>())
            {
                if (Collide.Check(conveyor, Entity, conveyor.Position - Vector2.UnitY))
                {
                    foundConveyor = true;
                    Move(conveyor.IsMovingLeft ? -Conveyor.ConveyorMoveSpeed: Conveyor.ConveyorMoveSpeed);
                }
            }
            IsOnConveyor = foundConveyor;
        }
    }
}
