using Celeste;
using Celeste.Mod.Entities;
using FactoryHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace FactoryHelper.Triggers
{
    [CustomEntity("FactoryHelper/SteamWallSpeedTrigger")]
    class SteamWallSpeedTrigger : Trigger
    {
        private float speed = 1f;
        public SteamWallSpeedTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            speed = data.Float("speed", defaultValue: 1f);
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
                Level level = Scene as Level;
                SteamWall steamWall = level.Tracker.GetEntity<SteamWall>();
                if (steamWall != null)
                {
                steamWall.Speed = 22f * speed;
                //sorry for the magic number it makes my life easier :p
                }


        }
    }

}