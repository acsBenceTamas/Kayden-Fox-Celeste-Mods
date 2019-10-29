using Celeste;
using Celeste.Mod.Entities;
using FactoryHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace FactoryHelper.Entities
{
    [CustomEntity("FactoryHelper/PowerLine")]
    [Tracked]
    class PowerLine : Entity
    {
        public FactoryActivator Activator;

        private enum Direction
        {
            Up,
            Down,
            Left,
            Right
        }

        private Node[] _cornerPoints;
        private List<Image> _images = new List<Image>();
        private Color _defaultColor;

        public PowerLine(EntityData entityData, Vector2 offset) : 
            this(entityData.Position,
                 offset,
                 entityData.Nodes,
                 entityData.Attr("activationId", ""),
                 entityData.Attr("colorCode", "00dd00"),
                 entityData.Bool("startActive", false))
        {
        }

        public PowerLine(Vector2 position, Vector2 offset, Vector2[] nodes, string activationId, string colorCode, bool startActive)
        {
            Position = offset;
            Add(Activator = new FactoryActivator());
            Activator.ActivationId = activationId == string.Empty ? null : activationId;
            Activator.StartOn = startActive;
            Activator.OnStartOn = PowerUp;
            Activator.OnTurnOn = PowerUp;
            Activator.OnStartOff = PowerDown;
            Activator.OnTurnOff = PowerDown;

            _defaultColor = Calc.HexToColor(colorCode);

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

            Depth = 10000;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            CheckConnections();
            PlaceLineSegments();
            Activator.HandleStartup(scene);
        }

        private void PowerUp()
        {
            foreach (var sprite in _images)
            {
                sprite.Color = _defaultColor * 0.5f;
            }
        }

        private void PowerDown()
        {
            foreach (var sprite in _images)
            {
                sprite.Color = _defaultColor * 0.1f;
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
                    Image baseImage = AddSpriteWithType(type);
                    baseImage.Position = node.Position;
                    Image lightImage = AddSpriteWithType(type, true);
                    lightImage.Position = node.Position;
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
                        Image baseImage = AddSpriteWithType(type);
                        baseImage.Position = node.Position + step * (i + 1);
                        Image lightImage = AddSpriteWithType(type, true);
                        lightImage.Position = node.Position + step * (i + 1);
                    }

                    node = node.Next;
                }
                else
                {
                    break;
                }
            }
        }

        private Image AddSpriteWithType(string type, bool isLight = false)
        {
            Image image = new Image(GFX.Game[$"objects/FactoryHelper/powerLine/powerLine{(isLight ? "Light" : "")}_{type}"]);
            if (isLight)
            {
                _images.Add(image);
            }
            Add(image);
            return image;
        }

        private void CheckConnections()
        {
            foreach(PowerLine otherLine in Scene.Entities.FindAll<PowerLine>())
            {
                if (otherLine != this && Activator.StartOn == otherLine.Activator.StartOn && Activator.ActivationId == otherLine.Activator.ActivationId)
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
