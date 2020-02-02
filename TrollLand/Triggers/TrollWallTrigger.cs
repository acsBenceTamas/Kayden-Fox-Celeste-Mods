using Celeste;
using Microsoft.Xna.Framework;
using TrollLand.Entities;
using System.Linq;
using Celeste.Mod.Entities;

namespace TrollLand.Triggers
{
    [CustomEntity("TrollLand/TrollWallTrigger")]
    public class TrollWallTrigger : Trigger
    {
        private readonly string _idString;

        public TrollWallTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            _idString = data.Attr("idString");
        }

        public override void OnEnter(Player player)
        {
            foreach (TrollWall trollWall in Scene.Tracker.GetEntities<TrollWall>().Where(tw => (tw as TrollWall).IdString == _idString))
            {
                trollWall.StartSequence();
            }
            base.OnEnter(player);
        }
    }
}
