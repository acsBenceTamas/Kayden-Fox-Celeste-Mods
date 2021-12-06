using System;
using Celeste;
using Celeste.Mod.Entities;
using FactoryHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace FactoryHelper.Triggers
{
    [CustomEntity("FactoryHelper/SpawnSteamWallTrigger")]
    class SpawnSteamWallTrigger : Trigger
    {
        private bool _spawned = false;

        private float speed = 1f;
        private Color overrideColor = new Color(1f, 1f, 1f);
        public SpawnSteamWallTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            speed = data.Float("speed", defaultValue: 1f);
            overrideColor = Calc.HexToColor(data.Attr("color", defaultValue: "ffffff"));
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            if (!_spawned)
            {
                Level level = Scene as Level;
                SteamWall steamWall = level.Tracker.GetEntity<SteamWall>();
                if (steamWall == null)
                {
                    level.Add(new SteamWall(level.Camera.Left - level.Bounds.Left, overrideColor, speed));
                }
                else
                {
                    steamWall.AdvanceToCamera();
                }
                _spawned = true;
            }
        }
    }
}
