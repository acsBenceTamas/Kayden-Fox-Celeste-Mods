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

        public bool MovingForward { get; private set; } = false;
        
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
        private Direction _direction = Direction.Up;

        public Piston(Vector2 position, Vector2 start, Vector2 end, string directionString)
        {
            Activated = true;
            Random rnd = new Random();
            double length;
            Enum.TryParse<Direction>(directionString, out _direction);

            _basePos = position;

            base.Add(new Coroutine(Sequence(), true));

            if (_direction == Direction.Up || _direction == Direction.Down)
            {
                _startPos.X = _basePos.X;
                _endPos.X = _basePos.X;

                switch (_direction)
                {
                    case Direction.Up:
                        _startPos.Y = Math.Min(start.Y, _basePos.Y - 8);
                        _endPos.Y = Math.Min(end.Y, _basePos.Y - 8);
                        break;
                    case Direction.Down:
                        _startPos.Y = Math.Max(start.Y, _basePos.Y + 8);
                        _endPos.Y = Math.Max(end.Y, _basePos.Y + 8);
                        break;
                }

                var initialHeadPos = !MovingForward ? _startPos : _endPos;
                _base = new PistonPart(_basePos + new Vector2(2, 0), 12, 8, "objects/FactoryHelper/piston/base00");
                _head = new PistonPart(initialHeadPos, 16, 8, "objects/FactoryHelper/piston/head00");

                if (_direction == Direction.Down)
                {
                    _base.Image.Rotation += (float)Math.PI;
                    _head.Image.Rotation += (float)Math.PI;
                }

                length = Math.Max(Math.Abs(_basePos.Y - _endPos.Y), Math.Abs(_basePos.Y - _startPos.Y));
                _bodyPartCount = (int)Math.Ceiling(length / 8);
                _body = new PistonPart[_bodyPartCount];

                for (int i = 0; i < _bodyPartCount; i++)
                {
                    var bodyPos = new Vector2(_basePos.X + 3, i * (_basePos.Y - (_head.Y + GetHeadPositionBonus())) / (_bodyPartCount) + _head.Y + GetHeadPositionBonus());
                    _body[i] = new PistonPart(bodyPos, 10, 8, $"objects/FactoryHelper/piston/body0{rnd.Next(_bodyVariantCount)}");
                    _body[i].Depth = -10;
                    _body[i].AllowStaticMovers = false;
                }
            }
            else if (_direction == Direction.Left || _direction == Direction.Right)
            {
                _startPos.Y = _basePos.Y;
                _endPos.Y = _basePos.Y;

                switch (_direction)
                {
                    case Direction.Left:
                        _startPos.X = Math.Min(start.X, _basePos.X - 8);
                        _endPos.X = Math.Min(end.X, _basePos.X - 8);
                        break;
                    case Direction.Right:
                        _startPos.X = Math.Max(start.X, _basePos.X + 8);
                        _endPos.X = Math.Max(end.X, _basePos.X + 8);
                        break;
                }

                var initialHeadPos = !MovingForward ? _startPos : _endPos;
                _base = new PistonPart(_basePos + new Vector2(0, 2), 8, 12, "objects/FactoryHelper/piston/base00");
                _head = new PistonPart(initialHeadPos, 8, 16, "objects/FactoryHelper/piston/head00");

                if (_direction == Direction.Left)
                {
                    _base.Image.Rotation -= (float)Math.PI / 2;
                    _head.Image.Rotation -= (float)Math.PI / 2;
                } else
                {
                    _base.Image.Rotation += (float)Math.PI / 2;
                    _head.Image.Rotation += (float)Math.PI / 2;
                }

                length = Math.Max(Math.Abs(_basePos.X - _endPos.X), Math.Abs(_basePos.X - _startPos.X));
                _bodyPartCount = (int)Math.Ceiling(length / 8);
                _body = new PistonPart[_bodyPartCount];

                for (int i = 0; i < _bodyPartCount; i++)
                {
                    var bodyPos = new Vector2(i * (_basePos.X - (_head.X + GetHeadPositionBonus())) / (_bodyPartCount) + _head.X + GetHeadPositionBonus(), _basePos.Y + 3);
                    _body[i] = new PistonPart(bodyPos, 8, 10, $"objects/FactoryHelper/piston/body0{rnd.Next(_bodyVariantCount)}");
                    _body[i].Depth = -10;
                    _body[i].AllowStaticMovers = false;

                    if (_direction == Direction.Left)
                    {
                        _body[i].Image.Rotation += (float)Math.PI / 2;
                    }
                    else
                    {
                        _body[i].Image.Rotation -= (float)Math.PI / 2;
                    }
                }
            }

            _base.Depth = -20;
            _head.Depth = -20;
        }

        private int GetHeadPositionBonus()
        {
            switch (_direction)
            {
                case Direction.Up:
                case Direction.Left:
                    return 8;
                case Direction.Right:
                case Direction.Down:
                    return -8;
            }
            return 0;
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
                MovingForward = !MovingForward;
            }
        }

        private void UpdatePosition()
        {
            var start = MovingForward ? _startPos : _endPos;
            var end = MovingForward ? _endPos : _startPos;
            _head.MoveTo(Vector2.Lerp(end, start, Ease.SineIn(Percent)));

            for (int i = 0; i < _bodyPartCount; i++)
            {
                switch (_direction)
                {
                    case Direction.Up:
                    case Direction.Down:
                    default:
                        _body[i].MoveTo(new Vector2(_basePos.X + 3, i * (_basePos.Y - (_head.Y + GetHeadPositionBonus())) / (_bodyPartCount) + _head.Y + GetHeadPositionBonus()));
                        break;
                    case Direction.Left:
                    case Direction.Right:
                        _body[i].MoveTo(new Vector2(i * (_basePos.X - (_head.X + GetHeadPositionBonus())) / (_bodyPartCount) + _head.X + GetHeadPositionBonus(), _basePos.Y + 3));
                        break;
                }
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

        public enum Direction
        {
            Up,
            Down,
            Left,
            Right
        }

        private class PistonPart : Solid
        {
            public Image Image;

            public PistonPart(Vector2 position, float width, float height, bool safe) : base(position, width, height, safe)
            {
            }

            public PistonPart(Vector2 position, float width, float height, string sprite) : base(position, width, height, false)
            {
                base.Add(Image = new Image(GFX.Game[sprite]));
                Image.Active = true;
                Image.CenterOrigin();
                Image.Position = new Vector2(Width / 2, Height / 2);
            }

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
