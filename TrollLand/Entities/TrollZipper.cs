using Celeste;
using Celeste.Mod.Entities;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace TrollLand.Entities
{
    [CustomEntity("TrollLand/TrollZipMover")]
    class TrollZipper : Solid
    {
        private enum TrollBehaviors
        {
            ZipMover,
            FallBlock,
            SwapBlock
        }

        public enum Themes
        {
            Normal,
            Moon
        }

        private class ZipMoverPathRenderer : Entity
        {
            public TrollZipper ZipMover;

            private MTexture cog;

            private Vector2 from;

            private Vector2 to;

            private Vector2 sparkAdd;

            private float sparkDirFromA;

            private float sparkDirFromB;

            private float sparkDirToA;

            private float sparkDirToB;

            public ZipMoverPathRenderer(TrollZipper zipMover)
            {
                base.Depth = 5000;
                ZipMover = zipMover;
                from = ZipMover.start + new Vector2(ZipMover.Width / 2f, ZipMover.Height / 2f);
                to = ZipMover.target + new Vector2(ZipMover.Width / 2f, ZipMover.Height / 2f);
                sparkAdd = (from - to).SafeNormalize(5f).Perpendicular();
                float num = (from - to).Angle();
                sparkDirFromA = num + (float)Math.PI / 8f;
                sparkDirFromB = num - (float)Math.PI / 8f;
                sparkDirToA = num + (float)Math.PI - (float)Math.PI / 8f;
                sparkDirToB = num + (float)Math.PI + (float)Math.PI / 8f;
                if (zipMover.theme == Themes.Moon)
                {
                    cog = GFX.Game["objects/zipmover/moon/cog"];
                }
                else
                {
                    cog = GFX.Game["objects/zipmover/cog"];
                }
            }

            public void CreateSparks()
            {
                SceneAs<Level>().ParticlesBG.Emit(P_Sparks, from + sparkAdd + Calc.Random.Range(-Vector2.One, Vector2.One), sparkDirFromA);
                SceneAs<Level>().ParticlesBG.Emit(P_Sparks, from - sparkAdd + Calc.Random.Range(-Vector2.One, Vector2.One), sparkDirFromB);
                SceneAs<Level>().ParticlesBG.Emit(P_Sparks, to + sparkAdd + Calc.Random.Range(-Vector2.One, Vector2.One), sparkDirToA);
                SceneAs<Level>().ParticlesBG.Emit(P_Sparks, to - sparkAdd + Calc.Random.Range(-Vector2.One, Vector2.One), sparkDirToB);
            }

            public override void Render()
            {
                DrawCogs(Vector2.UnitY, Color.Black);
                DrawCogs(Vector2.Zero);
                if (ZipMover.drawBlackBorder)
                {
                    Draw.Rect(new Rectangle((int)(ZipMover.X + ZipMover.Shake.X - 1f), (int)(ZipMover.Y + ZipMover.Shake.Y - 1f), (int)ZipMover.Width + 2, (int)ZipMover.Height + 2), Color.Black);
                }
            }

            private void DrawCogs(Vector2 offset, Color? colorOverride = null)
            {
                Vector2 vector = (to - from).SafeNormalize();
                Vector2 value = vector.Perpendicular() * 3f;
                Vector2 value2 = -vector.Perpendicular() * 4f;
                float rotation = ZipMover.percent * (float)Math.PI * 2f;
                Draw.Line(from + value + offset, to + value + offset, colorOverride.HasValue ? colorOverride.Value : ropeColor);
                Draw.Line(from + value2 + offset, to + value2 + offset, colorOverride.HasValue ? colorOverride.Value : ropeColor);
                for (float num = 4f - ZipMover.percent * (float)Math.PI * 8f % 4f; num < (to - from).Length(); num += 4f)
                {
                    Vector2 value3 = from + value + vector.Perpendicular() + vector * num;
                    Vector2 value4 = to + value2 - vector * num;
                    Draw.Line(value3 + offset, value3 + vector * 2f + offset, colorOverride.HasValue ? colorOverride.Value : ropeLightColor);
                    Draw.Line(value4 + offset, value4 - vector * 2f + offset, colorOverride.HasValue ? colorOverride.Value : ropeLightColor);
                }
                cog.DrawCentered(from + offset, colorOverride.HasValue ? colorOverride.Value : Color.White, 1f, rotation);
                cog.DrawCentered(to + offset, colorOverride.HasValue ? colorOverride.Value : Color.White, 1f, rotation);
            }
        }

        // Zip Mover

        public static ParticleType P_Scrape = ZipMover.P_Scrape;

        public static ParticleType P_Sparks = ZipMover.P_Sparks;

        // Fall Block

        public static ParticleType P_FallDustA = FallingBlock.P_FallDustA;

        public static ParticleType P_FallDustB = FallingBlock.P_FallDustB;

        public static ParticleType P_LandDust = FallingBlock.P_LandDust;

        // Swap Block
        
        public static ParticleType P_Move = SwapBlock.P_Move;

        private const float ReturnTime = 0.8f;

        // Zip Mover

        private Themes theme;

        private MTexture[,] edges = new MTexture[3, 3];

        private Sprite streetlight;

        private BloomPoint bloom;

        private ZipMoverPathRenderer pathRenderer;

        private List<MTexture> innerCogs;

        private MTexture temp = new MTexture();

        private bool drawBlackBorder;

        private Vector2 start;

        private Vector2 target;

        private float percent = 0f;

        private static readonly Color ropeColor = Calc.HexToColor("663931");

        private static readonly Color ropeLightColor = Calc.HexToColor("9b6157");

        private SoundSource sfx = new SoundSource();

        // Fall Block

        public bool Triggered;

        public float FallDelay;

        public bool HasStartedFalling
        {
            get;
            private set;
        }

        private bool climbFall = true;

        // Swap Block
        
        public Vector2 Direction;

        public bool Swapping;

        private int lerpTarget;

        private float speed;

        private float maxForwardSpeed;

        private float maxBackwardSpeed;

        private float returnTimer;

        private EventInstance moveSfx;

        private EventInstance returnSfx;

        private DisplacementRenderer.Burst burst;

        private float particlesRemainder;

        private float lerp;

        // Troll Zip Mover

        private int trollOffset;

        private TrollBehaviors trollBehavior;

        private bool flipDirection;

        private bool switchTroll;

        private bool trollFixed;

        private Coroutine coroutine;

        private DashListener dashListener;

        public TrollZipper(Vector2 position, int width, int height, Vector2 target, Themes theme, int trollOffset, bool trollFixed, bool switchTroll, bool flipDirection)
            : base(position, width, height, safe: false)
        {
            this.flipDirection = flipDirection;
            this.switchTroll = switchTroll;
            this.trollFixed = trollFixed;
            this.trollOffset = trollOffset;
            Depth = -9999;
            start = Position;
            this.target = target;
            maxForwardSpeed = 360f / Vector2.Distance(start, target);
            maxBackwardSpeed = maxForwardSpeed * 0.4f;
            Direction.X = Math.Sign(target.X - start.X);
            Direction.Y = Math.Sign(target.Y - start.Y);
            this.theme = theme;
            string path;
            string id;
            string key;
            if (theme == Themes.Moon)
            {
                path = "objects/zipmover/moon/light";
                id = "objects/zipmover/moon/block";
                key = "objects/zipmover/moon/innercog";
                drawBlackBorder = false;
            }
            else
            {
                path = "objects/zipmover/light";
                id = "objects/zipmover/block";
                key = "objects/zipmover/innercog";
                drawBlackBorder = true;
            }
            innerCogs = GFX.Game.GetAtlasSubtextures(key);
            Add(streetlight = new Sprite(GFX.Game, path));
            streetlight.Add("frames", "", 1f);
            streetlight.Play("frames");
            streetlight.Active = false;
            streetlight.SetAnimationFrame(1);
            streetlight.Position = new Vector2(Width / 2f - streetlight.Width / 2f, 0f);
            Add(bloom = new BloomPoint(1f, 6f));
            bloom.Position = new Vector2(Width / 2f, 4f);
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    edges[i, j] = GFX.Game[id].GetSubtexture(i * 8, j * 8, 8, 8);
                }
            }
            SurfaceSoundIndex = 7;
            sfx.Position = new Vector2(Width, Height) / 2f;
            Add(sfx);
            Add(new LightOcclude());
        }

        public TrollZipper(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Width, data.Height, data.Nodes[0] + offset, data.Enum("theme", Themes.Normal), data.Int("trollOffset"), data.Bool("trollFixed", false), data.Bool("switchTroll", false), data.Bool("flipDirection", false))
        {
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Add(pathRenderer = new ZipMoverPathRenderer(this));
            if (flipDirection)
            {
                target = Position - (target - Position);
            }
            SetupTrolls();
        }

        private void SetupTrolls()
        {
            Remove(coroutine);
            coroutine = null;
            Remove(dashListener);
            dashListener = null;
            Level level = Scene as Level;
            int modifier = ((!trollFixed ? TrollLandModule.Session.DeathCountForLevel(level.Session.Level) : 0) + trollOffset) % 3;
            trollBehavior = (TrollBehaviors)modifier;
            switch (trollBehavior)
            {
                default:
                case TrollBehaviors.ZipMover:
                    Add(coroutine = new Coroutine(ZipMoverSequence()));
                    break;
                case TrollBehaviors.FallBlock:
                    Add(coroutine = new Coroutine(FallBlockSequence()));
                    break;
                case TrollBehaviors.SwapBlock:
                    Add(dashListener = new DashListener() { OnDash = OnDash });
                    break;
            }
        }

        public override void Removed(Scene scene)
        {
            if (trollBehavior != TrollBehaviors.FallBlock)
            {
                scene.Remove(pathRenderer);
                pathRenderer = null;
            }
            base.Removed(scene);
            Audio.Stop(moveSfx);
            Audio.Stop(returnSfx);
        }
        
        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);
            Audio.Stop(moveSfx);
            Audio.Stop(returnSfx);
        }

        private void OnDash(Vector2 direction)
        {
            Swapping = (lerp < 1f);
            lerpTarget = 1;
            returnTimer = 0.8f;
            burst = (base.Scene as Level).Displacement.AddBurst(base.Center, 0.2f, 0f, 16f);
            if (lerp >= 0.2f)
            {
                speed = maxForwardSpeed;
            }
            else
            {
                speed = MathHelper.Lerp(maxForwardSpeed * 0.333f, maxForwardSpeed, lerp / 0.2f);
            }
            Audio.Stop(returnSfx);
            Audio.Stop(moveSfx);
            if (!Swapping)
            {
                Audio.Play("event:/game/05_mirror_temple/swapblock_move_end", base.Center);
            }
            else
            {
                moveSfx = Audio.Play("event:/game/05_mirror_temple/swapblock_move", base.Center);
            }
        }

        public override void Update()
        {
            base.Update();
            bloom.Y = streetlight.CurrentAnimationFrame * 3;
            if (trollBehavior == TrollBehaviors.SwapBlock)
            {

                if (returnTimer > 0f)
                {
                    returnTimer -= Engine.DeltaTime;
                    if (returnTimer <= 0f)
                    {
                        lerpTarget = 0;
                        speed = 0f;
                        returnSfx = Audio.Play("event:/game/05_mirror_temple/swapblock_return", base.Center);
                    }
                }
                if (burst != null)
                {
                    burst.Position = base.Center;
                }
                if (lerpTarget == 1)
                {
                    speed = Calc.Approach(speed, maxForwardSpeed, maxForwardSpeed / 0.2f * Engine.DeltaTime);
                }
                else
                {
                    speed = Calc.Approach(speed, maxBackwardSpeed, maxBackwardSpeed / 1.5f * Engine.DeltaTime);
                }
                float num = lerp;
                lerp = Calc.Approach(lerp, lerpTarget, speed * Engine.DeltaTime);
                if (lerp != num)
                {
                    Vector2 liftSpeed = (target - start) * speed;
                    Vector2 position = Position;
                    if (lerpTarget == 1)
                    {
                        liftSpeed = (target - start) * maxForwardSpeed;
                    }
                    if (lerp < num)
                    {
                        liftSpeed *= -1f;
                    }
                    if (lerpTarget == 1 && base.Scene.OnInterval(0.02f))
                    {
                        MoveParticles(target - start);
                    }
                    MoveTo(Vector2.Lerp(start, target, lerp), liftSpeed);
                    if (position != Position)
                    {
                        Audio.Position(moveSfx, base.Center);
                        Audio.Position(returnSfx, base.Center);
                        if (Position == start && lerpTarget == 0)
                        {
                            Audio.SetParameter(returnSfx, "end", 1f);
                            Audio.Play("event:/game/05_mirror_temple/swapblock_return_end", base.Center);
                            TrySwitchTroll();
                        }
                        else if (Position == target && lerpTarget == 1)
                        {
                            Audio.Play("event:/game/05_mirror_temple/swapblock_move_end", base.Center);
                        }
                    }
                }
                if (Swapping && lerp >= 1f)
                {
                    Swapping = false;
                }
                StopPlayerRunIntoAnimation = (lerp <= 0f || lerp >= 1f);
            }
        }

        private void TrySwitchTroll()
        {
            if (switchTroll)
            {
                trollOffset++;
                SetupTrolls();
            }
        }

        public override void Render()
        {
            Vector2 position = Position;
            Position += Shake;
            Draw.Rect(base.X + 1f, base.Y + 1f, base.Width - 2f, base.Height - 2f, Color.Black);
            int num = 1;
            float num2 = 0f;
            int count = innerCogs.Count;
            for (int i = 4; (float)i <= base.Height - 4f; i += 8)
            {
                int num3 = num;
                for (int j = 4; (float)j <= base.Width - 4f; j += 8)
                {
                    int index = (int)(mod((num2 + (float)num * percent * (float)Math.PI * 4f) / ((float)Math.PI / 2f), 1f) * (float)count);
                    MTexture mTexture = innerCogs[index];
                    Rectangle rectangle = new Rectangle(0, 0, mTexture.Width, mTexture.Height);
                    Vector2 zero = Vector2.Zero;
                    if (j <= 4)
                    {
                        zero.X = 2f;
                        rectangle.X = 2;
                        rectangle.Width -= 2;
                    }
                    else if ((float)j >= base.Width - 4f)
                    {
                        zero.X = -2f;
                        rectangle.Width -= 2;
                    }
                    if (i <= 4)
                    {
                        zero.Y = 2f;
                        rectangle.Y = 2;
                        rectangle.Height -= 2;
                    }
                    else if ((float)i >= base.Height - 4f)
                    {
                        zero.Y = -2f;
                        rectangle.Height -= 2;
                    }
                    mTexture = mTexture.GetSubtexture(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, temp);
                    mTexture.DrawCentered(Position + new Vector2(j, i) + zero, Color.White * ((num < 0) ? 0.5f : 1f));
                    num = -num;
                    num2 += (float)Math.PI / 3f;
                }
                if (num3 == num)
                {
                    num = -num;
                }
            }
            for (int k = 0; (float)k < base.Width / 8f; k++)
            {
                for (int l = 0; (float)l < base.Height / 8f; l++)
                {
                    int num4 = (k != 0) ? (((float)k != base.Width / 8f - 1f) ? 1 : 2) : 0;
                    int num5 = (l != 0) ? (((float)l != base.Height / 8f - 1f) ? 1 : 2) : 0;
                    if (num4 != 1 || num5 != 1)
                    {
                        edges[num4, num5].Draw(new Vector2(base.X + (float)(k * 8), base.Y + (float)(l * 8)));
                    }
                }
            }
            base.Render();
            Position = position;
        }

        public override void OnStaticMoverTrigger(StaticMover sm)
        {
             Triggered = true;
        }

        private void MoveParticles(Vector2 normal)
        {
            Vector2 position;
            Vector2 positionRange;
            float direction;
            float num;
            if (normal.X > 0f)
            {
                position = base.CenterLeft;
                positionRange = Vector2.UnitY * (base.Height - 6f);
                direction = (float)Math.PI;
                num = Math.Max(2f, base.Height / 14f);
            }
            else if (normal.X < 0f)
            {
                position = base.CenterRight;
                positionRange = Vector2.UnitY * (base.Height - 6f);
                direction = 0f;
                num = Math.Max(2f, base.Height / 14f);
            }
            else if (normal.Y > 0f)
            {
                position = base.TopCenter;
                positionRange = Vector2.UnitX * (base.Width - 6f);
                direction = -(float)Math.PI / 2f;
                num = Math.Max(2f, base.Width / 14f);
            }
            else
            {
                position = base.BottomCenter;
                positionRange = Vector2.UnitX * (base.Width - 6f);
                direction = (float)Math.PI / 2f;
                num = Math.Max(2f, base.Width / 14f);
            }
            particlesRemainder += num;
            int num2 = (int)particlesRemainder;
            particlesRemainder -= num2;
            positionRange *= 0.5f;
            SceneAs<Level>().Particles.Emit(P_Move, num2, position, positionRange, direction);
        }

        private bool PlayerFallCheck()
        {
            if (climbFall)
            {
                return HasPlayerRider();
            }
            return HasPlayerOnTop();
        }

        private bool PlayerWaitCheck()
        {
            if (Triggered)
            {
                return true;
            }
            if (PlayerFallCheck())
            {
                return true;
            }
            if (climbFall)
            {
                return CollideCheck<Player>(Position - Vector2.UnitX) || CollideCheck<Player>(Position + Vector2.UnitX);
            }
            return false;
        }

        private void ScrapeParticlesCheck(Vector2 to)
        {
            if (!base.Scene.OnInterval(0.03f))
            {
                return;
            }
            bool flag = to.Y != base.ExactPosition.Y;
            bool flag2 = to.X != base.ExactPosition.X;
            if (flag && !flag2)
            {
                int num = Math.Sign(to.Y - base.ExactPosition.Y);
                Vector2 value = (num != 1) ? base.TopLeft : base.BottomLeft;
                int num2 = 4;
                if (num == 1)
                {
                    num2 = Math.Min((int)base.Height - 12, 20);
                }
                int num3 = (int)base.Height;
                if (num == -1)
                {
                    num3 = Math.Max(16, (int)base.Height - 16);
                }
                if (base.Scene.CollideCheck<Solid>(value + new Vector2(-2f, num * -2)))
                {
                    for (int i = num2; i < num3; i += 8)
                    {
                        SceneAs<Level>().ParticlesFG.Emit(P_Scrape, base.TopLeft + new Vector2(0f, (float)i + (float)num * 2f), (num == 1) ? (-(float)Math.PI / 4f) : ((float)Math.PI / 4f));
                    }
                }
                if (base.Scene.CollideCheck<Solid>(value + new Vector2(base.Width + 2f, num * -2)))
                {
                    for (int j = num2; j < num3; j += 8)
                    {
                        SceneAs<Level>().ParticlesFG.Emit(P_Scrape, base.TopRight + new Vector2(-1f, (float)j + (float)num * 2f), (num == 1) ? ((float)Math.PI * -3f / 4f) : ((float)Math.PI * 3f / 4f));
                    }
                }
            }
            else
            {
                if (!flag2 || flag)
                {
                    return;
                }
                int num4 = Math.Sign(to.X - base.ExactPosition.X);
                Vector2 value2 = (num4 != 1) ? base.TopLeft : base.TopRight;
                int num5 = 4;
                if (num4 == 1)
                {
                    num5 = Math.Min((int)base.Width - 12, 20);
                }
                int num6 = (int)base.Width;
                if (num4 == -1)
                {
                    num6 = Math.Max(16, (int)base.Width - 16);
                }
                if (base.Scene.CollideCheck<Solid>(value2 + new Vector2(num4 * -2, -2f)))
                {
                    for (int k = num5; k < num6; k += 8)
                    {
                        SceneAs<Level>().ParticlesFG.Emit(P_Scrape, base.TopLeft + new Vector2((float)k + (float)num4 * 2f, -1f), (num4 == 1) ? ((float)Math.PI * 3f / 4f) : ((float)Math.PI / 4f));
                    }
                }
                if (base.Scene.CollideCheck<Solid>(value2 + new Vector2(num4 * -2, base.Height + 2f)))
                {
                    for (int l = num5; l < num6; l += 8)
                    {
                        SceneAs<Level>().ParticlesFG.Emit(P_Scrape, base.BottomLeft + new Vector2((float)l + (float)num4 * 2f, 0f), (num4 == 1) ? ((float)Math.PI * -3f / 4f) : (-(float)Math.PI / 4f));
                    }
                }
            }
        }

        private IEnumerator ZipMoverSequence()
        {
            Vector2 start = Position;
            while (true)
            {
                if (!HasPlayerRider())
                {
                    yield return null;
                    continue;
                }
                sfx.Play((theme == Themes.Normal) ? "event:/game/01_forsaken_city/zip_mover" : "event:/new_content/game/10_farewell/zip_mover");
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
                StartShaking(0.1f);
                yield return 0.1f;
                streetlight.SetAnimationFrame(3);
                StopPlayerRunIntoAnimation = false;
                float at2 = 0f;
                while (at2 < 1f)
                {
                    yield return null;
                    at2 = Calc.Approach(at2, 1f, 2f * Engine.DeltaTime);
                    percent = Ease.SineIn(at2);
                    Vector2 to = Vector2.Lerp(start, target, percent);
                    ScrapeParticlesCheck(to);
                    if (Scene.OnInterval(0.1f))
                    {
                        pathRenderer.CreateSparks();
                    }
                    MoveTo(to);
                }
                StartShaking(0.2f);
                Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
                SceneAs<Level>().Shake();
                StopPlayerRunIntoAnimation = true;
                yield return 0.5f;
                StopPlayerRunIntoAnimation = false;
                streetlight.SetAnimationFrame(2);
                float at = 0f;
                while (at < 1f)
                {
                    yield return null;
                    at = Calc.Approach(at, 1f, 0.5f * Engine.DeltaTime);
                    percent = 1f - Ease.SineIn(at);
                    Vector2 to2 = Vector2.Lerp(target, start, Ease.SineIn(at));
                    MoveTo(to2);
                }
                StopPlayerRunIntoAnimation = true;
                StartShaking(0.2f);
                streetlight.SetAnimationFrame(1);
                yield return 0.5f;
                TrySwitchTroll();
                yield return null;
            }
        }

        private IEnumerator FallBlockSequence()
        {
            while (!Triggered && (!PlayerFallCheck()))
            {
                yield return null;
            }
            while (FallDelay > 0f)
            {
                FallDelay -= Engine.DeltaTime;
                yield return null;
            }
            HasStartedFalling = true;
            while (true)
            {
                Audio.Play("event:/game/general/fallblock_shake", base.Center);
                StartShaking();
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                yield return 0.2f;
                float timer = 0.4f;
                while (timer > 0f && PlayerWaitCheck())
                {
                    yield return null;
                    timer -= Engine.DeltaTime;
                }
                StopShaking();
                for (int i = 2; (float)i < Width; i += 4)
                {
                    if (Scene.CollideCheck<Solid>(TopLeft + new Vector2(i, -2f)))
                    {
                        SceneAs<Level>().Particles.Emit(P_FallDustA, 2, new Vector2(X + (float)i, Y), Vector2.One * 4f, (float)Math.PI / 2f);
                    }
                    SceneAs<Level>().Particles.Emit(P_FallDustB, 2, new Vector2(X + (float)i, Y), Vector2.One * 4f);
                }
                float speed = 0f;
                float maxSpeed = 160f;
                while (true)
                {
                    Level level = SceneAs<Level>();
                    speed = Calc.Approach(speed, maxSpeed, 500f * Engine.DeltaTime);
                    if (MoveVCollideSolids(speed * Engine.DeltaTime, thruDashBlocks: true))
                    {
                        break;
                    }
                    if (Top > (float)(level.Bounds.Bottom + 16) || (Top > (float)(level.Bounds.Bottom - 1) && CollideCheck<Solid>(Position + new Vector2(0f, 1f))))
                    {
                        Collidable = (Visible = false);
                        yield return 0.2f;
                        if (level.Session.MapData.CanTransitionTo(level, new Vector2(Center.X, Bottom + 12f)))
                        {
                            yield return 0.2f;
                            SceneAs<Level>().Shake();
                            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
                        }
                        RemoveSelf();
                        DestroyStaticMovers();
                        yield break;
                    }
                    yield return null;
                }
                Audio.Play("event:/game/general/fallblock_impact", base.BottomCenter);
                Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
                SceneAs<Level>().DirectionalShake(Vector2.UnitY, 0.3f);
                StartShaking();
                LandParticles();
                yield return 0.2f;
                StopShaking();
                if (CollideCheck<SolidTiles>(Position + new Vector2(0f, 1f)))
                {
                    break;
                }
                while (CollideCheck<Platform>(Position + new Vector2(0f, 1f)))
                {
                    yield return 0.1f;
                }
            }
            Safe = true;
        }

        private void LandParticles()
        {
            for (int i = 2; (float)i <= base.Width; i += 4)
            {
                if (base.Scene.CollideCheck<Solid>(base.BottomLeft + new Vector2(i, 3f)))
                {
                    SceneAs<Level>().ParticlesFG.Emit(P_FallDustA, 1, new Vector2(base.X + (float)i, base.Bottom), Vector2.One * 4f, -(float)Math.PI / 2f);
                    float direction = (!((float)i < base.Width / 2f)) ? 0f : ((float)Math.PI);
                    SceneAs<Level>().ParticlesFG.Emit(P_LandDust, 1, new Vector2(base.X + (float)i, base.Bottom), Vector2.One * 4f, direction);
                }
            }
        }

        private float mod(float x, float m)
        {
            return (x % m + m) % m;
        }
    }
}
