using Celeste;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Monocle;

namespace FactoryHelper.Entities
{
    class BoomBox : Solid
    {
        public bool Activated
        {
            get
            {
                if (_activationId == null)
                {
                    return true;
                }
                else
                {
                    Level level = Scene as Level;
                    return level.Session.GetFlag(_activationId) || level.Session.GetFlag("Persistent" + _activationId);
                }
            }
        }
        private class BoomCollider : Entity
        {
            public BoomCollider(Vector2 position) : base(position)
            {
                Collider = new Circle(32f, 0, 0);
            }
        }

        private const float _angryResetTime = 2f;
        private const float _angryShootTime = 0.5f;

        private string _activationId;
        private float _initialDelay;
        private bool _startActive;
        private bool _activatedEarlier;
        private float _startupTime = 1.5f;
        private bool _startupStarted = false;
        private bool _startupFinished = false;
        private Sprite _sprite;
        private float _angryResetTimer = _angryResetTime;
        private float _angryShootTimer = _angryShootTime;
        private bool _angryMode = false;
        private bool _angryModeResetting = false;
        private Sprite _boomSprite;
        private BoomCollider _boomCollider;

        public BoomBox(EntityData data, Vector2 offest) : this(data.Position + offest, data.Attr("activationId", ""), data.Float("initialDelay", 0f), data.Bool("startActive", false))
        {
        }

        public BoomBox(Vector2 position, string activationId, float initialDelay, bool startActive) : base(position, 24, 24, true)
        {
            _activationId = activationId == string.Empty ? null : $"FactoryActivation:{activationId}";
            _startActive = _activationId == null ? !startActive : startActive;
            _initialDelay = initialDelay;
            Add(_sprite = new Sprite(GFX.Game, "objects/FactoryHelper/boomBox/"));
            _sprite.Add("idle", "idle", 0.4f, "idle");
            _sprite.Add("activating", "activating", _angryResetTime/8, "activating");
            _sprite.Add("active", "active", 0.25f, "active");
            _sprite.Add("angry", "active", 0.1f, "angry");

            Add(_boomSprite = new Sprite(GFX.Game, "objects/FactoryHelper/boomBox/"));
            _boomSprite.Add("boom", "boom", 0.1f);
            _boomSprite.Color = new Color(Color.White, 0.5f);
            _boomSprite.Visible = false;
            _boomSprite.CenterOrigin();
            _boomSprite.Position = new Vector2(Width / 2, Height / 2);

            _boomCollider = new BoomCollider(position + new Vector2(Width / 2, Height / 2));
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            _activatedEarlier = Activated;
            if (Activated != _startActive)
            {
                _sprite.Play("active", true);
            }
            else
            {
                _sprite.Play("idle", true);
            }
            if (_activatedEarlier)
            {
                _startupStarted = true;
                _startupFinished = true;
            }
            scene.Add(_boomCollider);
        }

        public override void Update()
        {
            base.Update();
            if ((!_activatedEarlier == Activated) && !_startupFinished)
            {
                if (!_startupStarted)
                {
                    Console.WriteLine("Startup Sequence Start");
                    _sprite.Play("activating", true);
                    _startupStarted = true;
                    if (_startActive)
                    {
                        _sprite.Rate = -1f;
                    }
                }
                if (_startupTime > 0f)
                {
                    _startupTime -= Engine.DeltaTime;
                }
                else if (!_startActive)
                {
                    Console.WriteLine("Startup Sequence Finished");
                    _sprite.Play("active", true);
                    _startupFinished = true;
                }
                else
                {
                    Console.WriteLine("Startup Sequence Finished");
                    _sprite.Play("idle", true);
                    _startupFinished = true;
                    _sprite.Rate = 1f;
                }
            }

            if (Activated != _startActive)
            {
                if (!_startActive && _angryModeResetting && _startupFinished)
                {
                    if (_angryResetTimer > 0)
                    {
                        _angryResetTimer -= Engine.DeltaTime;
                    }
                    else
                    {
                        _angryModeResetting = false;
                        _sprite.Play("active", true);
                    }
                }
                if (!_angryModeResetting && !_angryMode && HasPlayerRider())
                {
                    _angryMode = true;
                    _sprite.Play("angry", true);
                }
                if (_angryMode)
                {
                    if (_angryShootTimer > 0)
                    {
                        _angryShootTimer -= Engine.DeltaTime;
                    }
                    else
                    {
                        Audio.Play("event:/new_content/game/10_farewell/puffer_splode", Position);
                        _angryModeResetting = true;
                        _angryMode = false;
                        _angryResetTimer = _angryResetTime;
                        _angryShootTimer = _angryShootTime;
                        _sprite.Play("activating", true);
                        _boomSprite.Play("boom");
                        _boomSprite.Visible = true;
                        Player player = Scene.Tracker.GetEntity<Player>();
                        if (player != null && player.CollideCheck(_boomCollider))
                        {
                            if (player.Bottom < Top && player.Top > Bottom)
                            {
                                player.ExplodeLaunch(Center, false, true);
                            } else
                            {
                                player.ExplodeLaunch(Center, false, false);
                            }
                        }
                    }
                }
                if (_boomSprite.Visible && !_boomSprite.Active)
                {
                    _boomSprite.Visible = false;
                }
            }
        }
    }
}
