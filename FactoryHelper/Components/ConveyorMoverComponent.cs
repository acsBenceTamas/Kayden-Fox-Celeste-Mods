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
        public Action<float> Move;

        public ConveyorMoverComponent() : base(true, true)
        {
        }
    }
}
