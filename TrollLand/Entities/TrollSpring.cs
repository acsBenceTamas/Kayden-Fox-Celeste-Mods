using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace TrollLand.Entities
{
    [CustomEntity("TrollLand/TrollSpring")]
    class TrollSpring : Entity
    {
        public enum Orientations
        {
            FloorLeft,
            FloorRight,
            WallLeftUp,
            WallRightUp,
            WallLeftDown,
            WallRightDown
        }

        private Sprite sprite;

        private Wiggler wiggler;

        private StaticMover staticMover;

        public Orientations Orientation;

        private bool playerCanUse;

        public Color DisabledColor = Color.White;

        public bool VisibleWhenDisabled;

        public TrollSpring(Vector2 position, Orientations orientation, bool playerCanUse) : base(position)
        {
            Orientation = orientation;
            this.playerCanUse = playerCanUse;
            Add(new PlayerCollider(OnCollide));
            Add(new HoldableCollider(OnHoldable));
            PufferCollider pufferCollider = new PufferCollider(OnPuffer);
            Add(pufferCollider);
            Add(sprite = new Sprite(GFX.Game, "objects/TrollLand/trollSpring/"));
            sprite.Add("idle", "", 0f, default(int));
            sprite.Add("bounce", "", 0.07f, "idle", 0, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 3, 4, 5);
            sprite.Add("disabled", "white", 0.07f);
            sprite.Play("idle");
            sprite.Origin.X = sprite.Width / 2f;
            sprite.Origin.Y = sprite.Height;
            base.Depth = -8501;
            staticMover = new StaticMover();
            staticMover.OnAttach = delegate (Platform p)
            {
                base.Depth = p.Depth + 1;
            };
            switch (orientation)
            {
                case Orientations.FloorLeft:
                case Orientations.FloorRight:
                    staticMover.SolidChecker = ((Solid s) => CollideCheck(s, Position + Vector2.UnitY));
                    staticMover.JumpThruChecker = ((JumpThru jt) => CollideCheck(jt, Position + Vector2.UnitY));
                    break;
                case Orientations.WallLeftUp:
                case Orientations.WallLeftDown:
                    staticMover.SolidChecker = ((Solid s) => CollideCheck(s, Position - Vector2.UnitX));
                    staticMover.JumpThruChecker = ((JumpThru jt) => CollideCheck(jt, Position - Vector2.UnitX));
                    break;
                case Orientations.WallRightUp:
                case Orientations.WallRightDown:
                    staticMover.SolidChecker = ((Solid s) => CollideCheck(s, Position + Vector2.UnitX));
                    staticMover.JumpThruChecker = ((JumpThru jt) => CollideCheck(jt, Position + Vector2.UnitX));
                    break;
            }
            Add(staticMover);
            Add(wiggler = Wiggler.Create(1f, 4f, delegate (float v)
            {
                sprite.Scale.Y = 1f + v * 0.2f;
            }));
            switch (orientation)
            {
                case Orientations.FloorLeft:
                case Orientations.FloorRight:
                    base.Collider = new Hitbox(16f, 6f, -8f, -6f);
                    pufferCollider.Collider = new Hitbox(16f, 10f, -8f, -10f);
                    break;
                case Orientations.WallLeftUp:
                case Orientations.WallLeftDown:
                    base.Collider = new Hitbox(6f, 16f, 0f, -8f);
                    pufferCollider.Collider = new Hitbox(12f, 16f, 0f, -8f);
                    sprite.Rotation = (float)Math.PI / 2f;
                    break;
                case Orientations.WallRightUp:
                case Orientations.WallRightDown:
                    base.Collider = new Hitbox(6f, 16f, -6f, -8f);
                    pufferCollider.Collider = new Hitbox(12f, 16f, -12f, -8f);
                    sprite.Rotation = -(float)Math.PI / 2f;
                    break;
                default:
                    throw new Exception("Orientation not supported!");
            }
            switch (orientation)
            {
                case Orientations.FloorRight:
                case Orientations.WallLeftDown:
                case Orientations.WallRightUp:
                    sprite.Scale.X = -1;
                    break;
            }
            staticMover.OnEnable = OnEnable;
            staticMover.OnDisable = OnDisable;
        }

        public TrollSpring(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Enum<Orientations>("orientation"), data.Bool("playerCanUse", defaultValue: true))
        {
        }

        private void OnEnable()
        {
            Visible = (Collidable = true);
            sprite.Color = Color.White;
            sprite.Play("idle");
        }

        private void OnDisable()
        {
            Collidable = false;
            if (VisibleWhenDisabled)
            {
                sprite.Play("disabled");
                sprite.Color = DisabledColor;
            }
            else
            {
                Visible = false;
            }
        }

        private void OnCollide(Player player)
        {
            if (player.StateMachine.State == 9 || !playerCanUse)
            {
                return;
            }
            if (Orientation == Orientations.WallLeftDown || Orientation == Orientations.WallRightDown)
            {
                if (player.Speed.Y <= 0f)
                {
                    BounceAnimate();
                    player.ExplodeLaunch(player.Center - Vector2.UnitY * 8, true, false);
                }
                return;
            }
            if (Orientation == Orientations.WallLeftUp || Orientation == Orientations.WallRightUp)
            {
                if (player.Speed.Y >= 0f)
                {
                    BounceAnimate();
                    player.SuperBounce(base.Top);
                }
                return;
            }
            if (Orientation == Orientations.FloorLeft)
            {
                if (player.SideBounce(-1, base.Right, base.CenterY))
                {
                    BounceAnimate();
                }
                return;
            }
            if (Orientation == Orientations.FloorRight)
            {
                if (player.SideBounce(1, base.Left, base.CenterY))
                {
                    BounceAnimate();
                }
                return;
            }
            throw new Exception("Orientation not supported!");
        }

        private void BounceAnimate()
        {
            Audio.Play("event:/game/general/spring", base.BottomCenter);
            staticMover.TriggerPlatform();
            sprite.Play("bounce", restart: true);
            wiggler.Start();
        }

        private void OnHoldable(Holdable h)
        {
            if (h.HitSpring(GetFakeSpring()))
            {
                BounceAnimate();
            }
        }

        private void OnPuffer(Puffer p)
        {
            if (p.HitSpring(GetFakeSpring()))
            {
                BounceAnimate();
            }
        }

        private Spring GetFakeSpring()
        {
            Spring spring;
            switch (Orientation)
            {
                default:
                case Orientations.WallLeftUp:
                case Orientations.WallRightUp:
                    spring = new Spring(Position, Spring.Orientations.Floor, true);
                    break;
                case Orientations.WallLeftDown:
                case Orientations.FloorLeft:
                    spring = new Spring(Position, Spring.Orientations.WallLeft, true);
                    break;
                case Orientations.WallRightDown:
                case Orientations.FloorRight:
                    spring = new Spring(Position, Spring.Orientations.WallRight, true);
                    break;
            }
            return spring;
        }

        private void OnSeeker(Seeker seeker)
        {
            if (seeker.Speed.Y >= -120f)
            {
                BounceAnimate();
                seeker.HitSpring();
            }
        }

        public override void Render()
        {
            if (Collidable)
            {
                sprite.DrawOutline();
            }
            base.Render();
        }
    }
}
