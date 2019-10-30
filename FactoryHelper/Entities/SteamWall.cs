using Celeste;
using FactoryHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace FactoryHelper.Entities
{
    [Tracked(false)]
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

        public bool Halted = false;
        public float Speed = 22f;
        public float Fade = 1f;

        private float _delay;
        private bool _canMoveNormally = true;
        private Tween _tween;
        private SoundSource _loopSfx;
        private int[] _steamPoofPoints;
        private float _particleEmittionPeriod;
        private float _baseParticleEmittionPeriod;
        private float _particleEmittionPeriodVariance;
        private float _transitionFade = 1f;
        private TransitionListener _transitionListener;
        private List<SteamPoof> _steamPoofs = new List<SteamPoof>();

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
            Add(_transitionListener = new TransitionListener());
            _transitionListener.OnOut += FadeOutOnTransition;
        }

        public void AdvanceToCamera()
        {
            Level level = Scene as Level;
            Collider.Width = Math.Max(level.Camera.Left - level.Bounds.Left, Collider.Width);
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
                            AdvanceWall(MathHelper.Lerp(from, to, t.Eased));
                        }, 
                        delegate (Tween t) 
                        {
                            _canMoveNormally = true;
                        });
                    _tween.Start();
                    _delay = 0.3f;
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

        public override void Removed(Scene scene)
        {
            foreach (SteamPoof poof in _steamPoofs)
            {
                scene.Remove(poof);
            }
            base.Removed(scene);
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
                _steamPoofs.AddRange(SteamPoof.Create(Scene, new Vector2(Right - 8, _steamPoofPoints[index]), new Vector2(16, 6), 1, Fade, RemovePoof));
                index = (index + 1) % _steamPoofPoints.Length;
            }
        }

        private void RemovePoof(SteamPoof poof)
        {
            _steamPoofs.Remove(poof);
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
            
            if (_canMoveNormally && !Halted)
            {
                AdvanceWall(Collider.Width + Speed * Engine.DeltaTime);
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

        private void AdvanceWall(float to)
        {

            float widthBefore = Collider.Width;
            Collider.Width = to;
            foreach (SteamPoof poof in _steamPoofs)
            {
                poof.Position.X += (Collider.Width - widthBefore);
            }
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
            DrawGradient(Left, (int)Right - 32, new Color(1f, 1f, 1f) * 1f, new Color(1f, 1f, 1f) * 0.8f);
            DrawGradient((int)Right - 32, (int)Right + 16, new Color(1f, 1f, 1f) * 0.8f, new Color(1f, 1f, 1f) * 0.0f);
        }

        private void FadeOutOnTransition(float transition)
        {
            _transitionFade = 1f - transition;
        }

        public void DrawGradient(float from, float to, Color colorFrom, Color colorTo)
        {
            Level level = Scene as Level;
            for (float x = from; x < to; x ++)
            {
                float percent = (x - from) / (to - from);
                Color columnColor = Color.Lerp(colorFrom, colorTo, Ease.CubeIn(percent)) * _transitionFade * Fade;
                Draw.Line(x, level.Camera.Top, x, level.Camera.Bottom, columnColor);
            }
        }
    }
}
