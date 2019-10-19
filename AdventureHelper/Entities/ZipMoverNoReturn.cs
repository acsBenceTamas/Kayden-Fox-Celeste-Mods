using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.AdventureHelper.Entities
{
    public class ZipMoverNoReturn : Solid
    {
        public ZipMoverNoReturn(Vector2 position, int width, int height, Vector2 target, float speedMultiplier) : base(position, (float)width, (float)height, false)
        {
            this.speedMultiplier = speedMultiplier;
            this.edges = new MTexture[3, 3];
            this.innerCogs = GFX.Game.GetAtlasSubtextures("objects/AdventureHelper/noreturnzipmover/innercog");
            this.temp = new MTexture();
            this.percent = 0f;
            this.sfx = new SoundSource();
            base.Depth = -9999;
            this.start = this.Position;
            this.target = target;
            this.firstDirection = true;
            base.Add(new Coroutine(this.Sequence(), true));
            base.Add(new LightOcclude(1f));
            base.Add(this.streetlight = new Sprite(GFX.Game, "objects/zipmover/light"));
            this.streetlight.Add("frames", "", 1f);
            this.streetlight.Play("frames", false, false);
            this.streetlight.Active = false;
            this.streetlight.SetAnimationFrame(1);
            this.streetlight.Position = new Vector2(base.Width / 2f - this.streetlight.Width / 2f, base.Height / 2f - this.streetlight.Height /2f);
            base.Add(this.bloom = new BloomPoint(1f, 6f));
            this.bloom.Position = new Vector2(base.Width / 2f, base.Height / 2f - this.streetlight.Height / 2f + 3f);
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    this.edges[i, j] = GFX.Game["objects/AdventureHelper/noreturnzipmover/block"].GetSubtexture(i * 8, j * 8, 8, 8, null);
                }
            }
            this.SurfaceSoundIndex = 7;
            this.sfx.Position = new Vector2(base.Width, base.Height) / 2f;
            base.Add(this.sfx);
        }

        public ZipMoverNoReturn(EntityData data, Vector2 offset) : this(data.Position + offset, data.Width, data.Height, data.Nodes[0] + offset, data.Float("speedMultiplier", 1f))
        {
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Add(this.pathRenderer = new ZipMoverNoReturn.ZipMoverPathRenderer(this));
        }

        public override void Removed(Scene scene)
        {
            scene.Remove(this.pathRenderer);
            this.pathRenderer = null;
            base.Removed(scene);
        }

        public override void Update()
        {
            base.Update();
            this.bloom.Y = (float)(base.Height /2f - this.streetlight.Height / 2f + this.streetlight.CurrentAnimationFrame * 3);
        }

        public override void Render()
        {
            Vector2 position = this.Position;
            this.Position += base.Shake;
            Draw.Rect(base.X, base.Y, base.Width, base.Height, Color.Black);
            int num = 1;
            float num2 = 0f;
            int count = this.innerCogs.Count;
            int num3 = 4;
            while ((float)num3 <= base.Height - 4f)
            {
                int num4 = num;
                int num5 = 4;
                while ((float)num5 <= base.Width - 4f)
                {
                    int index = (int)(this.mod((num2 + (float)num * this.percent * 3.14159274f * 4f) / 1.57079637f, 1f) * (float)count);
                    MTexture mtexture = this.innerCogs[index];
                    Rectangle rectangle = new Rectangle(0, 0, mtexture.Width, mtexture.Height);
                    Vector2 zero = Vector2.Zero;
                    bool flag = num5 <= 4;
                    if (flag)
                    {
                        zero.X = 2f;
                        rectangle.X = 2;
                        rectangle.Width -= 2;
                    }
                    else
                    {
                        bool flag2 = (float)num5 >= base.Width - 4f;
                        if (flag2)
                        {
                            zero.X = -2f;
                            rectangle.Width -= 2;
                        }
                    }
                    bool flag3 = num3 <= 4;
                    if (flag3)
                    {
                        zero.Y = 2f;
                        rectangle.Y = 2;
                        rectangle.Height -= 2;
                    }
                    else
                    {
                        bool flag4 = (float)num3 >= base.Height - 4f;
                        if (flag4)
                        {
                            zero.Y = -2f;
                            rectangle.Height -= 2;
                        }
                    }
                    mtexture = mtexture.GetSubtexture(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, this.temp);
                    mtexture.DrawCentered(this.Position + new Vector2((float)num5, (float)num3) + zero, Color.White * ((num < 0) ? 0.5f : 1f));
                    num = -num;
                    num2 += 1.04719758f;
                    num5 += 8;
                }
                bool flag5 = num4 == num;
                if (flag5)
                {
                    num = -num;
                }
                num3 += 8;
            }
            int num6 = 0;
            while ((float)num6 < base.Width / 8f)
            {
                int num7 = 0;
                while ((float)num7 < base.Height / 8f)
                {
                    int num8 = (num6 == 0) ? 0 : (((float)num6 == base.Width / 8f - 1f) ? 2 : 1);
                    int num9 = (num7 == 0) ? 0 : (((float)num7 == base.Height / 8f - 1f) ? 2 : 1);
                    bool flag6 = num8 != 1 || num9 != 1;
                    if (flag6)
                    {
                        this.edges[num8, num9].Draw(new Vector2(base.X + (float)(num6 * 8), base.Y + (float)(num7 * 8)));
                    }
                    num7++;
                }
                num6++;
            }
            base.Render();
            this.Position = position;
        }

        private void ScrapeParticlesCheck(Vector2 to)
        {
            bool flag = base.Scene.OnInterval(0.03f);
            if (flag)
            {
                bool flag2 = to.Y != base.ExactPosition.Y;
                bool flag3 = to.X != base.ExactPosition.X;
                bool flag4 = flag2 && !flag3;
                if (flag4)
                {
                    int num = Math.Sign(to.Y - base.ExactPosition.Y);
                    bool flag5 = num == 1;
                    Vector2 value;
                    if (flag5)
                    {
                        value = base.BottomLeft;
                    }
                    else
                    {
                        value = base.TopLeft;
                    }
                    int num2 = 4;
                    bool flag6 = num == 1;
                    if (flag6)
                    {
                        num2 = Math.Min((int)base.Height - 12, 20);
                    }
                    int num3 = (int)base.Height;
                    bool flag7 = num == -1;
                    if (flag7)
                    {
                        num3 = Math.Max(16, (int)base.Height - 16);
                    }
                    bool flag8 = base.Scene.CollideCheck<Solid>(value + new Vector2(-2f, (float)(num * -2)));
                    if (flag8)
                    {
                        for (int i = num2; i < num3; i += 8)
                        {
                            base.SceneAs<Level>().ParticlesFG.Emit(ZipMoverNoReturn.P_Scrape, base.TopLeft + new Vector2(0f, (float)i + (float)num * 2f), (num == 1) ? -0.7853982f : 0.7853982f);
                        }
                    }
                    bool flag9 = base.Scene.CollideCheck<Solid>(value + new Vector2(base.Width + 2f, (float)(num * -2)));
                    if (flag9)
                    {
                        for (int j = num2; j < num3; j += 8)
                        {
                            base.SceneAs<Level>().ParticlesFG.Emit(ZipMoverNoReturn.P_Scrape, base.TopRight + new Vector2(-1f, (float)j + (float)num * 2f), (num == 1) ? -2.3561945f : 2.3561945f);
                        }
                    }
                }
                else
                {
                    bool flag10 = flag3 && !flag2;
                    if (flag10)
                    {
                        int num4 = Math.Sign(to.X - base.ExactPosition.X);
                        bool flag11 = num4 == 1;
                        Vector2 value2;
                        if (flag11)
                        {
                            value2 = base.TopRight;
                        }
                        else
                        {
                            value2 = base.TopLeft;
                        }
                        int num5 = 4;
                        bool flag12 = num4 == 1;
                        if (flag12)
                        {
                            num5 = Math.Min((int)base.Width - 12, 20);
                        }
                        int num6 = (int)base.Width;
                        bool flag13 = num4 == -1;
                        if (flag13)
                        {
                            num6 = Math.Max(16, (int)base.Width - 16);
                        }
                        bool flag14 = base.Scene.CollideCheck<Solid>(value2 + new Vector2((float)(num4 * -2), -2f));
                        if (flag14)
                        {
                            for (int k = num5; k < num6; k += 8)
                            {
                                base.SceneAs<Level>().ParticlesFG.Emit(ZipMoverNoReturn.P_Scrape, base.TopLeft + new Vector2((float)k + (float)num4 * 2f, -1f), (num4 == 1) ? 2.3561945f : 0.7853982f);
                            }
                        }
                        bool flag15 = base.Scene.CollideCheck<Solid>(value2 + new Vector2((float)(num4 * -2), base.Height + 2f));
                        if (flag15)
                        {
                            for (int l = num5; l < num6; l += 8)
                            {
                                base.SceneAs<Level>().ParticlesFG.Emit(ZipMoverNoReturn.P_Scrape, base.BottomLeft + new Vector2((float)l + (float)num4 * 2f, 0f), (num4 == 1) ? -2.3561945f : -0.7853982f);
                            }
                        }
                    }
                }
            }
        }

        private IEnumerator Sequence()
        {
            Vector2 start = this.Position;
            for (; ; )
            {
                while (!this.HasPlayerRider())
                {
                    yield return null;
                }
                this.sfx.Play("event:/game/01_forsaken_city/zip_mover", null, 0f);
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
                this.StartShaking(0.1f);
                yield return 0.1f;
                this.streetlight.SetAnimationFrame(3);
                this.StopPlayerRunIntoAnimation = false;
                float at = 0f;
                while (at < 1f)
                {
                    yield return null;
                    at = Calc.Approach(at, 1f, 2f * Engine.DeltaTime * speedMultiplier);
                    this.percent = Ease.SineIn(at);
                    Vector2 to;
                    if (firstDirection)
                    {
                        to = Vector2.Lerp(start, this.target, this.percent);
                    }
                    else
                    {
                        to = Vector2.Lerp(this.target, start, this.percent);
                    }
                    this.ScrapeParticlesCheck(to);
                    bool flag = this.Scene.OnInterval(0.1f);
                    if (flag)
                    {
                        this.pathRenderer.CreateSparks();
                    }
                    this.MoveTo(to);
                    to = default(Vector2);
                }
                this.StartShaking(0.2f);
                Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
                this.SceneAs<Level>().Shake(0.3f);
                this.StopPlayerRunIntoAnimation = true;
                this.streetlight.SetAnimationFrame(2);
                yield return 0.5f;
                this.sfx.Stop();
                this.StopPlayerRunIntoAnimation = false;
                this.streetlight.SetAnimationFrame(1);
                this.firstDirection = !this.firstDirection;
            }
        }

        private float mod(float x, float m)
        {
            return (x % m + m) % m;
        }

        static ZipMoverNoReturn()
        {
            ZipMoverNoReturn.ropeColor = Calc.HexToColor("d1d1d1");
            ZipMoverNoReturn.ropeLightColor = Calc.HexToColor("9e9e9e");
        }

        private float speedMultiplier;

        private MTexture[,] edges;

        private Sprite streetlight;

        private BloomPoint bloom;

        private ZipMoverNoReturn.ZipMoverPathRenderer pathRenderer;

        public static ParticleType P_Scrape = ZipMover.P_Scrape;

        public static ParticleType P_Sparks = ZipMover.P_Sparks;

        private List<MTexture> innerCogs;

        private MTexture temp;

        private Vector2 start;

        private Vector2 target;

        private float percent;

        private static readonly Color ropeColor;

        private static readonly Color ropeLightColor;

        private SoundSource sfx;

        private bool firstDirection;

        private class ZipMoverPathRenderer : Entity
        {
            public ZipMoverPathRenderer(ZipMoverNoReturn zipMover) : base()
            {
                this.cog = GFX.Game["objects/AdventureHelper/noreturnzipmover/cog"];
                base.Depth = 5000;
                this.ZipMover = zipMover;
                this.from = this.ZipMover.start + new Vector2(this.ZipMover.Width / 2f, this.ZipMover.Height / 2f);
                this.to = this.ZipMover.target + new Vector2(this.ZipMover.Width / 2f, this.ZipMover.Height / 2f);
                this.sparkAdd = (this.from - this.to).SafeNormalize(5f).Perpendicular();
                float num = (this.from - this.to).Angle();
                this.sparkDirFromA = num + 0.3926991f;
                this.sparkDirFromB = num - 0.3926991f;
                this.sparkDirToA = num + 3.14159274f - 0.3926991f;
                this.sparkDirToB = num + 3.14159274f + 0.3926991f;
            }

            public void CreateSparks()
            {
                base.SceneAs<Level>().ParticlesBG.Emit(ZipMoverNoReturn.P_Sparks, this.from + this.sparkAdd + Calc.Random.Range(-Vector2.One, Vector2.One), this.sparkDirFromA);
                base.SceneAs<Level>().ParticlesBG.Emit(ZipMoverNoReturn.P_Sparks, this.from - this.sparkAdd + Calc.Random.Range(-Vector2.One, Vector2.One), this.sparkDirFromB);
                base.SceneAs<Level>().ParticlesBG.Emit(ZipMoverNoReturn.P_Sparks, this.to + this.sparkAdd + Calc.Random.Range(-Vector2.One, Vector2.One), this.sparkDirToA);
                base.SceneAs<Level>().ParticlesBG.Emit(ZipMoverNoReturn.P_Sparks, this.to - this.sparkAdd + Calc.Random.Range(-Vector2.One, Vector2.One), this.sparkDirToB);
            }

            public override void Render()
            {
                this.DrawCogs(Vector2.UnitY, new Color?(Color.Black));
                this.DrawCogs(Vector2.Zero, null);
                Draw.Rect(new Rectangle((int)(this.ZipMover.X - 1f), (int)(this.ZipMover.Y - 1f), (int)this.ZipMover.Width + 2, (int)this.ZipMover.Height + 2), Color.Black);
            }

            private void DrawCogs(Vector2 offset, Color? colorOverride = null)
            {
                Vector2 vector = (this.to - this.from).SafeNormalize();
                Vector2 value = vector.Perpendicular() * 3f;
                Vector2 value2 = -vector.Perpendicular() * 4f;
                float rotation = this.ZipMover.percent * 3.14159274f * 2f;
                Draw.Line(this.from + value + offset, this.to + value + offset, (colorOverride != null) ? colorOverride.Value : ZipMoverNoReturn.ropeColor);
                Draw.Line(this.from + value2 + offset, this.to + value2 + offset, (colorOverride != null) ? colorOverride.Value : ZipMoverNoReturn.ropeColor);
                for (float num = 4f - this.ZipMover.percent * 3.14159274f * 8f % 4f; num < (this.to - this.from).Length(); num += 4f)
                {
                    Vector2 value3 = this.from + value + vector.Perpendicular() + vector * num;
                    Vector2 value4 = this.to + value2 - vector * num;
                    Draw.Line(value3 + offset, value3 + vector * 2f + offset, (colorOverride != null) ? colorOverride.Value : ZipMoverNoReturn.ropeLightColor);
                    Draw.Line(value4 + offset, value4 - vector * 2f + offset, (colorOverride != null) ? colorOverride.Value : ZipMoverNoReturn.ropeLightColor);
                }
                this.cog.DrawCentered(this.from + offset, (colorOverride != null) ? colorOverride.Value : Color.White, 1f, rotation);
                this.cog.DrawCentered(this.to + offset, (colorOverride != null) ? colorOverride.Value : Color.White, 1f, rotation);
            }
            public ZipMoverNoReturn ZipMover;

            private MTexture cog;

            private Vector2 from;

            private Vector2 to;

            private Vector2 sparkAdd;

            private float sparkDirFromA;

            private float sparkDirFromB;

            private float sparkDirToA;

            private float sparkDirToB;
        }
    }
}
