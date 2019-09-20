using Celeste;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Monocle;

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

        private Sprite _sprite;
        private Direction _direction;
        private EntityID _id;
        private HashSet<string> _activationIds = new HashSet<string>();
        private Vector2 _pressDirection;

        public DashFuseBox(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset, 4f, 16f, false)
        {
            string[] activationIds = data.Attr("activationIds", "").Split(',');

            Depth = 5;
            _persistent = data.Bool("persistent", false);
            _id = id;
            _direction = Direction.Right;
            Enum.TryParse(data.Attr("direction", "Right"), out _direction);
            
            Add(_sprite = new Sprite(GFX.Game, "objects/FactoryHelper/dashFuseBox/break"));
            _sprite.Add("break", "", 0.2f);
            _sprite.Play("break", false, false);
            _sprite.Active = false;

            if (_direction == Direction.Left)
            {
                _sprite.Scale *= new Vector2(-1f, 1f);
                Position.X -= 4;
                _sprite.Position.X += 4;
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

        public DashCollisionResults OnDashed(Player player, Vector2 direction)
        {
            if (!Activated && direction == _pressDirection)
            {
                Activated = true;
                _sprite.Active = true;
                
                foreach (string activationId in _activationIds)
                {
                    Activate(activationId);
                }

                player.RefillDash();
            }
            return DashCollisionResults.NormalCollision;
        }

        private void HandleRest()
        {
            if (!_persistent)
            {
                foreach (string activationId in _activationIds)
                {
                    Deactivate(activationId);
                }
            }
        }

        private void Activate(string activationId)
        {
            (Scene as Level).Session.SetFlag(activationId, true);
        }

        private void Deactivate(string activationId)
        {
            (Scene as Level).Session.SetFlag(activationId, false);
        }

        public override void SceneEnd(Scene scene)
        {
            HandleRest();
            base.SceneEnd(scene);
        }

        public override void Removed(Scene scene)
        {
            HandleRest();
            base.Removed(scene);
        }
    }
}
