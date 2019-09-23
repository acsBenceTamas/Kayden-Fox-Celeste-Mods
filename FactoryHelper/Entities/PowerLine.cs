using Celeste;
using FactoryHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace FactoryHelper.Entities
{
    [Tracked]
    class PowerLine : Entity
    {

        private enum Direction
        {
            Up,
            Down,
            Left,
            Right
        }

        private Node[] _cornerPoints;
        private List<Sprite> _sprites = new List<Sprite>();
        private bool _startActive;
        private FactoryActivationComponent _activator;
        private string _activationId;

        public bool Activated
        {
            get
            {
                return _activator.Active;
            }
            set
            {
                int i = _startActive == value ? 1 : 0;
                foreach (var sprite in _sprites)
                {
                    sprite.SetAnimationFrame(i);
                }
                _activator.Active = value;
            }
        }

        public PowerLine(EntityData entityData, Vector2 offset) : 
            this(entityData.Position,
                 offset,
                 entityData.Nodes,
                 entityData.Attr("activationId", ""),
                 entityData.Bool("startActive", false))
        {
        }

        public PowerLine(Vector2 position, Vector2 offset, Vector2[] nodes, string activationId, bool startActive)
        {
            Position = offset;
            _startActive = startActive;
            _activationId = activationId;

            string activationString = activationId == string.Empty ? null : $"FactoryActivation:{activationId}";
            Add(_activator = new FactoryActivationComponent(activationString));

            _cornerPoints = new Node[nodes.Length + 1];
            _cornerPoints[0] = new Node(position);
            for (int i=1; i < _cornerPoints.Length; i++)
            {
                _cornerPoints[i] = new Node(nodes[i - 1]);
                _cornerPoints[i - 1].Next = _cornerPoints[i];
                _cornerPoints[i].Previous = _cornerPoints[i - 1];
            }
            NormalizeCornerPoints();

            foreach (Node node in _cornerPoints)
            {
                node.CheckNeighbors();
            }

            Depth = 50;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            CheckConnections();
            PlaceLineSegments();
            if (_activationId != null)
            {
                string activationString = $"FactoryActivation:{_activationId}";
                Level level = SceneAs<Level>();
                if (level.Session.GetFlag(activationString) || level.Session.GetFlag("Persistent" + activationString))
                {
                    Activated = true;
                }
            }
        }

        private void PlaceLineSegments()
        {
            Node node = _cornerPoints[0];

            while (true)
            {
                string type;
                if (node.Rendered)
                {
                    type = GetTypeString(node);
                    Sprite sprite = AddSpriteWithType(type);
                    sprite.Position = node.Position;
                }
                if (node.Next != null)
                {
                    Vector2 step;

                    if (node.Next.X == node.X)
                    {
                        type = "v";
                        if (node.Y < node.Next.Y)
                        {
                            step = Vector2.UnitY * 8;
                        }
                        else
                        {
                            step = -Vector2.UnitY * 8;
                        }
                    }
                    else
                    {
                        type = "h";
                        if (node.X < node.Next.X)
                        {
                            step = Vector2.UnitX * 8;
                        }
                        else
                        {
                            step = -Vector2.UnitX * 8;
                        }
                    }

                    int stepCount = (int)(Math.Round(Math.Max(Math.Abs(node.Y - node.Next.Y), Math.Abs(node.X - node.Next.X))) / 8) - 1;

                    for (int i = 0; i < stepCount; i++)
                    {
                        Sprite sprite = AddSpriteWithType(type);
                        sprite.Position = node.Position + step * (i + 1);
                    }

                    node = node.Next;
                }
                else
                {
                    break;
                }
            }
        }

        private Sprite AddSpriteWithType(string type)
        {
            string path = $"/powerLine_{type}";
            Sprite sprite = new Sprite(GFX.Game, "objects/FactoryHelper/powerLine");
            sprite.Add("frames", path);
            sprite.Play("frames");
            sprite.Active = false;
            if (!_startActive)
            {
                sprite.SetAnimationFrame(1);
            }
            _sprites.Add(sprite);
            Add(sprite);
            return sprite;
        }

        private void CheckConnections()
        {
            foreach(PowerLine otherLine in Scene.Entities.FindAll<PowerLine>())
            {
                if (otherLine != this && _startActive == otherLine._startActive && _activationId == otherLine._activationId)
                {
                    foreach (Node thisNode in _cornerPoints)
                    {
                        foreach (Node otherNode in otherLine._cornerPoints)
                        {
                            if (otherNode.Rendered && thisNode.Position == otherNode.Position)
                            {
                                otherNode.Rendered = false;
                                foreach (Direction exitDirection in otherNode.ExitDirections)
                                {
                                    thisNode.ExitDirections.Add(exitDirection);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void NormalizeCornerPoints()
        {
            for (int i = 1; i < _cornerPoints.Length; i++)
            {
                var node = _cornerPoints[i];
                if (node.X != node.Previous.X && node.Y != node.Previous.Y)
                {
                    if (Math.Abs(node.X - node.Previous.X) < Math.Abs(node.Y - node.Previous.Y))
                    {
                        node.X = node.Previous.X;
                    }
                    else
                    {
                        node.Y = node.Previous.Y;
                    }
                }
            }
        }

        private string GetTypeString(Node node)
        {
            if (node.ExitDirections.Count == 1)
            {
                if (node.ExitDirections.Contains(Direction.Down) || node.ExitDirections.Contains(Direction.Up))
                {
                    return "v";
                }
                else
                {
                    return "h";
                }
            }
            else if (node.ExitDirections.Count == 2)
            {
                if (node.ExitDirections.Contains(Direction.Left))
                {
                    if (node.ExitDirections.Contains(Direction.Up))
                    {
                        return "lu";
                    }
                    else if (node.ExitDirections.Contains(Direction.Down))
                    {
                        return "ld";
                    }
                    else
                    {
                        return "h";
                    }
                }
                else if (node.ExitDirections.Contains(Direction.Right))
                {
                    if (node.ExitDirections.Contains(Direction.Up))
                    {
                        return "ru";
                    }
                    else
                    {
                        return "rd";
                    }
                }
                else
                {
                    return "v";
                }
            }
            else if (node.ExitDirections.Count == 3)
            {
                if (!node.ExitDirections.Contains(Direction.Right))
                {
                    return "tl";
                }
                else if (!node.ExitDirections.Contains(Direction.Left))
                {
                    return "tr";
                }
                else if (!node.ExitDirections.Contains(Direction.Down))
                {
                    return "tu";
                }
                else
                {
                    return "td";
                }
            }
            else
            {
                return "c";
            }
        }

        private class Node
        {
            public Vector2 Position;
            public Node Next;
            public Node Previous;
            public bool Rendered = true;
            public readonly HashSet<Direction> ExitDirections;
            public float X { get { return Position.X; } set { Position.X = value; } }
            public float Y { get { return Position.Y; } set { Position.Y = value; } }

            public Node(Vector2 position)
            {
                Position = position;
                ExitDirections = new HashSet<Direction>();
            }

            public void CheckNeighbors()
            {
                if (HasLeftNeighbor())
                {
                    ExitDirections.Add(Direction.Left);
                }
                if (HasRightNeighbor())
                {
                    ExitDirections.Add(Direction.Right);
                }
                if (HasUpNeighbor())
                {
                    ExitDirections.Add(Direction.Up);
                }
                if (HasDownNeighbor())
                {
                    ExitDirections.Add(Direction.Down);
                }
            }

            private bool HasLeftNeighbor()
            {
                bool hasPrevious = (Previous != null && Previous.Position.X < Position.X);
                bool hasNext = (Next != null && Next.Position.X < Position.X);
                return hasPrevious || hasNext;
            }

            private bool HasRightNeighbor()
            {
                bool hasPrevious = (Previous != null && Previous.Position.X > Position.X);
                bool hasNext = (Next != null && Next.Position.X > Position.X);
                return hasPrevious || hasNext;
            }

            private bool HasUpNeighbor()
            {
                bool hasPrevious = (Previous != null && Previous.Position.Y < Position.Y);
                bool hasNext = (Next != null && Next.Position.Y < Position.Y);
                return hasPrevious || hasNext;
            }

            private bool HasDownNeighbor()
            {
                bool hasPrevious = (Previous != null && Previous.Position.Y > Position.Y);
                bool hasNext = (Next != null && Next.Position.Y > Position.Y);
                return hasPrevious || hasNext;
            }
        }
    }
}
