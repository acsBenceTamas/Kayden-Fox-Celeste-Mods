using Celeste;
using Celeste.Mod.Entities;
using FactoryHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace FactoryHelper.Entities
{
    [CustomEntity("FactoryHelper/RustyLamp")]
    class RustyLamp : Entity
    {
        public static readonly Color Color = Color.Lerp(Color.White, Color.Orange, 0.5f);

        public FactoryActivatorComponent Activator { get; }

        private float _initialDelay;
        private readonly Sprite _sprite;
        private readonly VertexLight _light;
        private readonly BloomPoint _bloom;
        private Coroutine _strobePattern;
        private string _strobePatternString;
        private bool _startedOn;

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

        public RustyLamp(Vector2 position, Vector2 offset, string activationId, float initialDelay, string strobePattern, bool startActive) : base()
        {
            Depth = 10000 - 1;
            Position = position + offset;

            Add(Activator = new FactoryActivatorComponent());
            Activator.ActivationId = activationId == string.Empty ? null : activationId;
            Activator.StartOn = startActive;
            Activator.OnTurnOff = OnTurnOff;
            Activator.OnTurnOn = OnTurnOn;
            Activator.OnStartOff = OnStartOff;
            Activator.OnStartOn = OnStartOn;

            _initialDelay = initialDelay;
            Add(_sprite = new Sprite(GFX.Game, "objects/FactoryHelper/rustyLamp/rustyLamp"));
            _sprite.Add("frames", "");
            _sprite.Play("frames");
            _sprite.Active = false;

            _strobePatternString = strobePattern;

            Add(_light = new VertexLight(Color, 0f, 128, 128));
            Add(_bloom = new BloomPoint(0.0f, 16f));
            _light.Position = new Vector2(8, 8);
            _bloom.Position = new Vector2(8, 8);
        }

        public override void Update()
        {
            base.Update();
            if (Activator.IsOn)
            {
                if (_initialDelay > 0f)
                {
                    _initialDelay -= Engine.DeltaTime;
                }
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Activator.Added(scene);
        }

        private void OnStartOn(Scene scene)
        {
            _startedOn = true;
            _initialDelay = 0f;
            TurnOn();
            SetLightLevel(1f);
            SetStrobePattern(_strobePatternString);
        }

        private void OnStartOff(Scene scene)
        {
            _startedOn = false;
            SetLightLevel(0f);
        }

        private void OnTurnOn()
        {
            SetStrobePattern(_strobePatternString);
        }

        private void OnTurnOff()
        {
            SetStrobePattern("FlickerOff");
            _startedOn = false;
        }

        private void SetStrobePattern(string strobePattern)
        {
            if (_strobePattern != null)
            {
                Remove(_strobePattern);
            }
            switch (strobePattern)
            {
                default:
                case "None":
                    Add(_strobePattern = new Coroutine(PatternNone()));
                    break;
                case "FlickerOn":
                    Add(_strobePattern = new Coroutine(PatternFlickerOn()));
                    break;
                case "FlickerOff":
                    Add(_strobePattern = new Coroutine(FlickerOff()));
                    break;
                case "LightFlicker":
                    Add(_strobePattern = new Coroutine(PatternLightFlicker()));
                    break;
                case "TurnOffFlickerOn":
                    Add(_strobePattern = new Coroutine(PatternTurnOffFlickerOn()));
                    break;
            }
        }

        private IEnumerator WaitForActivation()
        {
            while (!Activator.IsOn || (_initialDelay > 0f))
            {
                yield return null;
            }
        }

        private IEnumerator PatternNone()
        {
            yield return WaitForActivation();
            TurnOn();
        }

        private IEnumerator PatternFlickerOn()
        {
            yield return WaitForActivation();
            if (!_startedOn)
            {
                yield return FlickerOn();
            }
        }

        private IEnumerator PatternLightFlicker()
        {
            yield return WaitForActivation();
            bool firstRun = true;
            for (; ; )
            {
                if (!_startedOn && firstRun)
                {
                    yield return FlickerOn();
                    firstRun = false;
                }
                yield return Calc.Random.NextFloat(10f) + 2f;
                int flickerCount = Calc.Random.Next(3, 7);
                float flickerLength = Calc.Random.NextFloat(0.02f) + 0.01f;
                for (int i = 0; i < flickerCount; i++)
                {
                    SetLightLevel(0.9f);
                    yield return flickerLength;
                    SetLightLevel(1.0f);
                    yield return flickerLength;
                }
            }
        }

        private IEnumerator PatternTurnOffFlickerOn()
        {
            yield return WaitForActivation();
            bool firstRun = true;
            for (; ; )
            {
                if (!_startedOn && firstRun)
                {
                    yield return FlickerOn();
                    firstRun = false;
                }
                yield return Calc.Random.NextFloat(5f) + 10f;
                SetLightLevel(0.5f);
                yield return 0.2f;
                TurnOff();
                yield return Calc.Random.NextFloat(2f) + 0.5f;
                yield return FlickerOn();
            }
        }

        private IEnumerator FlickerOn()
        {
            TurnOn();
            SetLightLevel(0.0f);
            int flickerCount = Calc.Random.Next(3, 7);
            float flickerLength = Calc.Random.NextFloat(0.02f) + 0.01f;
            for (int i = 0; i < flickerCount; i++)
            {
                SetLightLevel(0.5f * (i + 1) / flickerCount);
                yield return flickerLength;
                SetLightLevel(1.0f * (i + 1) / flickerCount);
                yield return flickerLength;
            }
        }

        private IEnumerator FlickerOff()
        {
            TurnOn();
            SetLightLevel(1.0f);
            int flickerCount = Calc.Random.Next(3, 7);
            float flickerLength = Calc.Random.NextFloat(0.02f) + 0.01f;
            for (int i = 0; i < flickerCount; i++)
            {
                SetLightLevel(0.5f * (1-(i + 1) / flickerCount));
                yield return flickerLength;
                SetLightLevel(1.0f * (1-(i + 1) / flickerCount));
                yield return flickerLength;
            }
            TurnOff();
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

        private void SetLightLevel(float dimnes)
        {
            _light.Alpha = dimnes;
            _bloom.Alpha = dimnes/2;
        }
    }
}
