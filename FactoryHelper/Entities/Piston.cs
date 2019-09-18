using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace FactoryHelper.Entities
{
    class Piston : Entity
    {
        public float MoveTime { get; } = 0.2f;
        public float PauseTime { get; } = 0.4f;

        public float Percent { get; private set; } = 0f;

        public bool MoveForward { get; private set; } = true;
        
        public bool Moving { get; private set; } = true;

        public bool Activated { get; private set; }
        public float PauseTimer { get; private set; }

        private const int _bodyVariantCount = 4;
        private PistonPart _head;
        private PistonPart[] _body;
        private PistonPart _base;
        private Vector2 _startPos;
        private Vector2 _endPos;
        private Vector2 _basePos;
        private int _bodyPartCount;

        public Piston(Vector2 position, Vector2 start, Vector2 end, string directionString)
        {
            Activated = true;
            Random rnd = new Random();
            double length;
            Directions direction = Directions.Up;
            Enum.TryParse<Directions>(directionString, out direction);

            _startPos = start;
            _endPos = end;
            _basePos = position;

            if (direction == Directions.Up || direction == Directions.Down)
            {
                length = Math.Max(position.Y - end.Y, position.Y - start.Y);
                _bodyPartCount = (int) Math.Ceiling(length / 8);

                _base = new PistonPart(position + new Vector2(2, 0), 12, 8, -2, 0, "objects/FactoryHelper/piston/base00");
                _base.Depth = -20;
                _head = new PistonPart(end, 16, 8, 0, 0, "objects/FactoryHelper/piston/head00");
                _head.Depth = -20;
                _body = new PistonPart[_bodyPartCount];
                for (int i = 0; i < _bodyPartCount; i++)
                {
                    Vector2 offset = new Vector2(3, i * -(position.Y - end.Y) / _bodyPartCount);
                    _body[i] = new PistonPart(position + offset, 10, 8, -3, 0, $"objects/FactoryHelper/piston/body0{rnd.Next(_bodyVariantCount)}");
                    _body[i].Depth = -10;
                }
            }

            base.Add(new Coroutine(Sequence(), true));
        }

        private IEnumerator Sequence()
        {
            for (; ; )
            {
                while (!(Activated && Moving))
                {
                    yield return null;
                }

                yield return PauseTime;

                while ( Percent < 1f )
                {
                    yield return null;
                    Percent = Calc.Approach(Percent, 1f, Engine.DeltaTime / MoveTime);
                    UpdatePosition();

                }
                PauseTimer = PauseTime;
                Percent = 0f;
                MoveForward = !MoveForward;
            }
        }

        private void UpdatePosition()
        {
            var start = MoveForward ? _startPos : _endPos;
            var end = MoveForward ? _endPos : _startPos;
            _head.MoveTo(Vector2.Lerp(start, end, Ease.SineIn(Percent)));

            for (int i = 0; i < _bodyPartCount; i++)
            {
                Vector2 from = new Vector2(_basePos.X + 3, i * -(_basePos.Y - _startPos.Y) / _bodyPartCount + _basePos.Y);
                Vector2 to = new Vector2(_basePos.X + 3, i * -(_basePos.Y - _endPos.Y) / _bodyPartCount + _basePos.Y);
                start = MoveForward ? from : to;
                end = MoveForward ? to : from;
                _body[i].MoveTo(Vector2.Lerp(start, end, Ease.SineIn(Percent)));
            }
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
                Sprite = new Image(GFX.Game[sprite]);
                Sprite.Active = true;
                Sprite.Origin = new Vector2(0, 0);
                Sprite.Position = new Vector2(0 + xOffset, 0 + yOffset);
                base.Add(Sprite);
                Console.WriteLine("Adding sprite: " + sprite);
                Console.WriteLine("Position: " + Sprite.Position);
                Console.WriteLine("Solid Position: " + base.Position);
            }

            public Image Sprite;

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
