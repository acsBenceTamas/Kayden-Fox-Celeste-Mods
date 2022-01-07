using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace FactoryHelper.Entities
{
    public class SteamPoof : Entity
    {
        public Action<SteamPoof> OnRemoved;
        public Sprite sprite;
        public Color _color = Color.White;

        public SteamPoof(Vector2 position, float fade, Color color) : base(position)
        {
            _color = color;
            Depth = -100001;
            string type = Calc.Random.Chance(0.1f) ? "b" : "a";
            sprite = new Sprite(GFX.Game, "danger/FactoryHelper/steamWall/");
            sprite.Add("poof", "poof_" + type, 0.07f);
            sprite.Play("poof");
            float scale = Calc.Random.NextFloat(0.4f) + 0.4f;
            sprite.Scale = new Vector2(scale, scale);
            sprite.Rotation = Calc.Random.NextAngle();
            sprite.Color = Color.Lerp(Color.Lerp(_color, Color.Black, 0.3f) * 0.8f, Color.Lerp(_color, Color.Black, 0.5f) * 0.5f, Calc.Random.NextFloat()) * fade;
            sprite.CenterOrigin();
            Add(sprite);
        }


        public static IEnumerable<SteamPoof> Create(Scene scene, Vector2 position, Vector2 range, Color color, int count=1, float fade=1f, Action<SteamPoof> onRemoved = null)
        {
            SteamPoof[] poofs = new SteamPoof[count];
            for (int i = 0; i < count; i++)
            {
                SteamPoof poof = new SteamPoof(new Vector2(position.X - range.X / 2 + Calc.Random.NextFloat(range.X), position.Y - range.Y / 2 + Calc.Random.NextFloat(range.Y)), fade, color);
                poof._color = color;
                scene.Add(poof);
                poofs[i] = poof;
                poof.OnRemoved = onRemoved;
            }
            return poofs;
        }

        public override void Update()
        {
            base.Update();
            if (!sprite.Animating)
            {
                RemoveSelf();
            }
        }

        public override void Removed(Scene scene)
        {
            OnRemoved?.Invoke(this);
            base.Removed(scene);
        }
    }

}
