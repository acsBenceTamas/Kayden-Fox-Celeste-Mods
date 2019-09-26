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
    public class RustySpike : Entity
    {
        public enum Directions
        {
            Up,
            Down,
            Left,
            Right
        }

        public Directions Direction;

        private PlayerCollider pc;

        private Vector2 imageOffset;

        private int size;

        private string overrideType;

        public RustySpike(Vector2 position, int size, Directions direction)
            : base(position)
        {
            base.Depth = -1;
            Direction = direction;
            this.size = size;
            switch (direction)
            {
                case Directions.Up:
                    base.Collider = new Hitbox(size, 3f, 0f, -3f);
                    Add(new LedgeBlocker());
                    break;
                case Directions.Down:
                    base.Collider = new Hitbox(size, 3f);
                    break;
                case Directions.Left:
                    base.Collider = new Hitbox(3f, size, -3f);
                    Add(new LedgeBlocker());
                    break;
                case Directions.Right:
                    base.Collider = new Hitbox(3f, size);
                    Add(new LedgeBlocker());
                    break;
            }
            Add(pc = new PlayerCollider(OnCollide));
            Add(new StaticMover
            {
                OnShake = OnShake,
                SolidChecker = IsRiding,
                JumpThruChecker = IsRiding,
                OnEnable = OnEnable,
                OnDisable = OnDisable
            });
        }

        public RustySpike(EntityData data, Vector2 offset, Directions dir)
            : this(data.Position + offset, GetSize(data, dir), dir)
        {
        }

        public void SetSpikeColor(Color color)
        {
            foreach (Component component in base.Components)
            {
                Image image = component as Image;
                if (image != null)
                {
                    image.Color = color;
                }
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            AreaData areaData = AreaData.Get(scene);
            string str = Direction.ToString().ToLower();
            List<MTexture> atlasSubtextures = GFX.Game.GetAtlasSubtextures("objects/FactoryHelper/rustySpike/rusty_" + str);
            for (int j = 0; j < size / 8; j++)
            {
                MTexture texture = Calc.Random.Choose(atlasSubtextures);
                Image image = new Image(texture);
                switch (Direction)
                {
                    case Directions.Up:
                        image.JustifyOrigin(0.5f, 1f);
                        image.Position = Vector2.UnitX * ((float)j + 0.5f) * 8f + Vector2.UnitY;
                        break;
                    case Directions.Down:
                        image.JustifyOrigin(0.5f, 0f);
                        image.Position = Vector2.UnitX * ((float)j + 0.5f) * 8f - Vector2.UnitY;
                        break;
                    case Directions.Right:
                        image.JustifyOrigin(0f, 0.5f);
                        image.Position = Vector2.UnitY * ((float)j + 0.5f) * 8f - Vector2.UnitX;
                        break;
                    case Directions.Left:
                        image.JustifyOrigin(1f, 0.5f);
                        image.Position = Vector2.UnitY * ((float)j + 0.5f) * 8f + Vector2.UnitX;
                        break;
                }
                Add(image);
            }
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

        private void OnCollide(Player player)
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

        private static int GetSize(EntityData data, Directions dir)
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
