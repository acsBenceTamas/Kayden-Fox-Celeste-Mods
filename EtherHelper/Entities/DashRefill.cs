using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtherHelper.Entities
{
    [CustomEntity("EtherHelper/DashRefill")]
    class DashRefill : Entity
    {
        public static ParticleType P_Shatter = Refill.P_Shatter;
        public static ParticleType P_Regen = Refill.P_Regen;
        public static ParticleType P_Glow = Refill.P_Glow;

        private Sprite _gemSprite;
        private Sprite _shineSprite;
        private Sprite _dangerSprite;
        private Image _outline;
        private Wiggler _wiggler;
        private BloomPoint _bloom;
        private VertexLight _light;
        private SineWave _sine;
        private Level _level;
        private float _respawnTimer;
        private bool _static;
        private Vector2[] _nodes;
        private int _amount;
        private int _index;
        private float _offset;
        private float _mult;
        private float[] _lengths;
        private float _percent;
        private float _speed;
        private Wiggler _hitWiggler;
        private Vector2 _hitDir;
        private bool _broken = false;

        public DashRefill(EntityData data, Vector2 offset) : this(data.NodesWithPosition(offset), data.Int("amount", 1), 0, data.Float("offset", 0f), data.Float("speed", 1f))
        {
        }

        public DashRefill(Vector2[] nodes, int amount, int index, float offset, float speedMult)
        {
            Tag = Tags.TransitionUpdate;
            Collider = new Circle(6f);
            Add(new PlayerCollider(OnPlayer));
            _static = nodes.Length == 1;
            if (!_static)
            {
                _nodes = nodes;
                _amount = amount;
                _index = index;
                _offset = offset;
                _mult = speedMult;

                _lengths = new float[nodes.Length];
                for (int i = 1; i < _lengths.Length; i++)
                {
                    _lengths[i] = _lengths[i - 1] + Vector2.Distance(nodes[i - 1], nodes[i]);
                }
                _speed = 60f / _lengths[_lengths.Length - 1] * _mult;
                if (index == 0)
                {
                    _percent = 0f;
                }
                else
                {
                    _percent = (float)index / (float)amount;
                }
                _percent += 1f / (float)amount * offset;
                _percent %= 1f;
                Position = GetPercentPosition(_percent);
            } else
            {
                Position = nodes[0];
            }

            Add(_outline = new Image(GFX.Game["objects/EtherHelper/dashRefill/outline"]));
            _outline.CenterOrigin();
            _outline.Visible = false;

            Add(_gemSprite = EtherHelperModule.SpriteBank.Create("dash_refill"));
            _gemSprite.Play("gem");
            Add(_shineSprite = EtherHelperModule.SpriteBank.Create("dash_refill"));
            _shineSprite.Play("shine");
            Add(_dangerSprite = EtherHelperModule.SpriteBank.Create("dash_refill"));
            _dangerSprite.Play("danger");

            _shineSprite.OnFinish = delegate
            {
                _shineSprite.Visible = false;
            };

            Add(_wiggler = Wiggler.Create(1f, 4f, delegate (float v)
            {
                _gemSprite.Scale = (_shineSprite.Scale = Vector2.One * (1f + v * 0.2f));
            }));
            Add(new MirrorReflection());
            Add(_bloom = new BloomPoint(0.8f, 24f));
            Add(_light = new VertexLight(Color.White, 1f, 16, 48));
            Add(_sine = new SineWave(0.6f, 0f));
            _sine.Randomize();
            UpdateY();
            Depth = -100;

            Add(_hitWiggler = Wiggler.Create(1.2f, 2f));
            _hitWiggler.StartZero = true;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            _level = SceneAs<Level>();
            if (_index == 0 && !_static)
            {
                for (int i = 1; i < _amount; i++)
                {
                    Scene.Add(new DashRefill(_nodes, _amount, i, _offset, _mult));
                }
            }
        }

        public override void Update()
        {
            base.Update();
            if (_respawnTimer > 0f)
            {
                _respawnTimer -= Engine.DeltaTime;
                if (_respawnTimer <= 0f)
                {
                    Add(new Coroutine(Respawn()));
                }
            }
            else if (!_broken && Scene.OnInterval(0.1f))
            {
                _level.ParticlesFG.Emit(P_Glow, 1, Position, Vector2.One * 8f);
            }
            if (_gemSprite.Visible && Scene.OnInterval(2.0f))
            {
                _shineSprite.Play("shine", restart: true);
                _shineSprite.Visible = true;
            }
            UpdateY();
            _light.Alpha = Calc.Approach(_light.Alpha, _gemSprite.Visible ? 1f : 0f, 4f * Engine.DeltaTime);
            _bloom.Alpha = _light.Alpha * 0.8f;

            if (!_static)
            {
                _percent += _speed * _mult * Engine.DeltaTime;
                if (_percent >= 1f)
                {
                    _percent %= 1f;
                    if (_broken && _nodes[_nodes.Length - 1] != _nodes[0])
                    {
                        _broken = false;
                        Collidable = true;
                        _gemSprite.Visible = _shineSprite.Visible = _dangerSprite.Visible = true;
                    }
                }
                Position = GetPercentPosition(_percent);
            }
        }

        public override void Render()
        {
            if (_gemSprite.Visible)
            {
                _gemSprite.DrawOutline();
            }
            base.Render();
        }

        private void UpdateY()
        {
            if (_broken)
            {
                Collider.Position = _dangerSprite.Position = _shineSprite.Position = _gemSprite.Position = Vector2.Zero;
                return;
            }
            Vector2 pos = Vector2.Zero;
            if (_hitWiggler != null)
            {
                pos = _hitDir * _hitWiggler.Value * 8f;
            }
            float deltaPosY = _bloom.Y = _sine.Value * 1f;
            Collider.Position = _dangerSprite.Position = _shineSprite.Position = _gemSprite.Position = deltaPosY * Vector2.UnitY + pos;
        }

        private IEnumerator RefillRoutine(Player player)
        {
            Celeste.Celeste.Freeze(0.05f);
            yield return null;
            _level.Shake();
            _dangerSprite.Visible = _gemSprite.Visible = _shineSprite.Visible = false;
            Depth = 8999;
            if (_static)
            {
                _outline.Visible = true;
            }
            _broken = true;
            yield return 0.05f;
            float angle = player.Speed.Angle();
            _level.ParticlesFG.Emit(P_Shatter, 5, Position, Vector2.One * 8f, angle - (float)Math.PI / 2f);
            _level.ParticlesFG.Emit(P_Shatter, 5, Position, Vector2.One * 8f, angle + (float)Math.PI / 2f);
            SlashFx.Burst(Position, angle);
        }

        private IEnumerator Respawn()
        {
            while (true)
            {
                Collidable = true;
                if (!CollideCheck<Player>())
                {
                    break;
                } else
                {
                    Collidable = false;
                    yield return null;
                }
            }
            if (_broken)
            {
                _gemSprite.Visible = _shineSprite.Visible = _dangerSprite.Visible = true;
                _outline.Visible = false;
                Depth = -100;
                _wiggler.Start();
                Audio.Play("event:/game/general/diamond_return", Position);
                _level.ParticlesFG.Emit(P_Regen, 16, Position, Vector2.One * 4f);
                _broken = false;
            }
        }

        private Vector2 GetPercentPosition(float percent)
        {
            if (percent <= 0f)
            {
                return _nodes[0];
            }
            if (percent >= 1f)
            {
                return _nodes[_nodes.Length - 1];
            }
            float length = _lengths[_lengths.Length - 1];
            float step = length * percent;
            int i;
            for (i = 0; i < _lengths.Length - 1 && !(_lengths[i + 1] > step); i++)
            {
            }
            if (i == _lengths.Length - 1)
            {
                return _nodes[0];
            }
            float min = _lengths[i] / length;
            float max = _lengths[i + 1] / length;
            float pos = Calc.ClampedMap(percent, min, max);
            return Vector2.Lerp(_nodes[i], _nodes[i + 1], pos);
        }

        private void OnPlayer(Player player)
        {
            if (player.DashAttacking)
            {
                player.RefillDash();
                Audio.Play("event:/game/general/diamond_touch", Position);
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                Collidable = false;
                Add(new Coroutine(RefillRoutine(player)));
                if (_static)
                {
                    _respawnTimer = 2.5f;
                }
            }
            else
            {
                KillPlayer(player);
            }
        }
        private void KillPlayer(Player player)
        {
            Vector2 direction = (player.Center - base.Center).SafeNormalize();
            if (player.Die(direction) != null)
            {
                _hitDir = direction;
                _hitWiggler.Start();
            }
        }
    }
}
