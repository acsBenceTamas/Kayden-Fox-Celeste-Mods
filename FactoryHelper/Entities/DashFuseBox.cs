using Celeste;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;
using FactoryHelper.Components;

namespace FactoryHelper.Entities
{
    class DashFuseBox : Solid
    {
        public enum Direction
        {
            Left,
            Right
        }

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
            Add(_mainSprite = new Sprite(GFX.Game, "objects/FactoryHelper/dashFuseBox/idle"));
            _mainSprite.Add("body", "");
            _mainSprite.Play("body", false, false);
            _mainSprite.Active = false;

            _door = new Entity(Position);
            _door.Depth = 20;
            _door.Add(_doorSprite = new Sprite(GFX.Game, "objects/FactoryHelper/dashFuseBox/break"));
            _doorSprite.Add("break", "", 0.05f);
            _doorSprite.Play("break", false, false);
            _doorSprite.Active = false;
            _doorSprite.Visible = true;


            if (_direction == Direction.Left)
            {
                _mainSprite.Scale *= new Vector2(-1f, 1f);
                _mainSprite.Position.X += 4;
                _doorSprite.Scale *= new Vector2(-1f, 1f);
                _doorSprite.Position.X += 4;

                Position.X -= 4;
                _pressDirection = Vector2.UnitX;
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
            if (!_activated && (direction == _pressDirection))
            {
                _activated = true;

                _doorSprite.Active = true;
                _doorSprite.Visible = true;
                _mainSprite.SetAnimationFrame(1);
                SetBustedCollider();

                Audio.Play("event:/new_content/game/10_farewell/fusebox_hit_2", Position);
                SetSessionTags();
                SendOutSignals();

                player.RefillDash();

                return DashCollisionResults.Rebound;
            }
            return DashCollisionResults.NormalCollision;
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
            if (_activatedPermanently)
            {
                DisplacePlayerOnElement();
            }
        }

        private void DisplacePlayerOnElement()
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
            _mainSprite.SetAnimationFrame(1);
            _doorSprite.SetAnimationFrame(6);
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
