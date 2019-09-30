using Celeste;
using FactoryHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace FactoryHelper.Entities
{
    class ThrowBox : Actor
    {
        public Vector2 Speed;
        public Holdable Hold;
        public ConveyorMoverComponent ConveyorMover;

        private Sprite _sprite;
        private float _noGravityTimer;
	    private Vector2 _prevLiftSpeed;
        private Vector2 _previousPosition;
        private float _swatTimer;
        private Level _level;
        private bool _shattered;
        private bool _isMetal;

        public ThrowBox(EntityData data, Vector2 offset) : this (data.Position + offset, data.Bool("isMetal", false))
        {
        }

        public ThrowBox(Vector2 position, bool isMetal) : base(position)
        {
            Depth = 100;
            Collider = new Hitbox(16f, 16f, 0, 0);
            _isMetal = isMetal;
            string pathString = isMetal ? "crate_metal" : "crate";

            Add(_sprite = new Sprite(GFX.Game, "objects/FactoryHelper/crate/"));
            _sprite.Add("idle", pathString);
            _sprite.Play("idle");
            _sprite.Visible = true;
            _sprite.Active = true;

            Add(Hold = new Holdable(0.1f));
            Hold.PickupCollider = new Hitbox(24f, 24f, -4f, -4f);
            Hold.SlowFall = false;
            Hold.SlowRun = true;
            Hold.OnPickup = OnPickup;
            Hold.OnRelease = OnRelease;
            Hold.OnHitSpring = HitSpring;
            Hold.OnCarry = OnCarry;
            Hold.SpeedGetter = (() => Speed);

            Add(ConveyorMover = new ConveyorMoverComponent());
            ConveyorMover.Move = Move;

            LiftSpeedGraceTime = 0.1f;
        }

        private void OnCarry(Vector2 position)
        {
            Position = position + new Vector2(-8f, -16f);
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            _level = SceneAs<Level>();
        }

        private void Move(float amount)
        {
            Speed.X = Calc.Approach(Speed.X, amount, 5f);
            MoveH(Speed.X);
        }
        protected override void OnSquish(CollisionData data)
        {
            if (!TrySquishWiggle(data))
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
                    Speed.Y = -160f;
                    _noGravityTimer = 0.15f;
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
        }

        private void OnPickup()
        {
            Speed = Vector2.Zero;
            AddTag(Tags.Persistent);
        }

        public override void Update()
        {
            base.Update();
            if (_swatTimer > 0f)
            {
                _swatTimer -= Engine.DeltaTime;
            }
            if (Hold.IsHeld)
            {
                _prevLiftSpeed = Vector2.Zero;
            }
            else
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
                    float num = 800f;
                    if (Math.Abs(Speed.Y) <= 30f)
                    {
                        num *= 0.5f;
                    }
                    float num2 = 350f;
                    if (Speed.Y < 0f)
                    {
                        num2 *= 0.5f;
                    }
                    Speed.X = Calc.Approach(Speed.X, 0f, num2 * Engine.DeltaTime);
                    if (_noGravityTimer > 0f)
                    {
                        _noGravityTimer -= Engine.DeltaTime;
                    }
                    else
                    {
                        Speed.Y = Calc.Approach(Speed.Y, 200f, num * Engine.DeltaTime);
                    }
                }
                _previousPosition = base.ExactPosition;
                MoveH(Speed.X * Engine.DeltaTime, OnCollideH);
                MoveV(Speed.Y * Engine.DeltaTime, OnCollideV);
                if (Left > _level.Bounds.Right || Right < _level.Bounds.Left || Top > _level.Bounds.Bottom || Bottom < _level.Bounds.Top)
                {
                    RemoveSelf();
                }
            }
            Hold.CheckAgainstColliders();
        }

        public override bool IsRiding(Solid solid)
        {
            return Speed.Y == 0f && base.IsRiding(solid);
        }

        private void OnCollideV(CollisionData data)
        {
            if (data.Hit is DashSwitch)
            {
                (data.Hit as DashSwitch).OnDashCollide(null, Vector2.UnitX * Math.Sign(Speed.X));
            }
            if (!_isMetal && Math.Abs(Speed.Y) > 10f)
            {
                Shatter();
            }
            else
            {
                if (Math.Abs(Speed.Y) > 20f)
                {
                    Speed.Y *= -0.1f;
                }
                else
                {
                    Speed.Y = 0f;
                }
            }
        }

        private void OnCollideH(CollisionData data)
        {
            if (!_isMetal && Math.Abs(Speed.X) > 10f)
            {
                Shatter();
            } 
            else
            {
                if (Math.Abs(Speed.X) > 10f)
                {
                    Speed.X *= -0.4f;
                }
                else
                {
                    Speed.X = 0f;
                }
            }
            if (data.Hit is DashSwitch)
            {
                (data.Hit as DashSwitch).OnDashCollide(null, Vector2.UnitY * Math.Sign(Speed.Y));
            }
        }

        private void Shatter()
        {
            if (!_shattered)
            {
                _shattered = true;
                _sprite.Visible = false;
                Audio.Play("event:/game/general/wall_break_wood", Position);
                for (int i = 0; i < Width / 8f; i++)
                {
                    for (int j = 0; j < Height / 8f; j++)
                    {
                        base.Scene.Add(Engine.Pooler.Create<Debris>().Init(Position + new Vector2(4 + i * 8, 4 + j * 8), '9', false).BlastFrom(Center));
                    }
                }
                RemoveSelf();
            }
        }
    }
}
