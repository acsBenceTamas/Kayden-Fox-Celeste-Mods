using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using FactoryHelper.Components;
using System.Collections;
using Celeste.Mod.Entities;
using System;

namespace FactoryHelper.Entities
{
    [CustomEntity("FactoryHelper/BoomBox")]
    class BoomBox : Solid
    {
        public ParticleType P_Steam = ParticleTypes.Steam;
        public ParticleType P_SteamAngry = new ParticleType(ParticleTypes.Steam)
        {
            LifeMin = 1f,
            LifeMax = 2f,
            SpeedMin = 12f,
            SpeedMax = 24f
        };

        private class BoomCollider : Entity
        {
            public BoomCollider(Vector2 position) : base(position)
            {
                Collider = new Circle(40f, 0, 0);
            }
        }

        private const float _angryResetTime = 2f;
        private const float _angryShootTime = 0.5f;
        private const float _idlePuffTime = 0.5f;
        private const float _activePuffTime = 0.1f;
        private const float _angryPuffTime = 0.02f;

        private readonly float _initialDelay;
        private readonly Sprite _sprite;
        private readonly Sprite _boomSprite;
        private readonly BoomCollider _boomCollider;
        private readonly SoundSource _sfx;
        private readonly float _startupTime = 1.5f;
        private float _angryResetTimer = 0f;
        private float _angryShootTimer = _angryShootTime;
        private bool _angryMode = false;
        private bool _canGetAngry = false;
        private Coroutine _sequence;
        private bool _steamAnger = false;

        public FactoryActivator Activator { get; }

        public BoomBox(EntityData data, Vector2 offest) : this(data.Position + offest, data.Attr("activationId", ""), data.Float("initialDelay", 0f), data.Bool("startActive", false))
        {
        }

        public BoomBox(Vector2 position, string activationId, float initialDelay, bool startActive) : base(position, 24, 24, false)
        {
            Add(Activator = new FactoryActivator());
            Activator.StartOn = startActive;
            Activator.ActivationId = activationId == string.Empty ? null : activationId;
            Activator.OnStartOn = OnStartOn;
            Activator.OnStartOff = OnStartOff;
            Activator.OnTurnOn = OnTurnOn;
            Activator.OnTurnOff = OnTurnOff;

            Add(new SteamCollider(OnSteamWall));

            _initialDelay = initialDelay;

            Add(_sprite = new Sprite(GFX.Game, "objects/FactoryHelper/boomBox/"));
            _sprite.Add("idle", "idle", 0.2f, "idle");
            _sprite.Add("activating", "activating", 0.2f, "activating");
            _sprite.Add("active", "active", 0.15f, "active");
            _sprite.Add("angry", "angry", 0.05f, "angry");
            _sprite.Add("resetting", "resetting", 0.15f, "active");

            Add(_boomSprite = new Sprite(GFX.Game, "objects/FactoryHelper/boomBox/"));
            _boomSprite.Add("boom", "boom", 0.04f);
            _boomSprite.Color = Color.White * 0.5f;
            _boomSprite.Visible = false;
            _boomSprite.CenterOrigin();
            _boomSprite.Position = new Vector2(Width / 2, Height / 2);

            _boomCollider = new BoomCollider(position + new Vector2(Width / 2, Height / 2));
            Add(_sfx = new SoundSource());
            _sfx.Position = new Vector2(Width / 2, Height / 2);
            Add(new LightOcclude(0.2f));
        }

        private void OnSteamWall(SteamWall obj)
        {
            _steamAnger = true;
            if (Activator.IsOn)
            {
                _canGetAngry = false;
                _angryMode = false;
                _sprite.Play("angry", true);
                Explode();
            }
            Activator.ForceActivate();
        }

        private void OnStartOn()
        {
            _sprite.Play("active", true);
            _canGetAngry = true;
        }

        private void OnStartOff()
        {
            _sprite.Play("idle", true);
            _canGetAngry = false;
        }

        private void OnTurnOn()
        {
            ResetTimers();
            if (_sequence != null)
            {
                Remove(_sequence);
            }
            Add(_sequence = new Coroutine(StartupSequence()));
        }

        private void OnTurnOff()
        {
            ResetTimers();
            if (_sequence != null)
            {
                Remove(_sequence);
            }
            Add(_sequence = new Coroutine(WindDownSequence()));
        }

        private void ResetTimers()
        {
            _angryResetTimer = 0f;
            _angryShootTimer = _angryShootTime;
        }

        private IEnumerator StartupSequence()
        {
            _canGetAngry = false;
            _sprite.Play("activating", true);
            yield return _initialDelay * (_steamAnger ? 0 : 1);
            yield return _startupTime * (_steamAnger ? 0.5 : 1);
            if (_steamAnger)
            {
                _sprite.Play("angry", true);
                yield return _angryShootTime * 0.5;
                Explode();
            }
            else
            {
                _sprite.Play("resetting", true);
                _canGetAngry = true;
            }
        }

        private IEnumerator WindDownSequence()
        {
            yield return _initialDelay;
            _canGetAngry = false;
            _sprite.Play("activating", true);
            yield return _startupTime;
            _sprite.Play("idle", true);
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Add(_boomCollider);
            Activator.HandleStartup(scene);
        }

        public override void Update()
        {
            base.Update();
            if (!_sfx.Playing)
            {
                _sfx.Play("event:/env/local/09_core/conveyor_idle");
            }

            if(!_steamAnger && _canGetAngry)
            {
                HandleAngryMode();
            }
            if(!_canGetAngry && Scene.OnInterval(_idlePuffTime))
            {
                SceneAs<Level>().ParticlesFG.Emit(P_Steam, 1, Center, new Vector2(8f, 8f));
            }
            else if (_canGetAngry && !_angryMode && Scene.OnInterval(_activePuffTime))
            {
                SceneAs<Level>().ParticlesFG.Emit(P_Steam, 1, Center, new Vector2(8f, 8f));
            }
            else if ((_angryMode || _steamAnger) && Scene.OnInterval(_angryPuffTime))
            {
                SceneAs<Level>().ParticlesFG.Emit(P_SteamAngry, 3, Center, new Vector2(8f, 8f), direction:Calc.Random.NextAngle());
            }

            if (_boomSprite.Visible && !_boomSprite.Active)
            {
                _boomSprite.Visible = false;
            }
        }

        private void HandleAngryMode()
        {
            CheckForAngryMode();
            if (_angryMode)
            {
                if (_angryShootTimer > 0)
                {
                    _angryShootTimer -= Engine.DeltaTime;
                }
                else
                {
                    Explode();
                    ResetAngryMode();
                }
            }
            HandleAngryModeResetting();
        }

        private void CheckForAngryMode()
        {
            if (_angryResetTimer <= 0f && !_angryMode && HasPlayerRider())
            {
                _angryMode = true;
                _sprite.Play("angry", true);
            }
        }

        private void HandleAngryModeResetting()
        {
            if (_angryResetTimer > 0f)
            {
                _angryResetTimer -= Engine.DeltaTime;
                if (_angryResetTimer <= 0f)
                {
                    _sprite.Play("resetting", true);
                }
            }
        }

        private void ResetAngryMode()
        {
            _angryMode = false;
            _angryResetTimer = _angryResetTime;
            _angryShootTimer = _angryShootTime;
            _sprite.Play("activating", true);
        }

        private void Explode()
        {
            Audio.Play("event:/new_content/game/10_farewell/puffer_splode", Position);
            _boomSprite.Play("boom");
            _boomSprite.Visible = true;
            (Scene as Level).Displacement.AddBurst(Center, 0.35f, 4f, 64f, 0.5f);
            Player player = Scene.Tracker.GetEntity<Player>();
            Collidable = false;
            if (player != null && player.CollideCheck(_boomCollider) && !Scene.CollideCheck<Solid>(player.Center, Center))
            {
                if (player.Bottom < Top && player.Top > Bottom)
                {
                    player.ExplodeLaunch(Center, false, true);
                }
                else
                {
                    player.ExplodeLaunch(Center, false, false);
                }
            }
            Collidable = true;
        }
    }
}
