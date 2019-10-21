using Celeste;
using Celeste.Mod.Entities;
using FactoryHelper.Components;
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
    [CustomEntity("FactoryHelper/BatteryBox")]
    class BatteryBox : Entity
    {
        private readonly EntityID _id;
        private readonly Sprite _boxSprite;
        private readonly Sprite _batterySprite;
        private readonly HashSet<string> _activationIds = new HashSet<string>();
        private bool _inserting = false;
        private SoundSource _sfx;
        private VertexLight _light;

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

            Add(_sfx = new SoundSource());

            Add(new PlayerCollider(OnPlayer, new Circle(60f)));

            Add(new LightOcclude(new Rectangle(-15,-15,30,30), 0.2f));

            Add(_light = new VertexLight(Color.LightSeaGreen, 1f, 32, 48));
            _light.Visible = false;

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
                        TryTurnOn(player, follower.Entity as Battery);
                        break;
                    }
                }
            }
        }

        private void TryTurnOn(Player player, Battery battery)
        {
            if (!Scene.CollideCheck<Solid>(player.Center, Center))
            {
                _inserting = true;
                battery.StartedUsing = true;
                Add(new Coroutine(TurnOnSequence(battery)));
            }
        }

        private IEnumerator TurnOnSequence(Battery battery)
        {
            Add(new Coroutine(battery.UseRoutine(Center)));
            _sfx.Play("event:/game/03_resort/key_unlock");
            yield return 1.2f;
            while (battery.Turning)
            {
                yield return null;
            }
            _sfx.Stop();
            _sfx.Play("event:/game/03_resort/door_metal_close");
            Activated = true;
            SendOutSignals();
            SetSessionTags();
            _batterySprite.Visible = true;
            _light.Visible = true;
            battery.RegisterUsed();
            yield return _boxSprite.PlayRoutine("activating");
            _boxSprite.Play("active");
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

        private void SetSessionTags()
        {
            foreach (string activationId in _activationIds)
            {
                ActivatePermanently(activationId);
            }
        }

        private void SendOutSignals()
        {
            foreach (FactoryActivator activator in Scene.Tracker.GetComponents<FactoryActivator>())
            {
                if (_activationIds.Contains(activator.ActivationId))
                {
                    activator.Activate();
                }
            }
        }

        private void ActivatePermanently(string activationId)
        {
            (Scene as Level).Session.SetFlag($"FactoryActivation:{activationId}", true);
        }

        private void StartInactive()
        {
            _batterySprite.Visible = false;
            _boxSprite.Play("inactive");
        }

        private void StartActive()
        {
            _boxSprite.Play("active");
            _light.Visible = true;
        }
    }
}
