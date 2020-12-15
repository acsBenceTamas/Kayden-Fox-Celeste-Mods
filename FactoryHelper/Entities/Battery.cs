using System;
using System.Collections;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;

namespace FactoryHelper.Entities
{
    [CustomEntity("FactoryHelper/Battery")]
    public class Battery : Entity
    {
        public static ParticleType P_Shimmer = new ParticleType(Key.P_Shimmer)
        {
            Color = Calc.HexToColor("61c200"),
            Color2 = Calc.HexToColor("a1db00")
        };

        public static ParticleType P_Insert = new ParticleType(Key.P_Insert)
        {
            Color = Calc.HexToColor("61c200"),
            Color2 = Calc.HexToColor("a1db00")
        };

        public static ParticleType P_Collect = new ParticleType(P_Shimmer);

        public EntityID ID;

        public bool IsUsed;

        public bool StartedUsing;

        public bool Turning;

        private readonly Follower follower;

        private readonly Sprite sprite;

        private readonly Wiggler wiggler;

        private readonly VertexLight light;

        private ParticleEmitter shimmerParticles;

        private float wobble;

        private bool wobbleActive;

        private Tween tween;

        private Alarm alarm;


        public Battery(Vector2 position, EntityID id) : base(position)
        {
            ID = id;
            Collider = new Hitbox(12f, 12f, -6f, -6f);
            Add(follower = new Follower(id));
            Add(new PlayerCollider(OnPlayer));
            Add(new MirrorReflection());
            Add(sprite = FactoryHelperModule.SpriteBank.Create("battery"));
            sprite.CenterOrigin();
            sprite.Play("rotating");
            Add(new TransitionListener
            {
                OnOut = delegate
                {
                    StartedUsing = false;
                    if (!IsUsed)
                    {
                        if (tween != null)
                        {
                            tween.RemoveSelf();
                            tween = null;
                        }
                        if (alarm != null)
                        {
                            alarm.RemoveSelf();
                            alarm = null;
                        }
                        Turning = false;
                        Visible = true;
                        sprite.Visible = true;
                        sprite.Rate = 1f;
                        sprite.Scale = Vector2.One;
                        sprite.Play("rotating");
                        sprite.Rotation = 0f;
                        wiggler.Stop();
                        follower.MoveTowardsLeader = true;
                    }
                }
            });
            Add(wiggler = Wiggler.Create(0.4f, 4f, delegate (float v)
            {
                sprite.Scale = Vector2.One * (1f + v * 0.35f);
            }));
            Add(light = new VertexLight(Color.LightSeaGreen, 1f, 32, 48));
        }

        public Battery(EntityData data, Vector2 offset) : this(data.Position + offset, new EntityID(data.Level.Name, data.ID))
        {
        }

        public Battery(Player player, EntityID id)
            : this(player.Position + new Vector2(-12 * (int)player.Facing, -8f), id)
        {
            player.Leader.GainFollower(follower);
            Collidable = false;
            Depth = -1000000;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            ParticleSystem particlesFG = (scene as Level).ParticlesFG;
            Add(shimmerParticles = new ParticleEmitter(particlesFG, P_Shimmer, Vector2.Zero, new Vector2(6f, 6f), 1, 0.1f));
            shimmerParticles.SimulateCycle();
        }

        public override void Update()
        {
            if (wobbleActive)
            {
                wobble += Engine.DeltaTime * 4f;
                sprite.Y = (float)Math.Sin(wobble);
            }
            base.Update();
        }

        public override void Render()
        {
            if (sprite.Visible)
            {
                sprite.DrawSimpleOutline();
            }
            base.Render();
        }

        public void RegisterUsed()
        {
            IsUsed = true;
            if (follower.Leader != null)
            {
                follower.Leader.LoseFollower(follower);
            }
            RemoveBattery(ID);
        }

        public IEnumerator UseRoutine(Vector2 target)
        {
            Turning = true;
            follower.MoveTowardsLeader = false;
            wiggler.Start();
            wobbleActive = false;
            sprite.Y = 0f;
            Vector2 from = Position;
            SimpleCurve curve = new SimpleCurve(from, target, (target + from) / 2f + new Vector2(0f, -48f));
            tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, 1f, start: true);
            tween.OnUpdate = delegate (Tween t)
            {
                Position = curve.GetPoint(t.Eased);
                sprite.Rate = 1f + t.Eased * 2f;
            };
            Add(tween);
            yield return tween.Wait();
            tween = null;
            while (sprite.CurrentAnimationFrame != 0)
            {
                yield return null;
            }
            shimmerParticles.Active = false;
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            for (int j = 0; j < 16; j++)
            {
                SceneAs<Level>().ParticlesFG.Emit(P_Insert, Center, (float)Math.PI / 8f * (float)j);
            }
            sprite.Visible = false;
            light.Visible = false;
            Turning = false;
        }

        private void OnPlayer(Player player)
        {
            SceneAs<Level>().Particles.Emit(P_Collect, 10, Position, Vector2.One * 3f);
            Audio.Play("event:/game/general/key_get", Position);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            player.Leader.GainFollower(follower);
            Collidable = false;
            Session session = SceneAs<Level>().Session;
            session.DoNotLoad.Add(ID);
            AddBattery(ID);
            wiggler.Start();
            Depth = -1000000;
        }

        private void AddBattery(EntityID id)
        {
            (FactoryHelperModule.Instance._Session as FactoryHelperSession).Batteries.Add(id);
        }

        private void RemoveBattery(EntityID id)
        {

            (FactoryHelperModule.Instance._Session as FactoryHelperSession).Batteries.Remove(id);
        }
    }
}