using Celeste;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;
using FactoryHelper.Components;
using System.Collections;

namespace FactoryHelper.Entities
{
    class DashFuseBox : Solid
    {
        public enum Direction
        {
            Left,
            Right
        }

        private ParticleType _sparks = new ParticleType
        {
            Size = 1f,
            Color = Calc.HexToColor("d97b00"),
            Color2 = Calc.HexToColor("f7be00"),
            ColorMode = ParticleType.ColorModes.Blink,
            FadeMode = ParticleType.FadeModes.Late,
            SpeedMin = 5f,
            SpeedMax = 30f,
            Acceleration = Vector2.UnitY * 60f,
            DirectionRange = (float)Math.PI / 2f,
            Direction = 0f,
            LifeMin = 0.5f,
            LifeMax = 1.0f
        };

        private bool _persistent;
        private bool _activatedPermanently {
            get
            {
                return (Scene as Level).Session.GetFlag($"DashFuseBox:{_id.Key}");
            }
            set
            {
                (Scene as Level).Session.SetFlag($"DashFuseBox:{_id.Key}", value);
            }
        }
        private bool _activated = false;

        private Sprite _mainSprite;
        private Sprite _doorSprite;
        private Entity _door;
        private Direction _direction;
        private EntityID _id;
        private HashSet<string> _activationIds = new HashSet<string>();
        private Vector2 _pressDirection;

        public DashFuseBox(EntityData data, Vector2 offset) : base(data.Position + offset, 4f, 16f, false)
        {
            string[] activationIds = data.Attr("activationIds", "").Split(',');

            _persistent = data.Bool("persistent", false);
            _id = new EntityID(data.Level.Name, data.ID);
            _direction = Direction.Right;
            Enum.TryParse(data.Attr("direction", "Right"), out _direction);

            Depth = -20;
            Add(_mainSprite = new Sprite(GFX.Game, "objects/FactoryHelper/dashFuseBox/"));
            _mainSprite.Add("idle", "idle", 0.1f, "idleBackwards");
            _mainSprite.Add("chaos", "chaos", 0.08f, "chaos");
            _mainSprite.Add("idleBackwards", "idle", 0.1f, "idle", 10, 9 ,8 ,7, 6, 5, 4, 3, 2, 1, 0);
            _mainSprite.Play("idle", false, false);

            _door = new Entity(Position)
            {
                Depth = 10000 - 10
            };
            _door.Add(_doorSprite = new Sprite(GFX.Game, "objects/FactoryHelper/dashFuseBox/"));
            _doorSprite.Add("busted", "busted", 0.05f);
            _doorSprite.Play("busted", false, false);
            _doorSprite.Active = false;
            _doorSprite.Visible = false;


            if (_direction == Direction.Left)
            {
                _mainSprite.Scale *= new Vector2(-1f, 1f);
                //_mainSprite.Position.X += 4;
                _doorSprite.Scale *= new Vector2(-1f, 1f);
                _doorSprite.Position.X += 4;

                Collider.Position.X -= 4;
                _pressDirection = Vector2.UnitX;
                _sparks.Direction = (float)Math.PI;
            }
            else
            {
                _pressDirection = -Vector2.UnitX;
            }

            foreach (string activationId in activationIds)
            {
                if (activationId != "")
                {
                    _activationIds.Add(activationId);
                }
            }
            OnDashCollide = OnDashed;
        }

        public DashFuseBox(Vector2 position, float width, float height, bool safe) : base(position, width, height, safe)
        {
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Add(_door);
            if (_activatedPermanently || AllCircuitsActive())
            {
                StartBusted();
                _activated = true;
            }
        }

        private void SendOutSignals()
        {
            foreach (FactoryActivatorComponent activator in Scene.Tracker.GetComponents<FactoryActivatorComponent>())
            {
                if (_activationIds.Contains(activator.ActivationId))
                {
                    activator.Activate();
                }
            }
        }

        private bool AllCircuitsActive()
        {
            Session session = (Scene as Level).Session;
            foreach (string activationId in _activationIds)
            {
                if (session.GetFlag($"FactoryActivation:{activationId}") == false)
                {
                    return false;
                }
            }
            return true;
        }

        public override void Removed(Scene scene)
        {
            scene.Remove(_door);
            base.Removed(scene);
        }

        public DashCollisionResults OnDashed(Player player, Vector2 direction)
        {
            Console.WriteLine("Dashed into fuse box");
            if (!_activated && (direction == _pressDirection))
            {
                _activated = true;

                _doorSprite.Active = true;
                _doorSprite.Visible = true;
                _mainSprite.Play("chaos", true, true);
                SetBustedCollider();

                Audio.Play("event:/new_content/game/10_farewell/fusebox_hit_2", Position);
                SetSessionTags();
                SendOutSignals();

                player?.RefillDash();

                Sparkle(20);

                Add(new Coroutine(SparkleSequence()));

                return DashCollisionResults.Rebound;
            }
            return DashCollisionResults.NormalCollision;
        }

        private IEnumerator SparkleSequence()
        {
            while (true)
            {
                yield return Calc.Random.NextFloat(4f);
                for (int i = 0; i < 6; i++)
                {
                    yield return Calc.Random.NextFloat(0.03f);
                    Sparkle(1);
                }
            }
        }

        private void Sparkle(int count)
        {
            if (Scene == null)
            {
                return;
            }
            SceneAs<Level>().ParticlesFG.Emit(_sparks, count, Position + Collider.CenterRight, new Vector2(0, Collider.Height / 4));
        }

        private void SetSessionTags()
        {
            if (_persistent)
            {
                foreach (string activationId in _activationIds)
                {
                    ActivatePermanently(activationId);
                }
                _activatedPermanently = true;
            }
        }

        public override void Update()
        {
            base.Update();
            if (_activated)
            {
                DisplacePlayerOnTop();
            }
        }

        public override void Render()
        {
            if (!_activated)
            {
                _mainSprite.DrawSimpleOutline();
            }
            base.Render();
        }

        private void DisplacePlayerOnTop()
        {
            if (HasPlayerOnTop())
            {
                Player player = GetPlayerOnTop();
                if (player != null)
                {
                    if (_direction == Direction.Right)
                    {
                        if (player.Left >= Left)
                        {
                            player.Left = Right;
                            player.Y += 1f;
                        }
                    }
                    else
                    {
                        if (player.Right <= Right)
                        {
                            player.Right = Left;
                            player.Y += 1f;
                        }
                    }
                }
            }
        }

        private void StartBusted()
        {
            _mainSprite.Play("chaos",true, true);
            _doorSprite.SetAnimationFrame(4);
            _doorSprite.Visible = true;
            SetBustedCollider();
        }

        private void SetBustedCollider()
        {
            Collider.Width = 2;
            if (_direction == Direction.Left)
            {
                Position += 2 * Vector2.UnitX;
                _mainSprite.Position -= 2 * Vector2.UnitX;
                _doorSprite.Position -= 4 * Vector2.UnitX;
            }
        }

        private void ActivatePermanently(string activationId)
        {
            (Scene as Level).Session.SetFlag($"FactoryActivation:{activationId}", true);
        }

        private void DeactivatePermanently(string activationId)
        {
            (Scene as Level).Session.SetFlag($"FactoryActivation:{activationId}", false);
        }
    }
}
