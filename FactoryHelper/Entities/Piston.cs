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

        public float InitialDelay { get; private set; } = 0f;

        public float Percent { get; private set; } = 0f;

        public float PauseTimer { get; private set; }

        public bool MovingForward { get; private set; } = false;
        
        public bool Moving { get; private set; } = true;

        public bool Activated { get; private set; }

        private string _activationId;
        private const int _bodyVariantCount = 4;
        private PistonPart _head;
        private PistonPart _base;
        private Solid _body;
        private SoundSource _sfx;
        private Image[] _bodyImages;
        private Vector2 _startPos;
        private Vector2 _endPos;
        private Vector2 _basePos;
        private int _bodyPartCount;
        private Direction _direction = Direction.Up;
        private static readonly Random _rnd = new Random();

        private float RotationModifier {
            get
            {
                switch (_direction)
                {
                    default:
                        return 0f;
                    case Direction.Down:
                        return (float)Math.PI;
                    case Direction.Left:
                        return (float)Math.PI / -2;
                    case Direction.Right:
                        return (float)Math.PI / 2;
                }
            }
        }

        private int HeadMinimumPositionModifier
        {
            get
            {
                switch (_direction)
                {
                    default:
                    case Direction.Up:
                    case Direction.Left:
                        return -8;
                    case Direction.Right:
                    case Direction.Down:
                        return 8;
                }
            }
        }

        private int HeadPositionModifier
        {
            get
            {
                switch (_direction)
                {
                    default:
                    case Direction.Left:
                    case Direction.Up:
                        return 8;
                    case Direction.Right:
                    case Direction.Down:
                        return -8;
                }
            }
        }

        private int GrabPositionModifier
        {
            get
            {
                switch (_direction)
                {
                    default:
                    case Direction.Up:
                    case Direction.Left:
                        return 16;
                    case Direction.Right:
                    case Direction.Down:
                        return 8;
                }
            }
        }

        private int BodyPositionModifier
        {
            get
            {
                switch (_direction)
                {
                    default:
                    case Direction.Up:
                    case Direction.Left:
                        return 0;
                    case Direction.Down:
                        return (int)Math.Abs(_basePos.Y - _head.Y) - 16;
                    case Direction.Right:
                        return (int)Math.Abs(_basePos.X - _head.X) - 16;
                }
            }
        }
        private bool ActivationProtocol
        {
            get
            {
                if (_activationId == null)
                {
                    return true;
                }
                else
                {
                    Level level = Scene as Level;
                    return level.Session.GetFlag(_activationId) || level.Session.GetFlag("Persistent" + _activationId);
                }
            }
        }

        public Piston(EntityData data, Vector2 offset) : this(data.Position + offset, 
                                                              data.Nodes[0] + offset, 
                                                              data.Nodes[1] + offset, 
                                                              data.Attr("direction", "Up"),
                                                              data.Bool("startActive", true),
                                                              data.Float("moveTime",0.4f),
                                                              data.Float("pauseTime", 0.2f),
                                                              data.Float("initialDelay", 0f),
                                                              data.Attr("activationId", ""))
        {
        }

        public Piston(Vector2 position, Vector2 start, Vector2 end, string directionString, bool startActive, float moveTime, float pauseTime, float initialDelay, string activationId)
        {
            MoveTime = moveTime;
            PauseTime = pauseTime;
            InitialDelay = initialDelay;
            Activated = activationId == string.Empty ? true : startActive;
            double length = 0;
            Enum.TryParse(directionString, out _direction);


            _basePos = position;
            _activationId = activationId == string.Empty ? null : $"FactoryActivation:{activationId}";

            //Add(new Coroutine(Sequence(), true));

            if (_direction == Direction.Up || _direction == Direction.Down)
            {
                _startPos.X = _basePos.X;
                _endPos.X = _basePos.X;

                if (_direction == Direction.Up)
                {
                    _startPos.Y = Math.Min(start.Y, _basePos.Y + HeadMinimumPositionModifier);
                    _endPos.Y = Math.Min(end.Y, _basePos.Y + HeadMinimumPositionModifier);
                } else
                {
                    _startPos.Y = Math.Max(start.Y, _basePos.Y + HeadMinimumPositionModifier);
                    _endPos.Y = Math.Max(end.Y, _basePos.Y + HeadMinimumPositionModifier);
                }

                length = Math.Max(Math.Abs(_basePos.Y - _endPos.Y), Math.Abs(_basePos.Y - _startPos.Y));

                _base = new PistonPart(_basePos + new Vector2(2, 0), 12, 8, "objects/FactoryHelper/piston/base00");
                _head = new PistonPart(_startPos, 16, 8, "objects/FactoryHelper/piston/head00");
                _body = new Solid(new Vector2(_basePos.X + 3, Math.Min(_startPos.Y, _basePos.Y) + 8), 10, 0, false);
            }
            else if (_direction == Direction.Left || _direction == Direction.Right)
            {
                _startPos.Y = _basePos.Y;
                _endPos.Y = _basePos.Y;

                if (_direction == Direction.Left)
                {
                    _startPos.X = Math.Min(start.X, _basePos.X + HeadMinimumPositionModifier);
                    _endPos.X = Math.Min(end.X, _basePos.X + HeadMinimumPositionModifier);
                }
                else
                {
                    _startPos.X = Math.Max(start.X, _basePos.X + HeadMinimumPositionModifier);
                    _endPos.X = Math.Max(end.X, _basePos.X + HeadMinimumPositionModifier);
                }

                length = Math.Max(Math.Abs(_basePos.X - _endPos.X), Math.Abs(_basePos.X - _startPos.X));

                _base = new PistonPart(_basePos + new Vector2(0, 2), 8, 12, "objects/FactoryHelper/piston/base00");
                _head = new PistonPart(_startPos, 8, 16, "objects/FactoryHelper/piston/head00");
                _body = new Solid(new Vector2(Math.Min(_startPos.X, _basePos.X) + 8, _basePos.Y + 3), Math.Abs(_base.X - _head.X) - 8, 10, false);
            }

            _base.Image.Rotation += RotationModifier;
            _head.Image.Rotation += RotationModifier;
            _bodyPartCount = (int)Math.Ceiling(length / 8);
            _bodyImages = new Image[_bodyPartCount];

            for (int i = 0; i < _bodyPartCount; i++)
            {
                Vector2 bodyPosition;
                if (_direction == Direction.Up || _direction == Direction.Down)
                {
                    bodyPosition = new Vector2(-3, i * (_basePos.Y - (_head.Y + HeadPositionModifier)) / (_bodyPartCount) + BodyPositionModifier);
                }
                else
                {
                    bodyPosition = new Vector2(i * (_basePos.X - (_head.X + HeadPositionModifier)) / (_bodyPartCount) + BodyPositionModifier, -3);
                }
                string bodyImage = $"objects/FactoryHelper/piston/body0{_rnd.Next(_bodyVariantCount)}";

                _body.Add(_bodyImages[i] = new Image(GFX.Game[bodyImage])
                {
                    Position = bodyPosition
                });
                _bodyImages[i].CenterOrigin();
                _bodyImages[i].Rotation += RotationModifier;
                _bodyImages[i].Position = _direction == Direction.Down || _direction == Direction.Up ? new Vector2(8, 4) : new Vector2(4, 8);
            }

            _base.Depth = -20;
            _head.Depth = -20;
            _body.Depth = -10;
            _body.AllowStaticMovers = false;
            _base.Add(_sfx = new SoundSource());
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            UpdatePosition();
        }

        public override void Update()
        {
            base.Update();
            if (ActivationProtocol && Moving)
            {
                if (!_sfx.Playing)
                {
                    _sfx.Play("event:/env/local/09_core/conveyor_idle");
                }

                if (InitialDelay > 0f)
                {
                    InitialDelay -= Engine.DeltaTime;
                }
                else if (PauseTimer > 0f)
                {
                    PauseTimer -= Engine.DeltaTime;
                }
                else if (Percent < 1f)
                {
                    Percent = Calc.Approach(Percent, 1f, Engine.DeltaTime / MoveTime);
                    UpdatePosition();
                }
                else
                {
                    PauseTimer = PauseTime;
                    Percent = 0f;
                    MovingForward = !MovingForward;
                }
            } else if (_sfx.Playing)
            {
                _sfx.Stop();
            }
        }

        private void UpdatePosition()
        {
            Vector2 posDisplacement;
            var start = MovingForward ? _startPos : _endPos;
            var end = MovingForward ? _endPos : _startPos;
            _head.MoveTo(Vector2.Lerp(end, start, Ease.SineIn(Percent)));
            switch (_direction)
            {
                default:
                case Direction.Up:
                case Direction.Down:
                    posDisplacement = new Vector2(8, 4);
                    float heightBefore = _body.Collider.Height;
                    float heightAfter = Math.Abs(_base.Y - _head.Y) - 8;
                    
                    _body.Y = Math.Min(_head.Y, _base.Y) + 8;
                    _body.Collider.Height = heightAfter;

                    if (_body.HasPlayerClimbing())
                    {
                        Player player = _body.GetPlayerRider();
                        float newY = _base.Y + GrabPositionModifier + (player.Y - (_base.Y + GrabPositionModifier)) * heightAfter / heightBefore;
                        player.LiftSpeed = new Vector2(player.LiftSpeed.X, (newY - player.Y) / Engine.DeltaTime);
                        player.MoveV(newY - player.Y);
                    }

                    for (int i = 0; i < _bodyPartCount; i++)
                    {
                        _bodyImages[i].Position = new Vector2(-3, i * (_basePos.Y - (_head.Y + HeadPositionModifier)) / (_bodyPartCount) + BodyPositionModifier) + posDisplacement;
                    }
                    break;
                case Direction.Left:
                case Direction.Right:
                    posDisplacement = new Vector2(4, 8);
                    float widthBefore = _body.Collider.Width;
                    float widthAfter = Math.Abs(_base.X - _head.X) - 8;

                    _body.X = Math.Min(_head.X, _base.X) + 8;
                    _body.Collider.Width = widthAfter;

                    if (_body.HasPlayerOnTop())
                    {
                        Player player = _body.GetPlayerRider();
                        float newX = _base.X + GrabPositionModifier + (player.X - (_base.X + GrabPositionModifier)) * widthAfter / widthBefore;
                        player.LiftSpeed = new Vector2((newX - player.X) / Engine.DeltaTime, player.LiftSpeed.Y);
                        player.MoveH(newX - player.X);
                    }

                    for (int i = 0; i < _bodyPartCount; i++)
                    {
                        _bodyImages[i].Position = new Vector2(i * (_basePos.X - (_head.X + HeadPositionModifier)) / (_bodyPartCount) + BodyPositionModifier, -3) + posDisplacement;
                    }
                    break;
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Add(_head);
            scene.Add(_base);
            scene.Add(_body);
        }

        public override void Removed(Scene scene)
        {
            scene.Remove(_head);
            scene.Remove(_base);
            scene.Remove(_body);
            base.Removed(scene);
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
        }
    }
}
