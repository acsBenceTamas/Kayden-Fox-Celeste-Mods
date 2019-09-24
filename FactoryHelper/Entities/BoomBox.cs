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
    class BoomBox : Solid
    {
        public bool Activated
        {
            get
            {
                if (_activationId == null)
                {
                    return true;
                }
                else
                {
                    Level level = Scene as Level;
                    return level.Session.GetFlag(_activationId) || level.Session.GetFlag("Persistent" + _activationId);
                }
            }
        }

        private string _activationId;
        private float _initialDelay;
        private bool _startActive;
        private bool _activatedEarlier;
        private Collider _boomArea = new Circle(48f, 12f, 12f);
        private Sprite _sprite;

        public BoomBox(EntityData data, Vector2 offest) : this(data.Position + offest, data.Attr("activationId", ""), data.Float("initialDelay", 0f), data.Bool("startActive", false))
        {

        }
        public BoomBox(Vector2 position, string activationId, float initialDelay, bool startActive) : base(position, 16, 16, true)
        {
            _activationId = activationId == string.Empty ? null : $"FactoryActivation:{activationId}";
            _startActive = startActive;
            _initialDelay = initialDelay;
            Add(_sprite = new Sprite(GFX.Game, "objects/FactoryHelper/boomBox/"));
            _sprite.Add("idle", "idle", 0.25f);
            _sprite.Add("activating", "activating", 0.25f);
            _sprite.Add("active", "active", 0.25f);
            _sprite.Add("active", "angry", 0.1f);
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            _activatedEarlier = Activated;
        }

        public override void Update()
        {
            base.Update();
            if (!_activatedEarlier == Activated)
            {
            }
            if (HasPlayerRider())
            {
                Player player = GetPlayerRider();
                if (player != null)
                {
                    player.ExplodeLaunch(Center, false, false);
                }
            }
        }
    }
}
