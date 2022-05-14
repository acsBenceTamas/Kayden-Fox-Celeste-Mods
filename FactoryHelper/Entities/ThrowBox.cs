using Celeste;
using Celeste.Mod.Entities;
using FactoryHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace FactoryHelper.Entities
{
    [CustomEntity("FactoryHelper/ThrowBox")]
    public class ThrowBox : Actor
    {
        public Vector2 Speed;
        public Holdable Hold;
        public ConveyorMover ConveyorMover;
        public Action OnRemoved;
        public bool IsSpecial;

        private const float HORIZONTAL_BREAK_SPEED = 180f;
        private const float VERTICAL_BREAK_SPEED = 300f;
        private const float CONVEYOR_ACCELERATION = 200f;
        private const float SOUND_RELOAD_TIME = 0.2f;
        private static readonly Vector2 DISPLACEMENT = new Vector2(-8f, -16f);

        private Image _image;
        private Image _warningImage;
        private float _noGravityTimer;
        private Vector2 _prevLiftSpeed;
        private Level _level;
        private bool _shattered;
        private bool _isMetal;
        private float _soundTimerX = 0f;
        private float _soundTimerY = 0f;
        private string _levelName;
        private bool _pickedUp;
        private Player _player;
        private ParticleEmitter _shimmerParticles;
        private Vector2 _starterPosition;
        private bool _unspecializeOnRemove = true;
        private bool _tutorial;
        private BirdTutorialGui _tutorialCarry;
        private BirdTutorialGui _tutorialPutDown;
        private bool _isCrucial;
        private TransitionListener _transitionListener;

        public static ParticleType P_Impact { get; } = new ParticleType
        {
            Color = Calc.HexToColor("9c8d7b"),
            Size = 1f,
            FadeMode = ParticleType.FadeModes.Late,
            DirectionRange = 1.74532926f,
            SpeedMin = 10f,
            SpeedMax = 20f,
            SpeedMultiplier = 0.1f,
            LifeMin = 0.3f,
            LifeMax = 0.8f
        };

        public ThrowBox(EntityData data, Vector2 offset) : this(data.Position + offset, data.Bool("isMetal", false), data.Bool("tutorial", false), data.Bool("isSpecial", false), data.Bool("isCrucial", false))
        {
            _levelName = data.Level.Name;
        }

        public ThrowBox(Vector2 position, bool isMetal, bool tutorial = false, bool isSpecial = false, bool isCrucial = false) : base(position)
        {
            Position -= DISPLACEMENT;
            _starterPosition = Position;
            Depth = 100;
            Collider = new Hitbox(8f, 10f, 4f + DISPLACEMENT.X, 6f + DISPLACEMENT.Y);
            _isMetal = isMetal;
            IsSpecial = isSpecial;
            _isCrucial = isCrucial;
            _tutorial = tutorial; 
            string pathString = isMetal ? "crate_metal0" : "crate0";

            Add(_image = new Image(GFX.Game[$"objects/FactoryHelper/crate/{pathString}"]));
            _image.Position += DISPLACEMENT;

            if (_isCrucial)
            {
                Add(_warningImage = new Image(GFX.Game["objects/FactoryHelper/crate/crucial"]));
                _warningImage.Position += DISPLACEMENT;
            }

            Add(Hold = new Holdable(0.1f));
            Hold.PickupCollider = new Hitbox(16f, 16f, DISPLACEMENT.X, DISPLACEMENT.Y);
            Hold.SlowFall = false;
            Hold.SlowRun = true;
            Hold.OnPickup = OnPickup;
            Hold.OnRelease = OnRelease;
            Hold.OnHitSpring = HitSpring;
            Hold.OnHitSpinner = OnHitSpinner;
            Hold.SpeedGetter = (() => Speed);

            Add(_transitionListener = new TransitionListener());
            _transitionListener.OnOutBegin = () => _isCrucial = false;

            Add(ConveyorMover = new ConveyorMover());
            ConveyorMover.OnMove = MoveOnConveyor;

            Add(new SteamCollider(OnSteamWall));

            LiftSpeedGraceTime = 0.1f;

            Add(new LightOcclude(0.2f));
            Add(new MirrorReflection());
        }

        private void OnSteamWall(SteamWall steamWall)
        {
            Shatter();
        }

        private void OnHitSpinner(Entity spinner)
        {
            Shatter();
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            _level = SceneAs<Level>();
        }

        public override void Update()
        {
            base.Update();
            if (_soundTimerX > 0f)
            {
                _soundTimerX -= Engine.DeltaTime;
            }
            if (_soundTimerY > 0f)
            {
                _soundTimerY -= Engine.DeltaTime;
            }
            if (Hold.IsHeld)
            {
                _prevLiftSpeed = Vector2.Zero;
            }
            else if (!ConveyorMover.IsOnConveyor)
            {
                if (OnGround())
                {
                    float target = (!OnGround(Position + Vector2.UnitX * 3f)) ? 20f : (OnGround(Position - Vector2.UnitX * 3f) ? 0f : (-20f));
                    Speed.X = Calc.Approach(Speed.X, target, 800f * Engine.DeltaTime);
                    Vector2 liftSpeed = base.LiftSpeed;
                    if (liftSpeed == Vector2.Zero && _prevLiftSpeed != Vector2.Zero)
                    {
                        Speed = _prevLiftSpeed;
                        _prevLiftSpeed = Vector2.Zero;
                        Speed.Y = Math.Min(Speed.Y * 0.6f, 0f);
                        if (Speed.X != 0f && Speed.Y == 0f)
                        {
                            Speed.Y = -60f;
                        }
                        if (Speed.Y < 0f)
                        {
                            _noGravityTimer = 0.15f;
                        }
                    }
                    else
                    {
                        _prevLiftSpeed = liftSpeed;
                        if (liftSpeed.Y < 0f && Speed.Y < 0f)
                        {
                            Speed.Y = 0f;
                        }
                    }
                }
                else if (Hold.ShouldHaveGravity)
                {
                    float accY = 800f;
                    if (Math.Abs(Speed.Y) <= 30f)
                    {
                        accY *= 0.5f;
                    }
                    float accX = 350f;
                    if (Speed.Y < 0f)
                    {
                        accX *= 0.5f;
                    }
                    Speed.X = Calc.Approach(Speed.X, 0f, accX * Engine.DeltaTime);
                    if (_noGravityTimer > 0f)
                    {
                        _noGravityTimer -= Engine.DeltaTime;
                    }
                    else
                    {
                        Speed.Y = Calc.Approach(Speed.Y, 300f, accY * Engine.DeltaTime);
                    }
                }
                MoveH(Speed.X * Engine.DeltaTime, OnCollideH);
                MoveV(Speed.Y * Engine.DeltaTime, OnCollideV);
                if (IsSpecial)
                {
                    if (Left < _level.Bounds.Left)
                    {
                        Left = _level.Bounds.Left;
                        Speed.X *= -0.4f;
                    }
                    else if (Right > _level.Bounds.Right)
                    {
                        Right = _level.Bounds.Right;
                        Speed.X *= -0.4f;
                    }
                    else if (Top < _level.Bounds.Top - 4)
                    {
                        Top = _level.Bounds.Top - 4;
                        Speed.Y = 0;
                    }
                    else if (Bottom > _level.Bounds.Bottom + 4)
                    {
                        Shatter();
                    }
                }
                if (_isCrucial && Bottom > _level.Bounds.Bottom + 4)
                {
                    Shatter();
                }
                if (Left > _level.Bounds.Right + 8 || Right < _level.Bounds.Left - 8 || Top > _level.Bounds.Bottom + 8 || Bottom < _level.Bounds.Top - 8)
                {
                    RemoveSelf();
                }
            }
            if (Collidable)
            {
                Hold.CheckAgainstColliders();
            }
            if (_tutorial && OnGround(4) && !ConveyorMover.IsOnConveyor && _tutorialCarry == null)
            {
                _tutorialCarry = new BirdTutorialGui(this, new Vector2(0f, -24f), Dialog.Clean("tutorial_carry"), Dialog.Clean("tutorial_hold"), Input.Grab);
                _tutorialCarry.Open = true;
                Scene.Add(_tutorialCarry);
            }
        }

        public override bool IsRiding(Solid solid)
        {
            return Speed.Y == 0f && base.IsRiding(solid);
        }

        public override void Removed(Scene scene)
        {
            OnRemoved?.Invoke();
            if (IsSpecial && _unspecializeOnRemove)
            {
                (FactoryHelperModule.Instance._Session as FactoryHelperSession).SpecialBoxPosition = null;
            }
            base.Removed(scene);
        }

        public void StopBeingSpecial()
        {
            (FactoryHelperModule.Instance._Session as FactoryHelperSession).SpecialBoxPosition = null;
            Remove(_shimmerParticles);
            _shimmerParticles = null;
            IsSpecial = false;
        }

        private void MoveOnConveyor(float amount)
        {
            float accY = 800f;
            if (Math.Abs(Speed.Y) <= 30f)
            {
                accY *= 0.5f;
            }
            if (_noGravityTimer > 0f)
            {
                _noGravityTimer -= Engine.DeltaTime;
            }
            else
            {
                Speed.Y = Calc.Approach(Speed.Y, 300f, accY * Engine.DeltaTime);
            }
            Speed.X = Calc.Approach(Speed.X, amount, CONVEYOR_ACCELERATION * Engine.DeltaTime);
            MoveH(Speed.X * Engine.DeltaTime, OnCollideH);
            MoveV(Speed.Y * Engine.DeltaTime, OnCollideV);
        }

        protected override void OnSquish(CollisionData data)
        {
            if (Collidable && !TrySquishWiggle(data))
            {
                Shatter();
            }
        }

        private bool HitSpring(Spring spring)
        {
            if (!Hold.IsHeld)
            {
                if (spring.Orientation == Spring.Orientations.Floor && Speed.Y >= 0f)
                {
                    Speed.X *= 0.5f;
                    Speed.Y = -200f;
                    _noGravityTimer = 0.3f;
                    return true;
                }
                if (spring.Orientation == Spring.Orientations.WallLeft && Speed.X <= 0f)
                {
                    MoveTowardsY(spring.CenterY + 5f, 4f);
                    Speed.X = 220f;
                    Speed.Y = -80f;
                    _noGravityTimer = 0.1f;
                    return true;
                }
                if (spring.Orientation == Spring.Orientations.WallRight && Speed.X >= 0f)
                {
                    MoveTowardsY(spring.CenterY + 5f, 4f);
                    Speed.X = -220f;
                    Speed.Y = -80f;
                    _noGravityTimer = 0.1f;
                    return true;
                }
            }
            return false;
        }

        private void OnRelease(Vector2 force)
        {
            if ((_player == null || _player.Dead) && FactoryHelperModule.Session.SpecialBoxLevel == SceneAs<Level>().Session.Level)
            {
                StopBeingSpecial();
            }
            Collidable = true;
            _unspecializeOnRemove = true;
            RemoveTag(Tags.Persistent);
            if (force.X != 0f && force.Y == 0f)
            {
                force.Y = -0.4f;
            }
            Speed = force * 200f;
            if (Speed != Vector2.Zero)
            {
                _noGravityTimer = 0.1f;
            }
            if (_tutorialPutDown != null)
            {
                _tutorialPutDown.Open = false;
            }
        }

        private void OnPickup()
        {
            _player = Hold.Holder;
            _pickedUp = true;
            Collidable = false;
            _unspecializeOnRemove = false;
            Speed = Vector2.Zero;
            AddTag(Tags.Persistent);
            if (IsSpecial)
            {
                FactoryHelperSession factorySession = FactoryHelperModule.Session;
                if (factorySession.SpecialBoxPosition == null)
                {
                    factorySession.SpecialBoxPosition = _starterPosition;
                    factorySession.OriginalSession = (Scene as Level).Session;
                    factorySession.SpecialBoxLevel = _levelName;
                }
                ParticleSystem particlesFG = (Scene as Level).ParticlesFG;
                Add(_shimmerParticles = new ParticleEmitter(particlesFG, Key.P_Shimmer, Vector2.UnitY * -6, new Vector2(6f, 6f), 1, 0.2f));
                _shimmerParticles.SimulateCycle();
            }
            if (_tutorialCarry != null)
            {
                _tutorialCarry.Open = false;
            }
            if (_tutorial && _tutorialPutDown == null)
            {
                _tutorialPutDown = new BirdTutorialGui(this, new Vector2(0f, -24f), Dialog.Clean("tutorial_drop"), Dialog.Clean("tutorial_hold"), new Vector2(0f, 1f), "+ ", Dialog.Clean("tutorial_release"), Input.Grab);
                Scene.Add(_tutorialPutDown);
            }
            if (_tutorialPutDown != null)
            {
                _tutorialPutDown.Open = true;
            }
        }

        private void OnCollideV(CollisionData data)
        {
            if (_soundTimerY <= 0f && Math.Abs(Speed.Y) > 100f)
            {
                PlayHitSound();
                _soundTimerY = SOUND_RELOAD_TIME;
            }
            if (Speed.Y > 160f)
            {
                ImpactParticles(data.Direction);
            }
            if (data.Hit is DashSwitch)
            {
                (data.Hit as DashSwitch).OnDashCollide(null, Vector2.UnitY * Math.Sign(Speed.Y));
            }
            if (!_isMetal && Math.Abs(Speed.Y) >= VERTICAL_BREAK_SPEED)
            {
                Shatter();
            }
            else if (CollideCheck<PressurePlate.Button>())
            {
                Speed = Vector2.Zero;
            }
            else if (Math.Abs(Speed.Y) > 40f)
            {
                Speed.Y *= -0.25f;
            }
            else
            {
                Speed.Y = 0f;
                Position.Y = (float)Math.Floor(Position.Y);
            }
        }

        private void OnCollideH(CollisionData data)
        {
            if (Math.Abs(Speed.X) > 100f)
            {
                ImpactParticles(data.Direction);
                if (_soundTimerX <= 0f)
                {
                    PlayHitSound();
                    _soundTimerX = SOUND_RELOAD_TIME;
                }
            }
            if (data.Hit is DashSwitch)
            {
                (data.Hit as DashSwitch).OnDashCollide(null, Vector2.UnitX * Math.Sign(Speed.X));
            }
            if (data.Hit is DashFuseBox)
            {
                (data.Hit as DashFuseBox).OnDashed(null, Vector2.UnitX * Math.Sign(Speed.X));
            }
            if (!_isMetal && (Math.Abs(Speed.X) >= HORIZONTAL_BREAK_SPEED))
            {
                Shatter();
            }
            else if (Math.Abs(Speed.X) > 10f)
            {
                Speed.X *= -0.4f;
            }
            else
            {
                Speed.X = 0f;
            }
        }

        private void PlayHitSound()
        {
            if (_isMetal)
            {
                Audio.Play("event:/char/madeline/landing", Position, "surface_index", 7);
            }
            else
            {
                Audio.Play("event:/char/madeline/landing", Position, "surface_index", 18);
            }
        }

        private void ImpactParticles(Vector2 dir)
        {
            float direction;
            Vector2 position;
            Vector2 positionRange;
            if (dir.X > 0f)
            {
                direction = (float)Math.PI;
                position = new Vector2(Right, Center.Y);
                positionRange = Vector2.UnitY * 6f;
            }
            else if (dir.X < 0f)
            {
                direction = 0f;
                position = new Vector2(Left, Center.Y);
                positionRange = Vector2.UnitY * 6f;
            }
            else if (dir.Y > 0f)
            {
                direction = -(float)Math.PI / 2f;
                position = new Vector2(Center.X, Bottom);
                positionRange = Vector2.UnitX * 6f;
            }
            else
            {
                direction = (float)Math.PI / 2f;
                position = new Vector2(Center.X, Top);
                positionRange = Vector2.UnitX * 6f;
            }
            (Scene as Level).Particles.Emit(P_Impact, 12, position, positionRange, direction);
        }

        private void Shatter()
        {
            if (!_shattered && !Hold.IsHeld)
            {
                _shattered = true;
                _image.Visible = false;
                if (_isMetal)
                {
                    Audio.Play("event:/game/general/wall_break_ice", Position);
                }
                else
                {
                    Audio.Play("event:/game/general/wall_break_wood", Position);
                }
                for (int i = 0; i < Width / 8f; i++)
                {
                    for (int j = 0; j < Height / 8f; j++)
                    {
                        if (_isMetal)
                        {
                            base.Scene.Add(Engine.Pooler.Create<Debris>().Init(Position + new Vector2(4 + i * 8, 4 + j * 8) + DISPLACEMENT, '8', false).BlastFrom(Center));
                        }
                        else
                        {
                            base.Scene.Add(Engine.Pooler.Create<Debris>().Init(Position + new Vector2(4 + i * 8, 4 + j * 8) + DISPLACEMENT, '9', false).BlastFrom(Center));
                        }
                    }
                }
                Player player;
                player = Scene.Tracker.GetEntity<Player>();
                if ((IsSpecial && FactoryHelperModule.Session.SpecialBoxPosition != null && FactoryHelperModule.Session.SpecialBoxLevel != SceneAs<Level>().Session.Level) || (_isCrucial && _pickedUp))
                {
                    if (player != null)
                    {
                        player.Die(-(Position - player.Position).SafeNormalize());
                    }
                    _unspecializeOnRemove = false;
                }
                else if (IsSpecial)
                {
                    StopBeingSpecial();
                    _unspecializeOnRemove = true;
                }
                RemoveSelf();
            }
        }
    }
}
