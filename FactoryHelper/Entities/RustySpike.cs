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
    public class RustySpike : FactorySpike
    {
        public RustySpike(Vector2 position, int size, Directions direction)
            : base(position, size, direction)
        {
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
        }

        public RustySpike(EntityData data, Vector2 offset, Directions dir)
            : this(data.Position + offset, GetSize(data, dir), dir)
        {
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
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
    }
}
