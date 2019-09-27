using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryHelper.Entities
{
    public class FactorySpike : Entity
    {
        public enum Directions
        {
            Up,
            Down,
            Left,
            Right
        }

        public Directions Direction;

        protected PlayerCollider pc;

        protected Vector2 imageOffset;

        protected int size;

        public FactorySpike(Vector2 position, int size, Directions direction)
            : base(position)
        {
            base.Depth = -1;
            Direction = direction;
            this.size = size;
            Add(new StaticMover
            {
                OnShake = OnShake,
                SolidChecker = IsRiding,
                JumpThruChecker = IsRiding,
                OnEnable = OnEnable,
                OnDisable = OnDisable
            });
        }

        public FactorySpike(EntityData data, Vector2 offset, Directions dir)
            : this(data.Position + offset, GetSize(data, dir), dir)
        {
        }

        private void OnEnable()
        {
            Active = (Visible = (Collidable = true));
        }

        private void OnDisable()
        {
            Active = (Visible = (Collidable = false));
        }

        private void OnShake(Vector2 amount)
        {
            imageOffset += amount;
        }

        public override void Render()
        {
            Vector2 position = Position;
            Position += imageOffset;
            base.Render();
            Position = position;
        }

        public void SetOrigins(Vector2 origin)
        {
            foreach (Component component in base.Components)
            {
                Image image = component as Image;
                if (image != null)
                {
                    Vector2 vector = origin - Position;
                    image.Origin = image.Origin + vector - image.Position;
                    image.Position = vector;
                }
            }
        }

        protected virtual void OnCollide(Player player)
        {
            switch (Direction)
            {
                case Directions.Up:
                    if (player.Speed.Y >= 0f && player.Bottom <= base.Bottom)
                    {
                        player.Die(new Vector2(0f, -1f));
                    }
                    break;
                case Directions.Down:
                    if (player.Speed.Y <= 0f)
                    {
                        player.Die(new Vector2(0f, 1f));
                    }
                    break;
                case Directions.Left:
                    if (player.Speed.X >= 0f)
                    {
                        player.Die(new Vector2(-1f, 0f));
                    }
                    break;
                case Directions.Right:
                    if (player.Speed.X <= 0f)
                    {
                        player.Die(new Vector2(1f, 0f));
                    }
                    break;
            }
        }

        protected static int GetSize(EntityData data, Directions dir)
        {
            switch (dir)
            {
                default:
                    return data.Height;
                case Directions.Up:
                case Directions.Down:
                    return data.Width;
            }
        }

        private bool IsRiding(Solid solid)
        {
            switch (Direction)
            {
                default:
                    return false;
                case Directions.Up:
                    return CollideCheckOutside(solid, Position + Vector2.UnitY);
                case Directions.Down:
                    return CollideCheckOutside(solid, Position - Vector2.UnitY);
                case Directions.Left:
                    return CollideCheckOutside(solid, Position + Vector2.UnitX);
                case Directions.Right:
                    return CollideCheckOutside(solid, Position - Vector2.UnitX);
            }
        }

        private bool IsRiding(JumpThru jumpThru)
        {
            if (Direction != 0)
            {
                return false;
            }
            return CollideCheck(jumpThru, Position + Vector2.UnitY);
        }
    }
}
