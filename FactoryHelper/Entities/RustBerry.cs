using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace FactoryHelper.Entities
{
    [CustomEntity("FactoryHelper/RustBerry")]
    class RustBerry : Entity
    {
        public static ParticleType P_Glow = new ParticleType(Strawberry.P_Glow) { Color = Calc.HexToColor("fc5a03"), Color2 = Calc.HexToColor("9e4210") };
        public static ParticleType P_GhostGlow = Strawberry.P_GhostGlow;
        public EntityID ID;
        public Follower Follower;
        public bool ReturnHomeWhenLost = true;

        private Sprite _sprite;
        private Sprite _gearSprite;
        private Wiggler _wiggler;
        private Wiggler _rotateWiggler;
        private BloomPoint _bloom;
        private VertexLight _light;
        private Tween _lightTween;
        private float _wobble = 0f;
        private Vector2 _start;
        private float _collectTimer = 0f;
        private bool _collected = false;
        private bool _isGhostBerry;

        public RustBerry(EntityData data, Vector2 offset, EntityID gid)
        {
            ID = gid;
            Position = (_start = data.Position + offset);
            _isGhostBerry = CheckRustBerry();
            Depth = -100;
            Collider = new Hitbox(14f, 14f, -7f, -7f);
            Add(new PlayerCollider(OnPlayer));
            Add(new MirrorReflection());
            Add(Follower = new Follower(ID, null, OnLoseLeader));
            Add(_sprite = FactoryHelperModule.SpriteBank.Create("rustBerry"));
            _sprite.OnFrameChange = OnAnimate;
            Add(_gearSprite = FactoryHelperModule.SpriteBank.Create("rustBerryGear"));
            Follower.FollowDelay = 0.3f;
            if (!_isGhostBerry)
            {
                _sprite.Play("idle");
                _gearSprite.Play("idle");
            } else
            {
                _sprite.Play("idleGhost");
                _gearSprite.Play("idleGhost");
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Add(_wiggler = Wiggler.Create(0.4f, 4f, delegate (float v)
            {
                _sprite.Scale = Vector2.One * (1f + v * 0.35f);
            }));
            Add(_rotateWiggler = Wiggler.Create(0.5f, 4f, delegate (float v)
            {
                _sprite.Rotation = v * 30f * ((float)Math.PI / 180f);
            }));
            Add(_bloom = new BloomPoint(_isGhostBerry ? 0.5f : 1f, 12f));
            Add(_light = new VertexLight(Color.RosyBrown, 1f, 16, 24));
            Add(_lightTween = _light.CreatePulseTween());
            if ((scene as Level).Session.BloomBaseAdd > 0.1f)
            {
                _bloom.Alpha *= 0.5f;
            }
        }

        public override void Update()
        {
            if (!_collected)
            {
                _wobble += Engine.DeltaTime * 4f;
                Sprite obj = _sprite;
                BloomPoint bloomPoint = _bloom;
                float num2 = _light.Y = (float)Math.Sin(_wobble) * 2f;
                float num5 = obj.Y = (bloomPoint.Y = num2);
                int followIndex = Follower.FollowIndex;
                if (Follower.Leader != null && Follower.DelayTimer <= 0f)
                {
                    Player player = Follower.Leader.Entity as Player;
                    bool canCollect = false;
                    if (player != null && player.Scene != null && !player.StrawberriesBlocked)
                    {
                        if (player.OnSafeGround && player.StateMachine.State != 13)
                        {
                            canCollect = true;
                        }
                    }
                    if (canCollect)
                    {
                        _collectTimer += Engine.DeltaTime;
                        if (_collectTimer > 0.15f)
                        {
                            OnCollect();
                        }
                    }
                    else
                    {
                        _collectTimer = Math.Min(_collectTimer, 0f);
                    }
                }
                else
                {
                    if (followIndex > 0)
                    {
                        _collectTimer = -0.15f;
                    }
                }
            }
            base.Update();
            if (Follower.Leader != null && base.Scene.OnInterval(0.08f))
            {
                ParticleType type = _isGhostBerry ? P_GhostGlow : P_Glow;
                SceneAs<Level>().ParticlesFG.Emit(type, Position + Calc.Random.Range(-Vector2.One * 6f, Vector2.One * 6f));
            }
        }

        private void OnAnimate(string id)
        {
            int num = 35;
            if (_sprite.CurrentAnimationFrame == num)
            {
                _lightTween.Start();
                if (!_collected && (CollideCheck<FakeWall>() || CollideCheck<Solid>()))
                {
                    Audio.Play("event:/game/general/strawberry_pulse", Position);
                    SceneAs<Level>().Displacement.AddBurst(Position, 0.6f, 4f, 28f, 0.1f);
                }
                else
                {
                    Audio.Play("event:/game/general/strawberry_pulse", Position);
                    SceneAs<Level>().Displacement.AddBurst(Position, 0.6f, 4f, 28f, 0.2f);
                }
            }
        }

        public void OnPlayer(Player player)
        {
            if (Follower.Leader == null && !_collected)
            {
                Audio.Play(_isGhostBerry ? "event:/game/general/strawberry_blue_touch" : "event:/game/general/strawberry_touch", Position);
                player.Leader.GainFollower(Follower);
                _wiggler.Start();
                _sprite.Play("noFlash" + (_isGhostBerry ? "Ghost": ""));
                base.Depth = -1000000;
            }
        }

        public void OnCollect()
        {
            if (!_collected)
            {
                int collectIndex = 0;
                _collected = true;
                if (Follower.Leader != null)
                {
                    Player player = Follower.Leader.Entity as Player;
                    collectIndex = player.StrawberryCollectIndex;
                    player.StrawberryCollectIndex++;
                    player.StrawberryCollectResetTimer = 2.5f;
                    Follower.Leader.LoseFollower(Follower);
                }
                SaveData.Instance.AddStrawberry(ID, false);
                RegisterCollected();
                Session session = (base.Scene as Level).Session;
                session.DoNotLoad.Add(ID);
                session.UpdateLevelStartDashes();
                Add(new Coroutine(CollectRoutine()));
            }
        }

        private bool CheckRustBerry()
        {
            return (FactoryHelperModule.Instance._SaveData as FactoryHelperSaveData).RustBerries.Contains(ID);
        }

        private void RegisterCollected()
        {
            (FactoryHelperModule.Instance._SaveData as FactoryHelperSaveData).RustBerries.Add(ID);
        }

        private IEnumerator CollectRoutine()
        {
            bool flag = Scene is Level;
            Tag = Tags.TransitionUpdate;
            Depth = -2000010;
            int color = 3;
            if (_isGhostBerry)
            {
                color = 1;
            }
            Audio.Play("event:/game/general/strawberry_get", Position, "colour", color);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            _sprite.Play("collect" + (_isGhostBerry ? "Ghost" : ""));
            _gearSprite.Play("collect" + (_isGhostBerry ? "Ghost" : ""));
            while (_sprite.Animating)
            {
                yield return null;
            }
            Scene.Add(new RustBerryPoints(Position, _isGhostBerry));
            RemoveSelf();
        }

        private void OnLoseLeader()
        {
            if (!_collected && ReturnHomeWhenLost)
            {
                Alarm.Set(this, 0.15f, delegate
                {
                    RustBerry strawberry = this;
                    Vector2 vector = (_start - Position).SafeNormalize();
                    float num = Vector2.Distance(Position, _start);
                    float scaleFactor = Calc.ClampedMap(num, 16f, 120f, 16f, 96f);
                    Vector2 control = _start + vector * 16f + vector.Perpendicular() * scaleFactor * Calc.Random.Choose(1, -1);
                    SimpleCurve curve = new SimpleCurve(Position, _start, control);
                    Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineOut, MathHelper.Max(num / 100f, 0.4f), start: true);
                    tween.OnUpdate = delegate (Tween f)
                    {
                        strawberry.Position = curve.GetPoint(f.Eased);
                    };
                    tween.OnComplete = delegate
                    {
                        Depth = 0;
                    };
                    Add(tween);
                });
            }
        }
    }
}
