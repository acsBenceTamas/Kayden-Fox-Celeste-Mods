using Celeste;
using FactoryHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace FactoryHelper.Entities
{
    class SteamWall : Entity
    {
        public static ParticleType P_FlyingDebris = new ParticleType
        {
            Color = new Color(0.3f, 0.2f, 0.17f),
            Acceleration = new Vector2(0, 50f),
            LifeMin = 2f,
            LifeMax = 4f,
            Size = 2f,
            SizeRange = 2f,
            Direction = -(float)Math.PI / 4,
            DirectionRange = (float)Math.PI / 16,
            SpeedMin = 50f,
            SpeedMax = 75f,
            FadeMode = ParticleType.FadeModes.Late,
        };
        public ParticleType P_SteamDebris = new ParticleType(ParticleTypes.Steam)
        {
            LifeMin = 1f,
            LifeMax = 2f,
            SpeedMin = 25f,
            SpeedMax = 50f,
            Direction = -(float)Math.PI / 4,
            DirectionRange = (float)Math.PI / 8,
        };

        private const float _speed = 22f;
        private float _delay;
        private bool _canMoveNormally = true;
        private Tween _tween;
        private SoundSource _loopSfx;
        private int[] _steamPoofPoints;
        private float _particleEmittionPeriod;
        private float _baseParticleEmittionPeriod;
        private float _particleEmittionPeriodVariance;

        public SteamWall(float startPosition)
        {
            Add(new PlayerCollider(OnPlayer));
            Add(new LightOcclude(0.8f));
            Add(_loopSfx = new SoundSource());
            Depth = -100000;
            Collider = new Hitbox(16 + startPosition, 0f);
            Add(new Coroutine(SteamPoofSpawnSequence()));
            Add(new Coroutine(ThrowDebrisSequence()));
            Add(new DisplacementRenderHook(RenderDisplacement));
        }

        private void RenderDisplacement()
        {
            Level level = Scene as Level;
            Draw.Rect(color: new Color(0.5f, 0.5f, 0.1f, 1f), x: Left - 5, y: Top - 5, width: Width + 32, height: Height + 10);
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
            Position = new Vector2(level.Bounds.Left - 32, level.Bounds.Top);

            _loopSfx.Play("event:/game/09_core/rising_threat", "room_state", 0);
            _loopSfx.Position = new Vector2(Width, Height / 2f);

            _steamPoofPoints = new int[level.Bounds.Height/12];
            for (int i = 0; i < _steamPoofPoints.Length; i++)
            {
                _steamPoofPoints[i] = (int)Top + 6 + i * 12;
            }
            foreach (int y in _steamPoofPoints)
            {
                SteamPoof.Create(Scene, new Vector2(Right - 4, y), new Vector2(6, 6), 2);
            }

            _baseParticleEmittionPeriod = 150 / Height;
            _particleEmittionPeriodVariance = 50 / Height;
            SetParticleEmittionPeriod();
        }

        private void SetParticleEmittionPeriod()
        {
            _particleEmittionPeriod = Math.Max(_baseParticleEmittionPeriod - _particleEmittionPeriodVariance / 2 + Calc.Random.NextFloat(_particleEmittionPeriodVariance), 0.05f);
        }

        private IEnumerator SteamPoofSpawnSequence()
        {
            int index = 0;
            while (true)
            {
                yield return 0.3f / Height + Calc.Random.NextFloat(0.01f / Height);
                SteamPoof.Create(Scene, new Vector2(Right - 8, _steamPoofPoints[index]), new Vector2(16, 6), 1);
                index = (index + 1) % _steamPoofPoints.Length;
            }
        }

        private IEnumerator ThrowDebrisSequence()
        {
            while (true)
            {
                yield return _particleEmittionPeriod;
                ThrowDebris();
                SetParticleEmittionPeriod();
            }
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

        private void ThrowDebris()
        {
            float y = Calc.Random.NextFloat(Height);
            SceneAs<Level>().ParticlesFG.Emit(P_FlyingDebris, 1, TopRight + new Vector2(0, y), new Vector2(0, 0));
            SceneAs<Level>().ParticlesFG.Emit(P_SteamDebris, 6, TopRight + new Vector2(0, y), new Vector2(0, 2));
        }

        public override void Render()
        {
            base.Render();
            Level level = (Scene as Level);
            float width = Right - 32 - level.Camera.Left;
            DrawGradient(Left, (int)Right - 32, new Color(1f, 1f, 1f) * 1f, new Color(1f, 1f, 1f) * 0.8f);
            DrawGradient((int)Right - 32, (int)Right + 16, new Color(1f, 1f, 1f) * 0.8f, new Color(1f, 1f, 1f) * 0.0f);
        }

        public void DrawGradient(float from, float to, Color colorFrom, Color colorTo)
        {
            for (float x = from; x < to; x ++)
            {
                float percent = (x - from) / (to - from);
                Color columnColor = Color.Lerp(colorFrom, colorTo, Ease.CubeIn(percent));
                Draw.Line(x, Top, x, Bottom, columnColor);
            }
        }
    }
}
