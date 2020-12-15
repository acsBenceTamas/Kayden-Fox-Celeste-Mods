using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace FactoryHelper.Entities
{
    [Tracked(false)]
    [CustomEntity("FactoryHelper/MachineHeart")]
    class MachineHeart : Solid
    {
        private Sprite _frontSprite;
        private Sprite _backSprite;
        private int _stage = 0;
        private SoundSource _firstHitSfx;
        private VertexLight _light;
        private BloomPoint _bloom;

        public MachineHeart(EntityData data, Vector2 offset) : this(data.Position + offset)
        {
        }

        public MachineHeart(Vector2 position) : base(position, 24, 32, true)
        {
            Add(_backSprite = FactoryHelperModule.SpriteBank.Create("machineHeart_back"));
            Collider.Position -= new Vector2(12, 16);
            DisableLightsInside = false;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);

            if (!SceneAs<Level>().Session.GetFlag("Machine_Heart_Destroyed"))
            {
                _backSprite.Play("idle");
                Add(_frontSprite = FactoryHelperModule.SpriteBank.Create("machineHeart_front"));
                _frontSprite.Play("idle");
                OnDashCollide = DashCollide;
                Add(_light = new VertexLight(Color.Red, 1f, 32, 64));
                _light.Visible = false;
                Add(_bloom = new BloomPoint(0.8f, 8f));
            }
            else
            {
                _backSprite.Play("destroyed");
                _stage = 3;
                Collidable = false;
                Depth = 8000;
                Add(new Coroutine(SpawnSteam()));
            }
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (!SceneAs<Level>().Session.GetFlag("Machine_Heart_Destroyed"))
            {
                foreach (RustyLamp rustyLamp in Scene.Tracker.GetEntities<RustyLamp>())
                {
                    if (rustyLamp.Activator.ActivationId == null)
                    {
                        rustyLamp.Activator.ForceActivate();
                    }
                }
            }
        }

        private DashCollisionResults DashCollide(Player player, Vector2 direction)
        {
            if (_stage == 0)
            {
                _frontSprite.Play("break");
                Audio.Play("event:/new_content/game/10_farewell/fusebox_hit_2", Position);
                Add(_firstHitSfx = new SoundSource("event:/new_content/game/10_farewell/fusebox_hit_1"));
                Celeste.Celeste.Freeze(0.1f);
                _light.Visible = true;
                foreach (RustyLamp rustyLamp in Scene.Tracker.GetEntities<RustyLamp>())
                {
                    rustyLamp.Activator.ForceDeactivate();
                }
            }
            else if (_stage == 1)
            {
                if (_firstHitSfx != null)
                {
                    _firstHitSfx.Stop();
                }
                _backSprite.Play("break");
                Audio.Play("event:/new_content/game/10_farewell/fusebox_hit_2", Position);
                Add(new Coroutine(CrystalBreakSound()));
                _bloom.Visible = false;
            }
            _stage++;
            return DashCollisionResults.Rebound;
        }

        private IEnumerator CrystalBreakSound()
        {
            _firstHitSfx.Play("event:/game/02_old_site/sequence_mirror");
            yield return 2f;
            _firstHitSfx.Stop();
        }

        public override void Update()
        {
            base.Update();
            if (_frontSprite != null && _stage > 0 && !_frontSprite.Animating)
            {
                Remove(_frontSprite);
                _frontSprite = null;
            }
            if (_stage == 2 && _backSprite.CurrentAnimationID == "break1" &&_backSprite.CurrentAnimationFrame == 3)
            {
                _stage++;
                CrystalDebris.Burst(Position, Color.DarkRed, false, 16);
                Collidable = false;
                _light.Visible = false;
                Depth = 8000;
                Add(new Coroutine(SpawnSteam()));
                Level level = Scene as Level;
                level.Session.SetFlag("Machine_Heart_Destroyed");
                level.Displacement.AddBurst(Position, 1f, 8f, 256f, 0.5f);
                foreach (DashBlock dashBlock in level.Tracker.GetEntities<DashBlock>())
                {
                    dashBlock.Break(dashBlock.CenterLeft, Vector2.UnitX, true);
                }
            }
        }

        private IEnumerator SpawnSteam()
        {
            Level level = Scene as Level;
            level.Session.Audio.Music.Event = "event:/music/factory/escape";
            level.Session.Audio.Apply(forceSixteenthNoteHack: false);
            yield return 2f;
            level.Add(new SteamWall(level.Camera.Left - level.Bounds.Left));
        }
    }
}
