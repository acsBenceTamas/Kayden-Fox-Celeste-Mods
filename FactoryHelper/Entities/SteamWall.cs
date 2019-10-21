using Celeste;
using FactoryHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace FactoryHelper.Entities
{
    class SteamWall : Entity
    {
        private const float _speed = 10f;
        private float _delay;
        private bool _canMoveNormally = true;
        private Tween _tween;
        private SoundSource _loopSfx;

        public SteamWall(float startPosition)
        {
            Add(new PlayerCollider(OnPlayer));
            Add(new LightOcclude(0.8f));
            Add(_loopSfx = new SoundSource());
            Depth = -10000;
            Collider = new Hitbox(16 + startPosition, 0f);
        }

        private void OnPlayer(Player player)
        {
            if (SaveData.Instance.Assists.Invincible)
            {
                if (_delay <= 0f)
                {
                    _canMoveNormally = false;
                    float from = Collider.Width;
                    float to = Math.Max(16, (player.Left - Left) - 48f);
                    _tween = Tween.Set(this, Tween.TweenMode.Oneshot, 0.4f, Ease.CubeOut, 
                        delegate (Tween t)
                        {
                            Collider.Width = MathHelper.Lerp(from, to, t.Eased);
                        }, 
                        delegate (Tween t) 
                        {
                            _canMoveNormally = true;
                        });
                    _tween.Start();
                    _delay = 0.5f;
                    Audio.Play("event:/game/general/assist_screenbottom", player.Position);
                }
            }
            else
            {
                player.Die(Vector2.UnitX);
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Level level = Scene as Level;
            Collider.Height = level.Bounds.Height;
            Position = new Vector2(level.Bounds.Left - 16, level.Bounds.Top);
            _loopSfx.Play("event:/game/09_core/rising_threat", "room_state", 0);
            _loopSfx.Position = new Vector2(Width, Height / 2f);
        }

        public override void Update()
        {
            base.Update();
            _delay -= Engine.DeltaTime;
            
            if (_canMoveNormally)
            {
                Collider.Width += _speed * Engine.DeltaTime;
                _loopSfx.Param("rising", 1f);
            }
            foreach(SteamCollider steamCollider in Scene.Tracker.GetComponents<SteamCollider>())
            {
                if (steamCollider.ShouldDoChecks)
                {
                    steamCollider.Check(this);
                }
            }
            _loopSfx.Position.X = Width;
        }

        public override void Render()
        {
            base.Render();
            Level level = (Scene as Level);
            float width = Right - 16 - level.Camera.Left;
            if (width > 0)
            {
                Draw.Rect(level.Camera.Left, Top, width, Height, new Color(Color.DimGray, 0.6f));
            }
            Draw.Rect(Right - 16, Top, 16, Height, new Color(0.99f, 0.99f, 0.99f, 0.8f));
        }
    }
}
