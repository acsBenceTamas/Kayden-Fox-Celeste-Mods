using Celeste;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        private bool Activated {
            get
            {
                return (Scene as Level).Session.GetFlag($"DashFuseBox:{_id.Key}");
            }
            set
            {
                (Scene as Level).Session.SetFlag($"DashFuseBox:{_id.Key}", value);
            }
        }

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
                    object persistenceString = _persistent ? "Persistent" : string.Empty;
                    _activationIds.Add($"{persistenceString}FactoryActivation:{activationId}");
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
            if (Activated)
            {
                StartBusted();
            }
        }

        public override void Removed(Scene scene)
        {
            HandleRest();
            scene.Remove(_door);
            base.Removed(scene);
        }

        public DashCollisionResults OnDashed(Player player, Vector2 direction)
        {
            Console.WriteLine($"Dash direction: {direction} | Press direction: {_pressDirection}");
            if (!Activated && direction == _pressDirection)
            {
                Activated = true;

                _doorSprite.Active = true;
                _doorSprite.Visible = true;
                _mainSprite.SetAnimationFrame(1);
                SetBustedCollider();

                Audio.Play("event:/new_content/game/10_farewell/fusebox_hit_2", Position);

                foreach (string activationId in _activationIds)
                {
                    Activate(activationId);
                }

                player.RefillDash();

                return DashCollisionResults.Rebound;
            }
            return DashCollisionResults.NormalCollision;
        }

        public override void SceneEnd(Scene scene)
        {
            HandleRest();
            base.SceneEnd(scene);
        }

        public override void Update()
        {
            base.Update();
            if (Activated)
            {
                DisplacePlayerOnElement();
            }
        }

        private void HandleRest()
        {
            Console.WriteLine("Handling Reset");
            if (!_persistent)
            {
                Activated = false;
                foreach (string activationId in _activationIds)
                {
                    Deactivate(activationId);
                }
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
                            player.Y += 0.5f;
                        }
                    }
                    else
                    {
                        if (player.Right <= Right)
                        {
                            player.Right = Left;
                            player.Y += 0.5f;
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

        private void Activate(string activationId)
        {
            (Scene as Level).Session.SetFlag(activationId, true);
            var components = Scene.Tracker.GetComponents<FactoryActivationComponent>()
                .Where(component => (component as FactoryActivationComponent).ActivationId == activationId);
            foreach (var component in components)
            {
                (component as FactoryActivationComponent).SetActivationState(true);
            }
        }

        private void Deactivate(string activationId, bool sceneEnding = true)
        {
            (Scene as Level).Session.SetFlag(activationId, false);
            if (!sceneEnding)
            {
                var components = Scene.Tracker.GetComponents<FactoryActivationComponent>()
                    .Where(component => (component as FactoryActivationComponent).ActivationId == activationId);
                foreach (var component in components)
                {
                    (component as FactoryActivationComponent).SetActivationState(false);
                }
            }
        }
    }
}
