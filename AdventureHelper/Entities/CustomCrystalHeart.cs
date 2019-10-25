using Celeste.Mod.Entities;
using Celeste.Mod.Meta;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.AdventureHelper.Entities
{
    [CustomEntity("AdventureHelper/CustomCrystalHeart")]
    [Tracked(false)]
    public class CustomCrystalHeart : Entity
    {
        public ParticleType P_Shine = new ParticleType(HeartGem.P_BlueShine);

        public bool IsGhost;

        public const float GhostAlpha = 0.8f;

        private Sprite sprite;

        private Sprite white;

        private ParticleType shineParticle;

        public Wiggler ScaleWiggler;

        private Wiggler moveWiggler;

        private Vector2 moveWiggleDir;

        private BloomPoint bloom;

        private VertexLight light;

        private Poem poem;

        private float timer;

        private bool collected;

        private bool autoPulse = true;

        private float bounceSfxDelay;

        private bool removeCameraTriggers;

        private SoundEmitter sfx;

        private List<InvisibleBarrier> walls = new List<InvisibleBarrier>();

        private HoldableCollider holdableCollider;

        private EntityID entityID;

        private Color color;

        private string path;

        public CustomCrystalHeart(Vector2 position)
            : base(position)
        {
            Add(holdableCollider = new HoldableCollider(OnHoldable));
            Add(new MirrorReflection());
        }

        public CustomCrystalHeart(EntityData data, Vector2 offset)
            : this(data.Position + offset)
        {
            removeCameraTriggers = data.Bool("removeCameraTriggers");
            entityID = new EntityID(data.Level.Name, data.ID);
            color = Calc.HexToColor(data.Attr("color", "00a81f"));
            path = data.Attr("path", "");
            P_Shine.Color = color;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Level level = base.Scene as Level;

            if (level.Session.HeartGem)
            {
                RemoveSelf();
                return;
            }

            AreaKey area = level.Session.Area;
            IsGhost = (SaveData.Instance.Areas_Safe[area.ID].Modes[(int)area.Mode].HeartGem);

            if (path == string.Empty)
            {
                Add(sprite = GFX.SpriteBank.Create("heartgem3"));
            }
            else
            {
                Add(sprite = GFX.SpriteBank.Create(path));
            }

            sprite.Play("spin");
            sprite.OnLoop = delegate (string anim)
            {
                if (Visible && anim == "spin" && autoPulse)
                {
                    Audio.Play("event:/game/general/crystalheart_pulse", Position);
                    ScaleWiggler.Start();
                    (base.Scene as Level).Displacement.AddBurst(Position, 0.35f, 8f, 48f, 0.25f);
                }
            };

            if (IsGhost)
            {
                if (path == string.Empty)
                {
                    sprite.Color = Color.Lerp(color, Color.White, 0.8f) * 0.8f;
                } else
                {
                    sprite.Color = Color.White * 0.8f;
                }
            }
            else if (path == string.Empty)
            {
                sprite.Color = color;
            }
            
            switch (path)
            {
                case "heartgem0":
                    color = Color.Aqua;
                    P_Shine = HeartGem.P_BlueShine;
                    break;
                case "heartgem1":
                    color = Color.Red;
                    P_Shine = HeartGem.P_RedShine;
                    break;
                case "heartgem2":
                    color = Color.Gold;
                    P_Shine = HeartGem.P_GoldShine;
                    break;
                case "heartgem3":
                    color = Calc.HexToColor("dad8cc");
                    P_Shine = HeartGem.P_FakeShine;
                    break;
            }

            Collider = new Hitbox(16f, 16f, -8f, -8f);
            Add(new PlayerCollider(OnPlayer));
            Add(ScaleWiggler = Wiggler.Create(0.5f, 4f, delegate (float f)
            {
                sprite.Scale = Vector2.One * (1f + f * 0.25f);
            }));
            Add(bloom = new BloomPoint(0.75f, 16f));
            Color value;
            shineParticle = P_Shine;
            value = Color.Lerp(color, Color.White, 0.5f);
            Add(light = new VertexLight(value, 1f, 32, 64));
            if (path == "heartgem3")
            {
                bloom.Alpha = 0f;
                light.Alpha = 0f;
            }
            moveWiggler = Wiggler.Create(0.8f, 2f);
            moveWiggler.StartZero = true;
            Add(moveWiggler);
        }

        public override void Update()
        {
            bounceSfxDelay -= Engine.DeltaTime;
            timer += Engine.DeltaTime;
            sprite.Position = Vector2.UnitY * (float)Math.Sin(timer * 2f) * 2f + moveWiggleDir * moveWiggler.Value * -8f;
            if (white != null)
            {
                white.Position = sprite.Position;
                white.Scale = sprite.Scale;
                if (white.CurrentAnimationID != sprite.CurrentAnimationID)
                {
                    white.Play(sprite.CurrentAnimationID);
                }
                white.SetAnimationFrame(sprite.CurrentAnimationFrame);
            }
            if (collected && (base.Scene.Tracker.GetEntity<Player>()?.Dead ?? true))
            {
                EndCutscene();
            }
            base.Update();
            if (!collected && base.Scene.OnInterval(0.1f))
            {
                SceneAs<Level>().Particles.Emit(shineParticle, 1, base.Center, Vector2.One * 8f);
            }
        }

        public void OnHoldable(Holdable h)
        {
            Player entity = base.Scene.Tracker.GetEntity<Player>();
            if (!collected && entity != null && h.Dangerous(holdableCollider))
            {
                Collect(entity);
            }
        }

        public void OnPlayer(Player player)
        {
            if (collected || (base.Scene as Level).Frozen)
            {
                return;
            }
            if (player.DashAttacking)
            {
                Collect(player);
                return;
            }
            if (bounceSfxDelay <= 0f)
            {
                Audio.Play("event:/game/general/crystalheart_bounce", Position);
                bounceSfxDelay = 0.1f;
            }
            player.PointBounce(base.Center);
            moveWiggler.Start();
            ScaleWiggler.Start();
            moveWiggleDir = (base.Center - player.Center).SafeNormalize(Vector2.UnitY);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
        }

        private void Collect(Player player)
        {
            base.Scene.Tracker.GetEntity<AngryOshiro>()?.StopControllingTime();
            Coroutine coroutine = new Coroutine(CollectRoutine(player));
            coroutine.UseRawDeltaTime = true;
            Add(coroutine);
            collected = true;
            if (removeCameraTriggers)
            {
                List<CameraOffsetTrigger> list = base.Scene.Entities.FindAll<CameraOffsetTrigger>();
                foreach (CameraOffsetTrigger item in list)
                {
                    item.RemoveSelf();
                }
            }
        }

        private IEnumerator CollectRoutine(Player player)
        {
            Level level = Scene as Level;
            AreaKey area = level.Session.Area;
            string poemID = AreaData.Get(level).Mode[(int)area.Mode].PoemID;
            bool completeArea = IsCompleteArea(false);
            level.CanRetry = false;
            if (completeArea)
            {
                Audio.SetMusic(null);
                Audio.SetAmbience(null);
            }
            if (completeArea)
            {
                List<Strawberry> strawbs = new List<Strawberry>();
                foreach (Follower follower in player.Leader.Followers)
                {
                    if (follower.Entity is Strawberry)
                    {
                        strawbs.Add(follower.Entity as Strawberry);
                    }
                }
                foreach (Strawberry strawb in strawbs)
                {
                    strawb.OnCollect();
                }
            }
            string sfxEvent = "event:/game/general/crystalheart_blue_get";
            if (area.Mode == AreaMode.BSide)
            {
                sfxEvent = "event:/game/general/crystalheart_red_get";
            }
            else if (area.Mode == AreaMode.CSide)
            {
                sfxEvent = "event:/game/general/crystalheart_gold_get";
            }
            sfx = SoundEmitter.Play(sfxEvent, this);
            Add(new LevelEndingHook(delegate
            {
                sfx.Source.Stop();
            }));
            walls.Add(new InvisibleBarrier(new Vector2(level.Bounds.Right, level.Bounds.Top), 8f, level.Bounds.Height));
            walls.Add(new InvisibleBarrier(new Vector2(level.Bounds.Left - 8, level.Bounds.Top), 8f, level.Bounds.Height));
            walls.Add(new InvisibleBarrier(new Vector2(level.Bounds.Left, level.Bounds.Top - 8), level.Bounds.Width, 8f));
            foreach (InvisibleBarrier wall in walls)
            {
                Scene.Add(wall);
            }
            Add(white = GFX.SpriteBank.Create("heartGemWhite"));
            Depth = -2000000;
            yield return null;
            Celeste.Freeze(0.2f);
            yield return null;
            Engine.TimeRate = 0.5f;
            player.Depth = -2000000;
            for (int i = 0; i < 10; i++)
            {
                Scene.Add(new AbsorbOrb(Position));
            }
            level.Shake();
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            level.Flash(Color.White);
            level.FormationBackdrop.Display = true;
            level.FormationBackdrop.Alpha = 1f;
            light.Alpha = (bloom.Alpha = 0f);
            Visible = false;
            for (float t3 = 0f; t3 < 2f; t3 += Engine.RawDeltaTime)
            {
                Engine.TimeRate = Calc.Approach(Engine.TimeRate, 0f, Engine.RawDeltaTime * 0.25f);
                yield return null;
            }
            yield return null;
            if (player.Dead)
            {
                yield return 100f;
            }
            Engine.TimeRate = 1f;
            Tag = Tags.FrozenUpdate;
            level.Frozen = true;
            RegisterAsCollected(level, poemID);
            if (completeArea)
            {
                level.TimerStopped = true;
                level.RegisterAreaComplete();
            }
            string poemText = null;
            if (!string.IsNullOrEmpty(poemID))
            {
                poemText = Dialog.Clean("poem_" + poemID);
            }
            poem = new Poem(poemText, (int)area.Mode, (area.Mode == AreaMode.CSide) ? 1f : 0.6f);
            poem.Alpha = 0f;
            Scene.Add(poem);
            for (float t2 = 0f; t2 < 1f; t2 += Engine.RawDeltaTime)
            {
                poem.Alpha = Ease.CubeOut(t2);
                yield return null;
            }
            while (!Input.MenuConfirm.Pressed && !Input.MenuCancel.Pressed)
            {
                yield return null;
            }
            sfx.Source.Param("end", 1f);
            if (!completeArea)
            {
                level.FormationBackdrop.Display = false;
                for (float t = 0f; t < 1f; t += Engine.RawDeltaTime * 2f)
                {
                    poem.Alpha = Ease.CubeIn(1f - t);
                    yield return null;
                }
                player.Depth = 0;
                EndCutscene();
            }
            else
            {
                yield return new FadeWipe(level, wipeIn: false)
                {
                    Duration = 3.25f
                }.Duration;
                level.CompleteArea(spotlightWipe: false, skipScreenWipe: true, skipCompleteScreen: false);
            }
        }

        private void EndCutscene()
        {
            Level level = base.Scene as Level;
            level.Frozen = false;
            level.CanRetry = true;
            level.FormationBackdrop.Display = false;
            Engine.TimeRate = 1f;
            if (poem != null)
            {
                poem.RemoveSelf();
            }
            foreach (InvisibleBarrier wall in walls)
            {
                wall.RemoveSelf();
            }
            RemoveSelf();
        }

        private void RegisterAsCollected(Level level, string poemID)
        {
            level.Session.HeartGem = true;
            level.Session.UpdateLevelStartDashes();
            int unlockedModes = SaveData.Instance.UnlockedModes;
            SaveData.Instance.RegisterHeartGem(level.Session.Area);
            if (!string.IsNullOrEmpty(poemID))
            {
                SaveData.Instance.RegisterPoemEntry(poemID);
            }
            if (unlockedModes < 3 && SaveData.Instance.UnlockedModes >= 3)
            {
                level.Session.UnlockedCSide = true;
            }
            if (SaveData.Instance.TotalHeartGems >= 24)
            {
                Achievements.Register(Achievement.CSIDES);
            }
        }

        private IEnumerator PlayerStepForward()
        {
            yield return 0.1f;
            Player player = Scene.Tracker.GetEntity<Player>();
            if (player?.CollideCheck<Solid>(player.Position + new Vector2(12f, 1f)) ?? false)
            {
                yield return player.DummyWalkToExact((int)player.X + 10);
            }
            yield return 0.2f;
        }

        private bool IsCompleteArea(bool value)
        {
            MapMetaModeProperties mapMetaModeProperties = (base.Scene as Level)?.Session.MapData.GetMeta();
            if (mapMetaModeProperties != null && mapMetaModeProperties.HeartIsEnd.HasValue)
            {
                return mapMetaModeProperties.HeartIsEnd.Value;
            }
            return value;
        }
    }

}
