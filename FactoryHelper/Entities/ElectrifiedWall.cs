using Monocle;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Celeste;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;

namespace FactoryHelper.Entities
{
    class ElectrifiedWall : FactorySpike
    {
        public bool Activated
        {
            get
            {
                if (_activationId == null)
                {
                    return !_startActive;
                }
                else
                {
                    Level level = Scene as Level;
                    return level.Session.GetFlag(_activationId) || level.Session.GetFlag("Persistent" + _activationId);
                }
            }
        }

        public float Fade { get; private set; } = 0f;
        
        private static Color[] _electricityColors = new Color[2]
        {
            Calc.HexToColor("fcf579"),
            Calc.HexToColor("8cf7e2")
        };
        private static ParticleType _fizzleParticle = new ParticleType
        {
            Size = 1f,
            Color = Calc.HexToColor("B9FEFE"),
            Color2 = Calc.HexToColor("FFF263"),
            ColorMode = ParticleType.ColorModes.Blink,
            FadeMode = ParticleType.FadeModes.Late,
            SpeedMin = 2f,
            SpeedMax = 16f,
            Acceleration = Vector2.UnitY * 10f,
            DirectionRange = (float)Math.PI * 2f,
            Direction = 0f,
            LifeMin = 0.8f,
            LifeMax = 1.5f
        };
        private static float _particleEmittionPeriodVariance = 0.1f;

        private Color[] _electricityColorsLerped;
        private Vector2 _start;
        private Vector2 _end;
        private string _activationId;
        private VertexPositionColor[] _edgeVerts = new VertexPositionColor[1024];
        private uint _edgeSeed;
        private bool _startActive;
        private float _particleEmittionPeriod;
        private float _baseParticleEmittionPeriod;

        public ElectrifiedWall(EntityData data, Vector2 offset, Directions dir)
            : this(data.Position + offset, GetSize(data, dir), dir, data.Attr("activationId", ""), data.Bool("startActive", true))
        {
        }

        public ElectrifiedWall(Vector2 position, int size, Directions direction, string activationId, bool startActive)
            : base(position, size, direction)
        {
            Depth = 10;

            _baseParticleEmittionPeriod = size / 8 * 0.2f;
            SetParticleEmittionPeriod();

            _activationId = activationId == string.Empty ? null : $"FactoryActivation:{activationId}";
            _startActive = startActive;

            _start = Vector2.Zero;
            Vector2 offset;
            switch (direction)
            {
                default:
                case Directions.Up:
                case Directions.Down:
                    offset = Vector2.UnitX * size;
                    break;
                case Directions.Left:
                case Directions.Right:
                    offset = Vector2.UnitY * size;
                    break;
            }
            _end = offset;

            _electricityColorsLerped = new Color[_electricityColors.Length];

            Add(pc = new PlayerCollider(OnCollide));

            switch (direction)
            {
                case Directions.Up:
                    base.Collider = new Hitbox(size, 6f, 0f, -3f);
                    Add(new LedgeBlocker());
                    break;
                case Directions.Down:
                    base.Collider = new Hitbox(size, 6f, 0f, -3f);
                    break;
                case Directions.Left:
                case Directions.Right:
                    base.Collider = new Hitbox(6f, size, -3f);
                    Add(new LedgeBlocker());
                    break;
            }
        }

        private void SetParticleEmittionPeriod()
        {
            _particleEmittionPeriod = Math.Max(_baseParticleEmittionPeriod - _particleEmittionPeriodVariance / 2 + Calc.Random.NextFloat(_particleEmittionPeriodVariance), 0.05f);
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            string str = Direction.ToString().ToLower();
            List<MTexture> atlasSubtextures = GFX.Game.GetAtlasSubtextures("objects/FactoryHelper/electrifiedWall/knob_" + str);
            for (int j = 0; j < 2; j++)
            {
                MTexture texture = Calc.Random.Choose(atlasSubtextures);
                Image image = new Image(texture);
                switch (Direction)
                {
                    case Directions.Up:
                        image.JustifyOrigin(0.5f, 1f);
                        image.Position = Vector2.UnitX * (j * (size - 8f) + 4f) + Vector2.UnitY;
                        break;
                    case Directions.Down:
                        image.JustifyOrigin(0.5f, 0f);
                        image.Position = Vector2.UnitX * (j * (size - 8f) + 4f) - Vector2.UnitY;
                        break;
                    case Directions.Right:
                        image.JustifyOrigin(0f, 0.5f);
                        image.Position = Vector2.UnitY * (j * (size - 8f) + 4f) - Vector2.UnitX;
                        break;
                    case Directions.Left:
                        image.JustifyOrigin(1f, 0.5f);
                        image.Position = Vector2.UnitY * (j * (size - 8f) + 4f) + Vector2.UnitX;
                        break;
                }
                Add(image);
            }
        }

        public override void Update()
        {
            base.Update();
            if (base.Scene.OnInterval(0.05f))
            {
                _edgeSeed = (uint)Calc.Random.Next();
            }
            if (base.Scene.OnInterval(_particleEmittionPeriod) && (Activated != _startActive))
            {
                Console.WriteLine("Particle Emitted");
                Sparkle();
                SetParticleEmittionPeriod();
            }
        }

        public override void Render()
        {
            base.Render();
            if (Activated != _startActive)
            {
                Camera camera = (base.Scene as Level).Camera;
                if (camera != null)
                {
                    for (int i = 0; i < _electricityColorsLerped.Length; i++)
                    {
                        _electricityColorsLerped[i] = Color.Lerp(_electricityColors[i], Color.White, Fade);
                    }
                    int index = 0;
                    DrawSimpleLightning(ref index, ref _edgeVerts, _edgeSeed, Position, _start, _end, _electricityColorsLerped[0], 1f + Fade * 1f);
                    DrawSimpleLightning(ref index, ref _edgeVerts, _edgeSeed + 1, Position, _start, _end, _electricityColorsLerped[1], 1f + Fade * 1f);
                    if (index > 0)
                    {
                        GameplayRenderer.End();
                        GFX.DrawVertices(camera.Matrix, _edgeVerts, index);
                        GameplayRenderer.Begin();
                    }
                }
            }
        }

        protected new void OnCollide(Player player)
        {
            if (Activated != _startActive)
            {
                switch (Direction)
                {
                    case Directions.Up:
                        player.Die(new Vector2(0f, -1f));
                        break;
                    case Directions.Down:
                        player.Die(new Vector2(0f, 1f));
                        break;
                    case Directions.Left:
                        player.Die(new Vector2(-1f, 0f));
                        break;
                    case Directions.Right:
                        player.Die(new Vector2(1f, 0f));
                        break;
                }
            }
        }
        private void Sparkle()
        {
            if (base.Scene == null)
            {
                return;
            }
            SceneAs<Level>().ParticlesFG.Emit(_fizzleParticle, 1, Position + Collider.Center, new Vector2(Collider.Width/2, Collider.Height / 2));
        }

        private static void DrawSimpleLightning(ref int index, ref VertexPositionColor[] verts, uint seed, Vector2 pos, Vector2 a, Vector2 b, Color color, float thickness = 1f)
        {
            seed = (uint)((int)seed + (a.GetHashCode() + b.GetHashCode()));
            a += pos;
            b += pos;
            float num = (b - a).Length();
            Vector2 vector = (b - a) / num;
            Vector2 vector2 = vector.TurnRight();
            a += vector2;
            b += vector2;
            Vector2 vector3 = a;
            int num2 = (PseudoRand(ref seed) % 2u != 0) ? 1 : (-1);
            float num3 = PseudoRandRange(ref seed, 0f, (float)Math.PI * 2f);
            float num4 = 0f;
            float num5 = (float)index + ((b - a).Length() / 4f + 1f) * 6f;
            while (num5 >= (float)verts.Length)
            {
                Array.Resize(ref verts, verts.Length * 2);
            }
            for (int i = index; (float)i < num5; i++)
            {
                verts[i].Color = color;
            }
            do
            {
                float num6 = PseudoRandRange(ref seed, 0f, 4f);
                num3 += 0.1f;
                num4 += 4f + num6;
                Vector2 vector4 = a + vector * num4;
                if (num4 < num)
                {
                    vector4 += num2 * vector2 * num6 - vector2;
                }
                else
                {
                    vector4 = b;
                }
                verts[index++].Position = new Vector3(vector3 - vector2 * thickness, 0f);
                verts[index++].Position = new Vector3(vector4 - vector2 * thickness, 0f);
                verts[index++].Position = new Vector3(vector4 + vector2 * thickness, 0f);
                verts[index++].Position = new Vector3(vector3 - vector2 * thickness, 0f);
                verts[index++].Position = new Vector3(vector4 + vector2 * thickness, 0f);
                verts[index++].Position = new Vector3(vector3, 0f);
                vector3 = vector4;
                num2 = -num2;
            }
            while (num4 < num);
        }

        private static uint PseudoRand(ref uint seed)
        {
            seed ^= seed << 13;
            seed ^= seed >> 17;
            return seed;
        }

        public static float PseudoRandRange(ref uint seed, float min, float max)
        {
            return min + (float)(double)(PseudoRand(ref seed) & 0x3FF) / 1024f * (max - min);
        }
    }
}
