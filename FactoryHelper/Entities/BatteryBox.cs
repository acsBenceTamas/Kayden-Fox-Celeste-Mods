using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryHelper.Entities
{
    class BatteryBox : Entity
    {
        private readonly EntityID _id;
        private readonly Sprite _boxSprite;
        private readonly Sprite _batterySprite;
        private readonly HashSet<string> _activationIds = new HashSet<string>();
        private bool _inserting = false;

        public bool Activated
        {
            get
            {
                return (Scene as Level).Session.GetFlag($"BatteryBox:{_id.Key}");
            }
            set
            {
                (Scene as Level).Session.SetFlag($"BatteryBox:{_id.Key}", value);
            }
        }

        public BatteryBox(EntityData data, Vector2 offset) : this(data.Position + offset, data.Attr("activationIds"))
        {
            _id = new EntityID(data.Level.Name, data.ID);
        }

        public BatteryBox(Vector2 position, string activationIds) : base(position)
        {
            Add(_boxSprite = FactoryHelperModule.SpriteBank.Create("battery_box"));
            Add(_batterySprite = FactoryHelperModule.SpriteBank.Create("battery"));
            _batterySprite.Play("idle");

            foreach (string activationId in activationIds.Split(','))
            {
                if (activationId != "")
                {
                    _activationIds.Add(activationId);
                }
            }

            Add(new PlayerCollider(OnPlayer, new Circle(60f)));

            Depth = 9000;
        }

        public override void Render()
        {
            _boxSprite.DrawSimpleOutline();
            base.Render();
        }

        private void OnPlayer(Player player)
        {
            if (!_inserting && !Activated)
            {
                foreach (Follower follower in player.Leader.Followers)
                {
                    if (follower.Entity is Battery && !(follower.Entity as Battery).StartedUsing)
                    {
                        TryTurnOn(player, follower);
                        break;
                    }
                }
            }
        }

        private void TryTurnOn(Player player, Follower follower)
        {
            if (!Scene.CollideCheck<Solid>(player.Center, Center))
            {
                _inserting = true;
                (follower.Entity as Battery).StartedUsing = true;
                Add(new Coroutine(TurnOnSequence(follower)));
            }
        }

        private IEnumerator TurnOnSequence(Follower follower)
        {
            Battery battery = follower.Entity as Battery;
            Add(new Coroutine(battery.UseRoutine(Center)));
            battery.RegisterUsed();
            while (battery.Turning)
            {
                yield return null;
            }
            Activated = true;
            yield return _boxSprite.PlayRoutine("activating");
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (Activated)
            {
                StartActive();
            }
            else
            {
                StartInactive();
            }
        }

        private void StartInactive()
        {
            _batterySprite.Visible = false;
            _boxSprite.Play("inactive");
        }

        private void StartActive()
        {
            _boxSprite.Play("active");
        }
    }
}
