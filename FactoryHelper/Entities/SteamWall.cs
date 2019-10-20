using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace FactoryHelper.Entities
{
    class SteamWall : Entity
    {
        private const float _speed = 10f;
        private float _delay;

        public SteamWall()
        {
            Add(new PlayerCollider(OnPlayer));
        }

        private void OnPlayer(Player player)
        {
            if (SaveData.Instance.Assists.Invincible)
            {
                if (_delay <= 0f)
                {
                    float from = Width;
                    float to = Math.Min(0, Width - 48f);
                    Tween.Set(this, Tween.TweenMode.Oneshot, 0.4f, Ease.CubeOut, delegate (Tween t)
                    {
                        Collider.Width = MathHelper.Lerp(from, to, t.Eased);
                    });
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
            Collider = new Hitbox(16, level.Bounds.Height);
            Position = new Vector2(level.Bounds.Left - 16, level.Bounds.Top);
        }

        public override void Update()
        {
            if (_delay > 0f)
            {
                _delay -= Engine.DeltaTime;
            }
            else
            {
                Collider.Width += _speed * Engine.DeltaTime;
            }
        }

        public override void Render()
        {
            base.Render();
            Level level = (Scene as Level);
            float width = Right - 16 - level.Camera.Left;
            if (width > 0)
            {
                Draw.Rect(level.Camera.Left, Top, width, Height, Color.DimGray);
            }
            Draw.Rect(Right - 16, Top, 16, Height, Color.White);
        }
    }
}
