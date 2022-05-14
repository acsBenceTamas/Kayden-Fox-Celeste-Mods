using Celeste;
using Celeste.Mod.Entities;
using FactoryHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace FactoryHelper.Entities
{
    [CustomEntity(
        "FactoryHelper/FanHorizontal = LoadHorizontal",
        "FactoryHelper/FanVertical = LoadVertical"
        )]
    public class Fan : Solid
    {
        public static Entity LoadHorizontal(Level level, LevelData levelData, Vector2 offset, EntityData data) => new Fan(data, offset, Directions.Horizontal);
        public static Entity LoadVertical(Level level, LevelData levelData, Vector2 offset, EntityData data) => new Fan(data, offset, Directions.Vertical);

        public enum Directions
        {
            Horizontal,
            Vertical
        }

        public FactoryActivator Activator;

        private Sprite _fanSprite;
        private float _percent;
        private bool _speedingUp;

        public Fan(EntityData data, Vector2 offset, Directions direction) : this (data.Position + offset, data.Width, data.Height, direction, data.Attr("activationId"), data.Bool("startActive", false))
        {
        }

        public Fan(Vector2 position, float width, float height, Directions direction, string activationId, bool startActive) : base(position, width, height, false)
        {
            Add(Activator = new FactoryActivator());
            Activator.ActivationId = activationId != string.Empty ? activationId : null;
            Activator.StartOn = startActive;
            Activator.OnTurnOn = () =>
            {
                _speedingUp = true;
            };
            Activator.OnTurnOff = () =>
            {
                _speedingUp = false;
            };
            Activator.OnStartOff = () =>
            {
                _percent = 0f;
                _speedingUp = false;
            };
            Activator.OnStartOn = () =>
            {
                _percent = 1f;
                _speedingUp = true;
            };

            Add(new SteamCollider(OnSteamWall));

            Add(_fanSprite = FactoryHelperModule.SpriteBank.Create("fan"));
            _fanSprite.CenterOrigin();
            _fanSprite.Position = new Vector2(width / 2, height / 2);
            switch (direction)
            {
                default:
                case Directions.Horizontal:
                    _fanSprite.Scale.X = width / 24f;
                    break;
                case Directions.Vertical:
                    _fanSprite.Scale.X = height / 24f;
                    _fanSprite.Rotation = (float)Math.PI/2;
                    break;
            }
            _fanSprite.Play("rotating", true);

            MTexture mTexture = GFX.Game["objects/FactoryHelper/fan/body0"];
            int size = (int)Math.Max(width, height);
            for (int i = 0; i < size; i+=8)
            {
                int x;
                Image image;
                if (i == 0)
                {
                    x = 0;
                }
                else if (i == size - 8)
                {
                    x = 2;
                }
                else
                {
                    x = 1;
                }
                Add(image = new Image(mTexture.GetSubtexture(x * 8, 0, 8, 16)));
                switch (direction)
                {
                    default:
                    case Directions.Horizontal:
                        image.X = i;
                        break;
                    case Directions.Vertical:
                        image.Rotation = (float)Math.PI / 2;
                        image.Y = i;
                        image.X += 16;
                        break;
                }
            }
            SurfaceSoundIndex = 7;
            Add(new LightOcclude(0.5f));
        }

        private void OnSteamWall(SteamWall obj)
        {
            Activator.ForceDeactivate();
        }

        public override void Update()
        {
            base.Update();
            if (_speedingUp && (_percent < 1f))
            {
                _percent = Calc.Approach(_percent, 1f, Engine.DeltaTime / 1f);
            }
            else if (!_speedingUp && (_percent > 0f))
            {
                _percent = Calc.Approach(_percent, 0f, Engine.DeltaTime / 1.5f);
            }
            _fanSprite.Rate = _percent;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Activator.HandleStartup(scene);
        }
    }
}
