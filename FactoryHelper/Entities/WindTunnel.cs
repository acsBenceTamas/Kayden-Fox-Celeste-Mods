using Celeste;
using Celeste.Mod.Entities;
using FactoryHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace FactoryHelper.Entities
{
    [CustomEntity("FactoryHelper/WindTunnel")]
    public class WindTunnel : Entity
    {
        public FactoryActivator Activator { get; }

        private class AmbienceTrigger : Trigger
        {
            private const int _hearingDistance = 64;
            private WindTunnel _parent;

            public AmbienceTrigger(EntityData data, Vector2 offset, WindTunnel parent) : base(data, offset)
            {
                _parent = parent;
                Collider.Width += _hearingDistance;
                Collider.Height += _hearingDistance;
                Position -= new Vector2(_hearingDistance/2, _hearingDistance/2);
            }

            public override void OnStay(Player player)
            {
                base.OnEnter(player);
                //_parent.SetAmbience();
            }

            public override void OnLeave(Player player)
            {
                base.OnLeave(player);
                //_parent.SetAmbience(false);
            }
        }

        private struct Particle
        {
            public Vector2 Position;

            public float Percent;

            public float Duration;

            public Vector2 Direction;

            public float Speed;

            public float Spin;

            public int Color;
        }

        private static readonly float _baseAlpha = 0.7f;
        private float _windUpTime = 0.5f;
        private float _windDownTime = 0.2f;
        private static readonly Color[] _colors = new Color[3]
            {
                Calc.HexToColor("808080"),
                Calc.HexToColor("545151"),
                Calc.HexToColor("ada5a5")
            };
        
        private Dictionary<WindMover, float> _componentPercentages = new Dictionary<WindMover, float>();
        private Vector2 _scale = Vector2.One;
        private float _percent;
        private bool _speedingUp;
        private float _loopWidth;
        private float _loopHeight;
        private float _strength;
        private Direction _direction;
        private Particle[] _particles;
        private Vector2 _defaultWindSpeed;
        private Vector2 _actualWindSpeed
        {
            get
            {
                return _defaultWindSpeed * _percent;
            }
        }
        private AmbienceTrigger _ambienceTrigger;

        private enum Direction
        {
            Up,
            Down,
            Left,
            Right
        }

        public WindTunnel(EntityData data, Vector2 offset) : 
            this(data.Position + offset,
                data.Width,
                data.Height,
                data.Float("strength", 1f),
                data.Attr("direction", "Up"),
                data.Attr("activationId", ""),
                data.Bool("startActive", false))
        {
            _ambienceTrigger = new AmbienceTrigger(data, offset, this);
        }

        public WindTunnel(Vector2 position, int width, int height, float strength, string direction, string activationId, bool startActive)
        {
            Depth = -1000;
            Position = position;
            _loopWidth = width;
            _loopHeight = height;

            Add(Activator = new FactoryActivator());
            Activator.ActivationId = activationId == string.Empty ? null : activationId;
            Activator.StartOn = startActive;
            Activator.OnTurnOn = () =>
            {
                _speedingUp = true;
            };
            Activator.OnTurnOff = () =>
            {
                _speedingUp = false;
            };
            Activator.OnStartOff = () =>
            {
                _percent = 0f;
                _speedingUp = false;
            };
            Activator.OnStartOn = () =>
            {
                _percent = 1f;
                _speedingUp = true;
            };

            Add(new SteamCollider(OnSteamWall));

            Collider = new Hitbox(width, height);
            _strength = strength;
            Enum.TryParse(direction, out _direction);

            switch (_direction)
            {
                case Direction.Up:
                    _defaultWindSpeed = -Vector2.UnitY * _strength;
                    break;
                case Direction.Down:
                    _defaultWindSpeed = Vector2.UnitY * _strength;
                    break;
                case Direction.Left:
                    _defaultWindSpeed = -Vector2.UnitX * _strength;
                    break;
                case Direction.Right:
                    _defaultWindSpeed = Vector2.UnitX * _strength;
                    break;
            }

            int particlecount = width * height / 300;

            _particles = new Particle[particlecount];
            for (int i = 0; i < _particles.Length; i++)
            {
                Reset(i, Calc.Random.NextFloat(_baseAlpha));
            }
        }

        private void OnSteamWall(SteamWall obj)
        {
            Activator.ForceDeactivate();
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Activator.HandleStartup(scene);
            PositionParticles();
            Scene.Add(_ambienceTrigger);
        }

        public override void Update()
        {
            if (_speedingUp && (_percent < 1f))
            {
                _percent = Calc.Approach(_percent, 1f, Engine.DeltaTime / 1f);
            }
            else if (!_speedingUp && (_percent > 0f))
            {
                _percent = Calc.Approach(_percent, 0f, Engine.DeltaTime / 1.5f);
            }
            PositionParticles();
            foreach (WindMover component in Scene.Tracker.GetComponents<WindMover>())
            {
                if (component.Entity.CollideCheck(this))
                {
                    if (_componentPercentages.ContainsKey(component))
                    {
                        _componentPercentages[component] = Calc.Approach(_componentPercentages[component], 1f, Engine.DeltaTime / _windUpTime);
                    }
                    else
                    {
                        _componentPercentages.Add(component, 0f);
                    }
                }
                else
                {
                    if (_componentPercentages.ContainsKey(component))
                    {
                        _componentPercentages[component] = Calc.Approach(_componentPercentages[component], 0.0f, Engine.DeltaTime / _windDownTime);
                        if (_componentPercentages[component] == 0f)
                        {
                            _componentPercentages.Remove(component);
                        }
                    }
                }
            }
            foreach (WindMover component in _componentPercentages.Keys)
            {
                if (component != null && component.Entity != null && component.Entity.Scene != null)
                {
                    component.Move(_actualWindSpeed * 0.1f * Engine.DeltaTime * Ease.CubeInOut(_componentPercentages[component]));
                }
            }
            base.Update();
        }

        private void SetAmbience(bool turnOn = true)
        {
            Vector2 considered = turnOn ? _actualWindSpeed : Vector2.Zero;
            int direction = 0;
            if (considered.X != 0f)
            {
                direction = Math.Sign(considered.X);
            }
            else if (considered.Y != 0f)
            {
                direction = Math.Sign(considered.Y);
            }
            Audio.SetParameter(Audio.CurrentAmbienceEventInstance, "wind_direction", direction);
            Audio.SetParameter(Audio.CurrentAmbienceEventInstance, "strong_wind", 0);
        }

        public override void Render()
        {
            for (int i = 0; i < _particles.Length; i++)
            {
                Vector2 particlePosition = default(Vector2);
                particlePosition.X = mod(_particles[i].Position.X, _loopWidth);
                particlePosition.Y = mod(_particles[i].Position.Y, _loopHeight);
                float percent = _particles[i].Percent;
                float num = 0f;
                num = ((!(percent < 0.7f)) ? Calc.ClampedMap(percent, 0.7f, 1f, 1f, 0f) : Calc.ClampedMap(percent, 0f, 0.3f));
                Draw.Rect(particlePosition + this.Position, _scale.X, _scale.Y, _colors[_particles[i].Color] * num);
            }
        }

        private void PositionParticles()
        {
            bool flag = _actualWindSpeed.Y == 0f;
            Vector2 zero = Vector2.Zero;
            if (flag)
            {
                _scale.X = Math.Max(1f, Math.Abs(_actualWindSpeed.X) / 40f);
                _scale.Y = 1f;
                zero = new Vector2(_actualWindSpeed.X, 0f);
            }
            else
            {
                float divisor = _direction == Direction.Down ? 20f : 100f;
                _scale.X = 1f;
                _scale.Y = Math.Max(1f, Math.Abs(_actualWindSpeed.Y) / divisor);
                zero = new Vector2(0f, _actualWindSpeed.Y * 2f);
            }
            for (int i = 0; i < _particles.Length; i++)
            {
                if (_particles[i].Percent >= 1f)
                {
                    Reset(i, 0f);
                }
                float divisor;
                switch (_direction)
                {
                    case Direction.Up:
                        divisor = 4f;
                        break;
                    case Direction.Down:
                        divisor = 1.5f;
                        break;
                    default:
                        divisor = 1f;
                        break;
                }
                _particles[i].Percent += Engine.DeltaTime / _particles[i].Duration;
                _particles[i].Position += (_particles[i].Direction * _particles[i].Speed + zero / divisor) * Engine.DeltaTime;
                _particles[i].Direction.Rotate(_particles[i].Spin * Engine.DeltaTime);
            }
        }

        private float mod(float x, float m)
        {
            return (x % m + m) % m;
        }

        private void Reset(int i, float p)
        {
            _particles[i].Percent = p;
            _particles[i].Position = new Vector2(Calc.Random.Range(0, _loopWidth), Calc.Random.Range(0, _loopHeight));
            _particles[i].Speed = Calc.Random.Range(4, 14);
            _particles[i].Spin = Calc.Random.Range(0.25f, (float)Math.PI * 6f);
            _particles[i].Duration = Calc.Random.Range(1f, 4f);
            _particles[i].Direction = Calc.AngleToVector(Calc.Random.NextFloat((float)Math.PI * 2f), 1f);
            _particles[i].Color = Calc.Random.Next(_colors.Length);
        }
    }
}
