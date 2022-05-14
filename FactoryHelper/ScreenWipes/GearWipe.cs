using System;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace FactoryHelper.ScreenWipes
{
    public class GearWipe : ScreenWipe
    {
        private struct Gear
        {
            public const int ToothCount = 8;

            public float Scale;
            public float Angle;
            public float X;
            public float Y;
            public int Direction;

            private const float _turnSpeed = 12f;

            public Gear(int index)
            {
                Scale = Calc.Random.NextFloat() * 0.2f + 0.8f;
                Angle = Calc.Random.NextAngle();
                X = Calc.Random.NextFloat(Engine.Width/2) + index % 2 * Engine.Width/2;
                Y = Calc.Random.NextFloat(Engine.Height/2) + index % 2 * Engine.Height/2;
                Direction = Calc.Random.NextFloat() > 0.5 ? 1 : -1;
            }

            public void Update()
            {
                Angle += Direction * _turnSpeed * Engine.DeltaTime;
            }
        }

        public static readonly BlendState SubtractBlendmode = new BlendState
        {
            ColorSourceBlend = Blend.One,
            ColorDestinationBlend = Blend.One,
            ColorBlendFunction = BlendFunction.ReverseSubtract,
            AlphaSourceBlend = Blend.One,
            AlphaDestinationBlend = Blend.One,
            AlphaBlendFunction = BlendFunction.Add
        };

        private const int _gearCount = 8;
        private const int _countPerTeeth = 16;

        private Gear[] _gears = new Gear[_gearCount];
        private VertexPositionColor[] _verts = new VertexPositionColor[_gearCount * _countPerTeeth * Gear.ToothCount * 3];
        private bool _hasDrawn;

        public GearWipe(Scene scene, bool wipeIn, Action onComplete = null) : base(scene, wipeIn, onComplete)
        {
            for (int i = 0; i < _gears.Length; i++)
            {
                _gears[i] = new Gear(i);
            }

            for (int j = 0; j < _verts.Length; j++)
            {
                _verts[j].Color = (WipeIn ? Color.Black : Color.White);
            }
        }

        public override void Update(Scene scene)
        {
            base.Update(scene);
            for(int i = 0; i < _gears.Length; i++)
            {
                _gears[i].Update();
            }
        }

        public override void BeforeRender(Scene scene)
        {
            DrawGears();
        }

        public override void Render(Scene scene)
        {
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, SubtractBlendmode, SamplerState.LinearClamp, null, null, null, Engine.ScreenMatrix);
            if ((WipeIn && Percent <= 0.01f) || (!WipeIn && Percent >= 0.99f))
            {
                Draw.Rect(-1f, -1f, 1922f, 1082f, Color.White);
            }
            else if (_hasDrawn)
            {
                Draw.SpriteBatch.Draw(Celeste.Celeste.WipeTarget, new Vector2(-1f, -1f), Color.White);
            }
            Draw.SpriteBatch.End();
        }

        private void DrawGears()
        {
            _hasDrawn = true;
            Engine.Graphics.GraphicsDevice.SetRenderTarget(Celeste.Celeste.WipeTarget);
            Engine.Graphics.GraphicsDevice.Clear(WipeIn ? Color.White : Color.Black);
            if (Percent > 0.9f)
            {
                float num = Calc.Map(Percent, 0.9f, 1f) * 1082f;
                Draw.SpriteBatch.Begin();
                Draw.Rect(-1f, (1080f - num) * 0.5f, 1922f, num, (!WipeIn) ? Color.White : Color.Black);
                Draw.SpriteBatch.End();
            }
            float sizeMultiplier = Ease.CubeIn(Percent);
            float baseSize = 1000f;

            for (int i = 0; i < _gears.Length; i++)
            {
                int index = 0;
                for (int j = 0; j < Gear.ToothCount; j++)
                {
                    for (int k = 0; k < _countPerTeeth; k++)
                    {
                        Gear gear = _gears[i];
                        float size = k >= 8 ? baseSize : 1.2f * baseSize;
                        float angle = gear.Angle + (j * Calc.Circle / Gear.ToothCount) + k * Calc.Circle / Gear.ToothCount / _countPerTeeth;
                        _verts[index++].Position = new Vector3(Calc.AngleToVector(angle, size * sizeMultiplier * gear.Scale) + new Vector2(gear.X, gear.Y), 0f);
                        angle += Calc.Circle / Gear.ToothCount / _countPerTeeth;
                        _verts[index++].Position = new Vector3(Calc.AngleToVector(angle, size * sizeMultiplier * gear.Scale) + new Vector2(gear.X, gear.Y), 0f);
                        _verts[index++].Position = new Vector3(new Vector2(gear.X, gear.Y), 0f);
                    }
                }
                GFX.DrawVertices(Matrix.Identity, _verts, _verts.Length);
            }
        }
    }
}
