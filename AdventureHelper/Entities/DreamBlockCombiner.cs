using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.AdventureHelper.Entities
{
    [Tracked(false)]
    class DreamBlockCombiner : Entity
    {
        private struct DreamParticle
        {
            public Vector2 Position;

            public int Layer;

            public Color Color;

            public float TimeOffset;
        }

        private struct Edge
        {
            public DreamBlock Parent;

            public bool Visible;

            public Vector2 A;

            public Vector2 B;

            public Vector2 Min;

            public Vector2 Max;

            public Edge(DreamBlock parent, Vector2 a, Vector2 b)
            {
                Parent = parent;
                Visible = true;
                A = a;
                B = b;
                Min = new Vector2(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y));
                Max = new Vector2(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
            }

            public bool InView(ref Rectangle view)
            {
                return (float)view.Left < Parent.X + Max.X && (float)view.Right > Parent.X + Min.X && (float)view.Top < Parent.Y + Max.Y && (float)view.Bottom > Parent.Y + Min.Y;
            }
        }

        private bool playerHasDreamDash;
        private static readonly Color activeBackColor = Color.Black;
        private static readonly Color disabledBackColor = Calc.HexToColor("1f2e2d");
        private static readonly Color activeLineColor = Color.White;
        private static readonly Color disabledLineColor = Calc.HexToColor("6a8480");
        private float wobbleFrom = Calc.Random.NextFloat((float)Math.PI * 2f);
        private float wobbleTo = Calc.Random.NextFloat((float)Math.PI * 2f);
        private float whiteFill = 0f;
        private float wobbleEase = 0f;
        private Dictionary<DreamBlock, DreamParticle[]> allParticles = new Dictionary<DreamBlock, DreamParticle[]>();
        private List<Edge> edges = new List<Edge>();
        private Vector2 shake;
        private float whiteHeight;
        private float animTimer;
        private MTexture[] particleTextures;

        public DreamBlockCombiner()
        {
            particleTextures = new MTexture[4]
            {
            GFX.Game["objects/dreamblock/particles"].GetSubtexture(14, 0, 7, 7),
            GFX.Game["objects/dreamblock/particles"].GetSubtexture(7, 0, 7, 7),
            GFX.Game["objects/dreamblock/particles"].GetSubtexture(0, 0, 7, 7),
            GFX.Game["objects/dreamblock/particles"].GetSubtexture(7, 0, 7, 7)
            };
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            playerHasDreamDash = SceneAs<Level>().Session.Inventory.DreamDash;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Setup();
        }

        public override void Update()
        {
            base.Update();
            if (playerHasDreamDash)
            {
                wobbleEase += Engine.DeltaTime * 2f;
                if (wobbleEase > 1f)
                {
                    wobbleEase = 0f;
                    wobbleFrom = wobbleTo;
                    wobbleTo = Calc.Random.NextFloat((float)Math.PI * 2f);
                }
            }
        }
        public override void Render()
        {
            Camera camera = SceneAs<Level>().Camera;
            foreach (DreamBlock block in allParticles.Keys)
            {
                if (block.Right < camera.Left || block.Left > camera.Right || block.Bottom < camera.Top || block.Top > camera.Bottom)
                {
                    continue;
                }
                Draw.Rect(block.X, block.Y, block.Width, block.Height, playerHasDreamDash ? activeBackColor : disabledBackColor);
                Vector2 position = SceneAs<Level>().Camera.Position;
                var particles = allParticles[block];
                for (int i = 0; i < particles.Length; i++)
                {
                    int layer = particles[i].Layer;
                    Vector2 position2 = particles[i].Position;
                    position2 += position * (0.3f + 0.25f * (float)layer);
                    position2 = PutInside(block, position2);
                    Color color = particles[i].Color;
                    MTexture mTexture;
                    switch (layer)
                    {
                        case 0:
                            {
                                int num2 = (int)((particles[i].TimeOffset * 4f + animTimer) % 4f);
                                mTexture = particleTextures[3 - num2];
                                break;
                            }
                        case 1:
                            {
                                int num = (int)((particles[i].TimeOffset * 2f + animTimer) % 2f);
                                mTexture = particleTextures[1 + num];
                                break;
                            }
                        default:
                            mTexture = particleTextures[2];
                            break;
                    }
                    if (position2.X >= block.X + 2f && position2.Y >= block.Y + 2f && position2.X < block.Right - 2f && position2.Y < block.Bottom - 2f)
                    {
                        mTexture.DrawCentered(position2 + shake, color);
                    }
                }
                if (whiteFill > 0f)
                {
                    Draw.Rect(block.X + shake.X, block.Y + shake.Y, block.Width, block.Height * whiteHeight, Color.White * whiteFill);
                }
            }
            WobbleLine(shake + new Vector2(base.X, base.Y), shake + new Vector2(base.X + base.Width, base.Y), 0f);
            WobbleLine(shake + new Vector2(base.X + base.Width, base.Y), shake + new Vector2(base.X + base.Width, base.Y + base.Height), 0.7f);
            WobbleLine(shake + new Vector2(base.X + base.Width, base.Y + base.Height), shake + new Vector2(base.X, base.Y + base.Height), 1.5f);
            WobbleLine(shake + new Vector2(base.X, base.Y + base.Height), shake + new Vector2(base.X, base.Y), 2.5f);
            Draw.Rect(shake + new Vector2(base.X, base.Y), 2f, 2f, playerHasDreamDash ? activeLineColor : disabledLineColor);
            Draw.Rect(shake + new Vector2(base.X + base.Width - 2f, base.Y), 2f, 2f, playerHasDreamDash ? activeLineColor : disabledLineColor);
            Draw.Rect(shake + new Vector2(base.X, base.Y + base.Height - 2f), 2f, 2f, playerHasDreamDash ? activeLineColor : disabledLineColor);
            Draw.Rect(shake + new Vector2(base.X + base.Width - 2f, base.Y + base.Height - 2f), 2f, 2f, playerHasDreamDash ? activeLineColor : disabledLineColor);
        }

        public void Setup()
        {
            foreach (DreamBlock block in AdventureHelperModule.Session.DreamBlocksToCombine)
            {
                var particles = new DreamParticle[(int)(block.Width / 8f * (block.Height / 8f) * 0.7f)];
                allParticles[block] = particles;
                for (int i = 0; i < particles.Length; i++)
                {
                    particles[i].Position = new Vector2(Calc.Random.NextFloat(block.Width), Calc.Random.NextFloat(block.Height));
                    particles[i].Layer = Calc.Random.Choose(0, 1, 1, 2, 2, 2);
                    particles[i].TimeOffset = Calc.Random.NextFloat();
                    particles[i].Color = Color.LightGray * (0.5f + (float)particles[i].Layer / 2f * 0.5f);
                    if (playerHasDreamDash)
                    {
                        switch (particles[i].Layer)
                        {
                            case 0:
                                particles[i].Color = Calc.Random.Choose(Calc.HexToColor("FFEF11"), Calc.HexToColor("FF00D0"), Calc.HexToColor("08a310"));
                                break;
                            case 1:
                                particles[i].Color = Calc.Random.Choose(Calc.HexToColor("5fcde4"), Calc.HexToColor("7fb25e"), Calc.HexToColor("E0564C"));
                                break;
                            case 2:
                                particles[i].Color = Calc.Random.Choose(Calc.HexToColor("5b6ee1"), Calc.HexToColor("CC3B3B"), Calc.HexToColor("7daa64"));
                                break;
                        }
                    }
                }
            }
        }

        private void WobbleLine(Vector2 from, Vector2 to, float offset)
        {
            float length = (to - from).Length();
            Vector2 direction = Vector2.Normalize(to - from);
            Vector2 vector = new Vector2(direction.Y, 0f - direction.X);
            Color color = playerHasDreamDash ? activeLineColor : disabledLineColor;
            Color color2 = playerHasDreamDash ? activeBackColor : disabledBackColor;
            if (whiteFill > 0f)
            {
                color = Color.Lerp(color, Color.White, whiteFill);
                color2 = Color.Lerp(color2, Color.White, whiteFill);
            }
            float scaleFactor = 0f;
            int num2 = 16;
            for (int i = 2; (float)i < length - 2f; i += num2)
            {
                float num3 = Lerp(LineAmplitude(wobbleFrom + offset, i), LineAmplitude(wobbleTo + offset, i), wobbleEase);
                if ((float)(i + num2) >= length)
                {
                    num3 = 0f;
                }
                float num4 = Math.Min(num2, length - 2f - (float)i);
                Vector2 vector2 = from + direction * i + vector * scaleFactor;
                Vector2 vector3 = from + direction * ((float)i + num4) + vector * num3;
                Draw.Line(vector2 - vector, vector3 - vector, color2);
                Draw.Line(vector2 - vector * 2f, vector3 - vector * 2f, color2);
                Draw.Line(vector2, vector3, color);
                scaleFactor = num3;
            }
        }
        private float Lerp(float a, float b, float percent)
        {
            return a + (b - a) * percent;
        }
        private float LineAmplitude(float seed, float index)
        {
            return (float)(Math.Sin((double)(seed + index / 16f) + Math.Sin(seed * 2f + index / 32f) * 6.2831854820251465) + 1.0) * 1.5f;
        }

        private Vector2 PutInside(DreamBlock block, Vector2 pos)
        {
            while (pos.X < block.X)
            {
                pos.X += block.Width;
            }
            while (pos.X > block.X + block.Width)
            {
                pos.X -= block.Width;
            }
            while (pos.Y < block.Y)
            {
                pos.Y += block.Height;
            }
            while (pos.Y > block.Y + block.Height)
            {
                pos.Y -= block.Height;
            }
            return pos;
        }

    }
}
