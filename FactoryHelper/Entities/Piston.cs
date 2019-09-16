using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace FactoryHelper.Entities
{
    class Piston : Entity
    {
        public Piston(Vector2 position, Vector2 start, Vector2 end, Directions direction)
        {
            Random rnd = new Random();
            double length;
            int bodyPartCount;
            if (direction == Directions.Up || direction == Directions.Down)
            {
                length = Math.Max(position.Y - end.Y, position.Y - start.Y);
                bodyPartCount = (int) Math.Ceiling(length);
                _base = new PistonPart(position, 12, 8, -2, 0, "objects/FactoryHelper/piston/base");
                _head = new PistonPart(end, 16, 8, 0, 0, "objects/FactoryHelper/piston/head");
                _body = new PistonPart[bodyPartCount];
                for (int i = 0; i < bodyPartCount; i++)
                {
                    Vector2 offset = new Vector2(0, i * (position.Y - end.Y) / bodyPartCount);
                    _body[i] = new PistonPart(position + offset, 10, 8, -3, 0, $"objects/FactoryHelper/piston/body0{rnd.Next(4)}");
                }
            }
        }

        public Piston(EntityData data, Vector2 offset) : this(data.Position + offset, data.Nodes[0] + offset, data.Nodes[1] + offset, data.Enum<Directions>("direction"))
        {
        }

        private PistonPart _head;
        private PistonPart[] _body;
        private PistonPart _base;

        public enum Directions
        {
            Up,
            Down,
            Left,
            Right
        }

        private class PistonPart : Solid
        {
            public PistonPart(Vector2 position, float width, float height, bool safe) : base(position, width, height, safe)
            {
            }

            public PistonPart(Vector2 position, float width, float height, float xOffset, float yOffset, string sprite) : base(position, width, height, true)
            {
                Sprite = new Sprite(GFX.Game, sprite);
                Sprite.Position = position - new Vector2(xOffset, yOffset);
                base.Add(Sprite);
            }

            public Sprite Sprite;
        }
    }
}
