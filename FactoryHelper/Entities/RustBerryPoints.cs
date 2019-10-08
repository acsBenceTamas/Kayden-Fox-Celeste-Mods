using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace FactoryHelper.Entities
{
    [Tracked(false)]
    public class RustBerryPoints : Entity
    {
        private Sprite _sprite;

        private bool _ghostberry;

        private VertexLight _light;

        private BloomPoint _bloom;

        private DisplacementRenderer.Burst _burst;

        public RustBerryPoints(Vector2 position, bool ghostberry)
            : base(position)
        {
            Add(_sprite = FactoryHelperModule.SpriteBank.Create("rustBerry"));
            Add(_light = new VertexLight(Color.RosyBrown, 1f, 16, 24));
            Add(_bloom = new BloomPoint(1f, 12f));
            Depth = -2000100;
            Tag = (Tags.Persistent | Tags.TransitionUpdate | Tags.FrozenUpdate);
            _ghostberry = ghostberry;
        }

        public override void Added(Scene scene)
        {
            _sprite.Play("fade" + (_ghostberry ? "Ghost" : ""));
            _sprite.OnFinish = delegate
            {
                RemoveSelf();
            };
            base.Added(scene);
            foreach (Entity entity in base.Scene.Tracker.GetEntities<RustBerryPoints>())
            {
                if (entity != this && Vector2.DistanceSquared(entity.Position, Position) <= 256f)
                {
                    entity.RemoveSelf();
                }
            }
            _burst = (scene as Level).Displacement.AddBurst(Position, 0.3f, 16f, 24f, 0.3f);
        }

        public override void Update()
        {
            Level level = base.Scene as Level;
            if (level.Frozen)
            {
                if (_burst != null)
                {
                    _burst.AlphaFrom = (_burst.AlphaTo = 0f);
                    _burst.Percent = _burst.Duration;
                }
                return;
            }
            base.Update();
            Camera camera = level.Camera;
            base.Y -= 8f * Engine.DeltaTime;
            base.X = Calc.Clamp(base.X, camera.Left + 8f, camera.Right - 8f);
            base.Y = Calc.Clamp(base.Y, camera.Top + 8f, camera.Bottom - 8f);
            _light.Alpha = Calc.Approach(_light.Alpha, 0f, Engine.DeltaTime * 4f);
            _bloom.Alpha = _light.Alpha;
            ParticleType particleType = _ghostberry ? RustBerry.P_GhostGlow : RustBerry.P_Glow;
            if (Scene.OnInterval(0.06f) && _sprite.CurrentAnimationFrame > 11)
            {
                level.ParticlesFG.Emit(particleType, 1, Position + Vector2.UnitY * -2f, new Vector2(8f, 4f));
            }
        }
    }
}