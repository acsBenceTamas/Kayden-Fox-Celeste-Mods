using Celeste;
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
    class RustyLamp : Entity
    {
        public bool Activated
        {
            get
            {
                return _activator.Active;
            }
            set
            {
                _activator.Active = value;
                if (!_hasDelay && (_startActive != Activated))
                {
                    TurnOn();
                }
                else
                {
                    TurnOff();
                }
            }
        }

        public static readonly Color Color = Color.Lerp(Color.White, Color.Orange, 0.5f);

        private FactoryActivationComponent _activator;
        private string _activationId;
        private float _initialDelay;
        private bool _startActive;
        private Sprite _sprite;
        private VertexLight _light;
        private BloomPoint _bloom;
        private bool _hasDelay;

        public RustyLamp(EntityData data, Vector2 offset) : 
            this (
                data.Position,
                offset,
                data.Attr("activationId", ""),
                data.Float("initialDelay", 0f),
                data.Attr("strobePattern", "None"),
                data.Bool("startActive", false))
        {
        }

        public RustyLamp(Vector2 position, Vector2 offset, string activationId, float initialDelay, string strobePattern, bool startActive)
        {
            Position = position + offset;
            _activationId = activationId;
            _initialDelay = initialDelay;
            _hasDelay = initialDelay > 0f;
            _startActive = startActive;
            Add(_sprite = new Sprite(GFX.Game, "objects/FactoryHelper/rustyLamp/rustyLamp"));
            _sprite.Add("frames", "");
            _sprite.Play("frames");
            _sprite.Active = false;
            if (!_startActive && !_hasDelay)
            {
                _sprite.SetAnimationFrame(1);
            }

            string activationString = activationId == string.Empty ? null : $"FactoryActivation:{activationId}";
            Add(_activator = new FactoryActivationComponent(activationString));
            SetStrobePattern(strobePattern);

            Add(_light = new VertexLight(Color, 1f, 128, 256));
            Add(_bloom = new BloomPoint(0.5f, 16f));
            _light.Position = new Vector2(8, 8);
            _bloom.Position = new Vector2(8, 8);
        }

        public override void Update()
        {
            base.Update();
            Console.WriteLine(Activated);
            if ((_initialDelay > 0f) && Activated)
            {
                _initialDelay -= Engine.DeltaTime;
            } 
            else if (_hasDelay)
            {
                if (_startActive != Activated)
                {
                    TurnOn();
                }
                else
                {
                    TurnOff();
                }
                _hasDelay = false;
            }
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (_activationId != null)
            {
                string activationString = $"FactoryActivation:{_activationId}";
                Level level = SceneAs<Level>();
                if (level.Session.GetFlag(activationString) || level.Session.GetFlag("Persistent" + activationString))
                {
                    Activated = true;
                }
                else
                {
                    Activated = false;
                }
            }
        }

        private void SetStrobePattern(string strobePattern)
        {
            switch (strobePattern)
            {
                default:
                case "None":
                    break;
                case "LightFlicker":
                    Add(new Coroutine(LightFlicker()));
                    break;
            }
        }

        private IEnumerator LightFlicker()
        {
            for (; ; )
            {
                while ((Activated == _startActive) || _hasDelay)
                {
                    yield return null;
                }
                yield return Calc.Random.NextFloat(10f) + 2f;
                int flickerCount = Calc.Random.Next(3, 7);
                float flickerLength = Calc.Random.NextFloat(0.02f) + 0.01f;
                for (int i = 0; i < flickerCount; i++)
                {
                    TurnOff();
                    yield return flickerLength;
                    TurnOn();
                    yield return flickerLength;
                }
            }
        }

        private void TurnOn()
        {
            _sprite.SetAnimationFrame(1);
            _light.Visible = true;
            _bloom.Visible = true;
        }

        private void TurnOff()
        {
            _sprite.SetAnimationFrame(0);
            _light.Visible = false;
            _bloom.Visible = false;
        }
    }
}
