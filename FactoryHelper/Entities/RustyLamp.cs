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
    class RustyLamp : Entity
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

        public static readonly Color Color = Color.Lerp(Color.White, Color.Orange, 0.5f);
        
        private string _activationId;
        private float _initialDelay;
        private bool _startActive;
        private Sprite _sprite;
        private VertexLight _light;
        private BloomPoint _bloom;
        private bool _hasDelay;
        private bool _startsWithDelay;

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
            _activationId = activationId == string.Empty ? null : $"FactoryActivation:{activationId}";
            _initialDelay = initialDelay;
            _hasDelay = initialDelay > 0f;
            _startsWithDelay = _hasDelay;
            Console.WriteLine($"Starts with delay: {_hasDelay}");
            _startActive = startActive;
            Add(_sprite = new Sprite(GFX.Game, "objects/FactoryHelper/rustyLamp/rustyLamp"));
            _sprite.Add("frames", "");
            _sprite.Play("frames");
            _sprite.Active = false;
            
            SetStrobePattern(strobePattern);

            Add(_light = new VertexLight(Color, 1f, 128, 128));
            Add(_bloom = new BloomPoint(0.5f, 16f));
            _light.Position = new Vector2(8, 8);
            _bloom.Position = new Vector2(8, 8);
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            SwitchOnOrOff();
            if (Activated)
            {
                _hasDelay = false;
                _startsWithDelay = false;
            }
        }

        public override void Update()
        {
            base.Update();
            if (Activated && _hasDelay)
            {
                _initialDelay -= Engine.DeltaTime;
                if (_initialDelay <= 0f)
                {
                    _hasDelay = false;
                }
            }
        }

        private void SwitchOnOrOff()
        {
            if (_startActive != Activated)
            {
                TurnOn();
            }
            else
            {
                TurnOff();
            }
        }

        private void SetStrobePattern(string strobePattern)
        {
            switch (strobePattern)
            {
                default:
                case "None":
                    Add(new Coroutine(PatternNone()));
                    break;
                case "FlickerOn":
                    Add(new Coroutine(PatternFlickerOn()));
                    break;
                case "LightFlicker":
                    Add(new Coroutine(PatternLightFlicker()));
                    break;
                case "TurnOffFlickerOn":
                    Add(new Coroutine(PatternTurnOffFlickerOn()));
                    break;
            }
        }

        private IEnumerator PatternNone()
        {
            while ((Activated == _startActive) || _hasDelay)
            {
                yield return null;
            }
            TurnOn();
        }

        private IEnumerator PatternFlickerOn()
        {
            bool firstRun = true;
            while ((Activated == _startActive) || _hasDelay)
            {
                yield return null;
            }
            TurnOn();
            if (firstRun)
            {
                if (_startsWithDelay)
                {
                    yield return FlickerOn();
                    firstRun = false;
                }
                else
                {
                    TurnOn();
                }
            }
        }

        private IEnumerator PatternLightFlicker()
        {
            bool firstRun = true;
            for (; ; )
            {
                while ((Activated == _startActive) || _hasDelay)
                {
                    yield return null;
                }
                if (firstRun)
                {
                    if (_startsWithDelay)
                    {
                        yield return FlickerOn();
                        firstRun = false;
                    }
                    else
                    {
                        TurnOn();
                    }
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
            bool firstRun = true;
            for (;;)
            {
                while ((Activated == _startActive) || _hasDelay)
                {
                    yield return null;
                }
                if (firstRun)
                {
                    if (_startsWithDelay)
                    {
                        yield return FlickerOn();
                        firstRun = false;
                    }
                    else
                    {
                        TurnOn();
                    }
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
            _bloom.Alpha = dimnes;
        }
    }
}
