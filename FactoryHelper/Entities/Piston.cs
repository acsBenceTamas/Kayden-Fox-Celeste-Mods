using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace FactoryHelper.Entities
{
    class Piston : Entity
    {
        public Piston(Vector2 position, Vector2 start, Vector2 end, string direction)
        {
            Random rnd = new Random();
            double length;
            int bodyPartCount;
            //if (direction == Directions.Up || direction == Directions.Down)
            //{
                length = Math.Max(position.Y - end.Y, position.Y - start.Y);
                bodyPartCount = (int) Math.Ceiling(length / 8);

                _base = new PistonPart(position + new Vector2(2, 0), 12, 8, -2, 0, "objects/FactoryHelper/piston/base");
                _base.Depth = -10;
                _head = new PistonPart(end, 16, 8, 0, 0, "objects/FactoryHelper/piston/head");
                _head.Depth = -10;
                _body = new PistonPart[bodyPartCount];
                for (int i = 0; i < bodyPartCount; i++)
                {
                    Vector2 offset = new Vector2(3, i * -(position.Y - end.Y) / bodyPartCount);
                    _body[i] = new PistonPart(position + offset, 10, 8, -3, 0, "objects/FactoryHelper/piston/body");
                    _body[i].Depth = -20;
                }
            //}

        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Add(_head);
            scene.Add(_base);
            foreach (var bodyPart in _body)
            {
                scene.Add(bodyPart);
            }
        }

        public override void Removed(Scene scene)
        {
            scene.Remove(_head);
            scene.Remove(_base);
            foreach (var bodyPart in _body)
            {
                scene.Remove(bodyPart);
            }
            base.Removed(scene);
        }

        public Piston(EntityData data, Vector2 offset) : this(data.Position + offset, data.Nodes[0] + offset, data.Nodes[1] + offset, data.Attr("direction", "Up"))
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
                // Sprite.Position = new Vector2(0 + xOffset, 0 + yOffset);
                base.Add(Sprite);
                Console.WriteLine("Adding sprite: " + sprite);
                Console.WriteLine("Position: " + Sprite.Position);
                Console.WriteLine("Solid Position: " + base.Position);
            }

            public Sprite Sprite;

            public override void Added(Scene scene)
            {
                base.Added(scene);
            }

            public override void Removed(Scene scene)
            {
                base.Removed(scene);
            }
        }
    }
}
