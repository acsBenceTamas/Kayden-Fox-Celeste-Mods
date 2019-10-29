using Monocle;
using System;
using Microsoft.Xna.Framework;
using Celeste;
using FactoryHelper.Components;
using Celeste.Mod.Entities;

namespace FactoryHelper.Entities
{
    [CustomEntity("FactoryHelper/DashNegator")]
    class DashNegator : Entity
    {
        public FactoryActivator Activator;

        private Sprite[] _turretSprites;
        private Solid[] _turretSolids;
        private float _particleSpanPeriod;

        public static readonly ParticleType P_NegatorField = new ParticleType
        {
            Size = 1f,
            Color = Calc.HexToColor("800000") * 0.8f,
            Color2 = Calc.HexToColor("c40000") * 0.8f,
            ColorMode = ParticleType.ColorModes.Static,
            FadeMode = ParticleType.FadeModes.InAndOut,
            SpeedMin = 4f,
            SpeedMax = 8f,
            SpinMin = 0.002f,
            SpinMax = 0.005f,
            Acceleration = Vector2.Zero,
            DirectionRange = (float)Math.PI * 2f,
            Direction = 0f,
            LifeMin = 1.5f,
            LifeMax = 2.5f
        };
        private PlayerCollider _pc;

        public DashNegator(EntityData data, Vector2 offset) 
            : this(data.Position + offset, data.Width, data.Height, data.Attr("activationId"), data.Bool("startActive"))
        {
        }

        public DashNegator(Vector2 position, int width, int height, string activationId, bool startActive) : base(position)
        {
            Add(Activator = new FactoryActivator());
            Activator.ActivationId = activationId == string.Empty ? null : activationId;
            Activator.StartOn = startActive;
            Activator.OnStartOff = OnStartOff;
            Activator.OnStartOn = OnStartOn;
            Activator.OnTurnOff = OnTurnOff;
            Activator.OnTurnOn = OnTurnOn;

            Add(new SteamCollider(OnSteamWall));

            Collider = new Hitbox(width - 4, height, 2, 0);

            width = 16 * (width / 16);

            Depth = -8999;

            Add(new StaticMover
            {
                OnShake = OnShake,
                SolidChecker = IsRiding,
                OnDestroy = RemoveSelf
            });
            Add(_pc = new PlayerCollider(OnPlayer));

            int length = width / 16;
            _turretSprites = new Sprite[length];
            _turretSolids = new Solid[length];

            for (int i = 0; i < length; i++)
            {
                Add(_turretSprites[i] = new Sprite(GFX.Game, "danger/FactoryHelper/dashNegator/"));
                _turretSprites[i].Add("inactive", "turret", 1f, 0);
                _turretSprites[i].Add("rest", "turret", 0.2f, "active", 0);
                _turretSprites[i].Add("active", "turret", 0.05f, "rest");
                _turretSprites[i].Position = new Vector2(-2 + 16 * i, -2);

                _turretSolids[i] = new Solid(position + new Vector2(2 + 16 * i, 0), 12, 8, false);
            }

            _particleSpanPeriod =  256f / (width * height);
        }

        private void OnSteamWall(SteamWall obj)
        {
            Activator.ForceDeactivate();
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            foreach (var solid in _turretSolids)
            {
                scene.Add(solid);
            }
            Activator.HandleStartup(scene);
        }

        public override void Removed(Scene scene)
        {
            foreach (var solid in _turretSolids)
            {
                scene.Remove(solid);
            }
            base.Removed(scene);
        }

        public override void Update()
        {
            base.Update();

            if (Visible && Activator.IsOn && Scene.OnInterval(_particleSpanPeriod))
            {
                SceneAs<Level>().ParticlesFG.Emit(P_NegatorField, 1, Position + Collider.Center, new Vector2(Width/2, Height/2));
            }
        }

        public override void Render()
        {
            Color color = Color.DarkRed * 0.3f;
            if (Visible && Activator.IsOn)
            {
                Draw.Rect(Collider, color);

                Player player = Scene.Tracker.GetEntity<Player>();
                if (player != null && !(player.Top - 4f > Bottom || player.Bottom < Top))
                {
                    float left = Math.Min(Math.Max(Left, player.Left - 2f), Right);
                    float right = Math.Max(Math.Min(Right, player.Right + 2f), left);
                    Draw.Rect(left, Top, right - left, Height, Color.Red * 0.3f);
                }
            }
            base.Render();
        }

        private void OnTurnOn()
        {
            foreach(var sprite in _turretSprites)
            {
                sprite.Play("active", true);
            }
            Fizzle();
        }

        private void OnTurnOff()
        {
            foreach (var sprite in _turretSprites)
            {
                sprite.Play("inactive");
            }
            Fizzle();
        }

        private void OnStartOn()
        {
            foreach (var sprite in _turretSprites)
            {
                sprite.Play("active", true);
            }
        }

        private void OnStartOff()
        {
            foreach (var sprite in _turretSprites)
            {
                sprite.Play("inactive");
            }
        }

        private void Fizzle()
        {
            for (int i = 0; i < Width; i += 16)
            {
                for (int j = 0; j < Height; j += 16)
                {
                    SceneAs<Level>().ParticlesFG.Emit(P_NegatorField, 1, Position + new Vector2(4 + i, 4 + j), new Vector2(4, 4));
                }
            }
        }

        private void OnPlayer(Player player)
        {
            if (Activator.IsOn && player.StartedDashing)
            {
                ShootClosestLaserToPlayer(player);
                player.Die(Vector2.UnitY);
            }
        }

        private void ShootClosestLaserToPlayer(Player player)
        {
            Audio.Play("event:/char/badeline/boss_laser_fire", player.Position);
            Vector2 beamPosition = new Vector2(Position.X, Position.Y + 8);
            beamPosition.X += Math.Min((int)(player.Center.X - Left) / 16 * 16, Width - 12) + 8;
            Scene.Add(new DashNegatorBeam(beamPosition));
        }

        private void OnShake(Vector2 pos)
        {
            foreach (Component component in Components)
            {
                if (component is Image)
                {
                    (component as Image).Position = pos;
                }
            }
        }

        private bool IsRiding(Solid solid)
        {
            bool riding = false;
            foreach (Solid turret in _turretSolids)
            {
                if (turret.CollideCheck(solid, turret.Position - Vector2.UnitY))
                {
                    riding = true;
                    break;
                }
            }
            return riding;
        }
    }
}
