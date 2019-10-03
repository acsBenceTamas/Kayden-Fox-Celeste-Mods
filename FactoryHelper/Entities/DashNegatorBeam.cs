using Celeste;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryHelper.Entities
{
    public class DashNegatorBeam : Entity
    {
        private float _beamAlpha = 0f;
        private float _sideFadeAlpha = 1f;
        private Sprite _beamSprite;
        private Sprite _beamStartSprite;
        private VertexPositionColor[] fade = new VertexPositionColor[24];
        private Vector2 _beamOrigin;
        private float _angle = Calc.Angle(Vector2.UnitY);

        public DashNegatorBeam(Vector2 origin)
        {
            Add(_beamSprite = GFX.SpriteBank.Create("badeline_beam"));
            Add(_beamStartSprite = GFX.SpriteBank.Create("badeline_beam_start"));
            _beamOrigin = origin;
            _beamSprite.OnLastFrame = delegate (string anim)
            {
                if (anim == "shoot")
                {
                    RemoveSelf();
                }
            };
            Depth = -1000000;
            _beamSprite.Play("shoot");
            _beamStartSprite.Play("shoot");
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            DissipateParticles();
        }

        public override void Update()
        {
            base.Update();
            _beamAlpha = Calc.Approach(_beamAlpha, 1f, 16f * Engine.DeltaTime);
            _sideFadeAlpha = Calc.Approach(_sideFadeAlpha, 0f, Engine.DeltaTime * 8f);
        }
        public override void Render()
        {
            Vector2 beamOrigin = _beamOrigin;
            Vector2 vector = Calc.AngleToVector(_angle, _beamSprite.Width);
            _beamSprite.Rotation = _angle;
            _beamSprite.Color = Color.White * _beamAlpha;
            _beamStartSprite.Rotation = _angle;
            _beamStartSprite.Color = Color.White * _beamAlpha;
            if (_beamSprite.CurrentAnimationID == "shoot")
            {
                beamOrigin += Calc.AngleToVector(_angle, 8f);
            }
            for (int i = 0; i < 15; i++)
            {
                _beamSprite.RenderPosition = beamOrigin;
                _beamSprite.Render();
                beamOrigin += vector;
            }
            if (_beamSprite.CurrentAnimationID == "shoot")
            {
                _beamStartSprite.RenderPosition = _beamOrigin;
                _beamStartSprite.Render();
            }
            GameplayRenderer.End();
            Vector2 vector2 = vector.SafeNormalize();
            Vector2 vector3 = vector2.Perpendicular();
            Color color = Color.Black * _sideFadeAlpha * 0.35f;
            Color transparent = Color.Transparent;
            vector2 *= 4000f;
            vector3 *= 120f;
            int v = 0;
            Quad(ref v, beamOrigin, -vector2 + vector3 * 2f, vector2 + vector3 * 2f, vector2 + vector3, -vector2 + vector3, color, color);
            Quad(ref v, beamOrigin, -vector2 + vector3, vector2 + vector3, vector2, -vector2, color, transparent);
            Quad(ref v, beamOrigin, -vector2, vector2, vector2 - vector3, -vector2 - vector3, transparent, color);
            Quad(ref v, beamOrigin, -vector2 - vector3, vector2 - vector3, vector2 - vector3 * 2f, -vector2 - vector3 * 2f, color, color);
            GFX.DrawVertices((base.Scene as Level).Camera.Matrix, fade, fade.Length);
            GameplayRenderer.Begin();
        }

        private void Quad(ref int v, Vector2 offset, Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color ab, Color cd)
        {
            fade[v].Position.X = offset.X + a.X;
            fade[v].Position.Y = offset.Y + a.Y;
            fade[v++].Color = ab;
            fade[v].Position.X = offset.X + b.X;
            fade[v].Position.Y = offset.Y + b.Y;
            fade[v++].Color = ab;
            fade[v].Position.X = offset.X + c.X;
            fade[v].Position.Y = offset.Y + c.Y;
            fade[v++].Color = cd;
            fade[v].Position.X = offset.X + a.X;
            fade[v].Position.Y = offset.Y + a.Y;
            fade[v++].Color = ab;
            fade[v].Position.X = offset.X + c.X;
            fade[v].Position.Y = offset.Y + c.Y;
            fade[v++].Color = cd;
            fade[v].Position.X = offset.X + d.X;
            fade[v].Position.Y = offset.Y + d.Y;
            fade[v++].Color = cd;
        }

        private void DissipateParticles()
        {
            Level level = SceneAs<Level>();
            Vector2 source = source = _beamOrigin + Calc.AngleToVector(_angle, 8f);
            Vector2 from = Calc.AngleToVector(_angle, 12f);
            Vector2 to = Calc.AngleToVector(_angle, 2000f);
            Vector2 perpendicular = (to - from).Perpendicular().SafeNormalize();
            Vector2 range = (to - from).SafeNormalize();
            Vector2 min = -perpendicular * 1f;
            Vector2 max = perpendicular * 1f;
            float left = perpendicular.Angle();
            float right = (-perpendicular).Angle();
            for (int i = 0; i < 200; i += 12)
            {
                for (int j = -1; j <= 1; j += 2)
                {
                    level.ParticlesFG.Emit(FinalBossBeam.P_Dissipate, source + range * i + perpendicular * 2f * j + Calc.Random.Range(min, max), left);
                    level.ParticlesFG.Emit(FinalBossBeam.P_Dissipate, source + range * i - perpendicular * 2f * j + Calc.Random.Range(min, max), right);
                }
            }
        }
    }
}
