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
        private Sprite _sprite;

        public SteamPoof(Vector2 position, float fade) : base(position)
        {
            Depth = -100001;
            string type = Calc.Random.Chance(0.1f) ? "b" : "a";
            Add(_sprite = new Sprite(GFX.Game, "danger/FactoryHelper/steamWall/"));
            _sprite.Add("poof", "poof_" + type, 0.07f);
            _sprite.Play("poof");
            float scale = Calc.Random.NextFloat(0.4f) + 0.4f;
            _sprite.Scale = new Vector2(scale, scale);
            _sprite.Rotation = Calc.Random.NextAngle();
            _sprite.Color = Color.Lerp(new Color(0.9f, 0.9f, 0.9f) * 0.8f, new Color(0.7f, 0.7f, 0.7f) * 0.5f, Calc.Random.NextFloat()) * fade;
            _sprite.CenterOrigin();
        }

        public static IEnumerable<SteamPoof> Create(Scene scene, Vector2 position, Vector2 range, int count=1, float fade=1f, Action<SteamPoof> onRemoved = null)
        {
            SteamPoof[] poofs = new SteamPoof[count];
            for (int i = 0; i < count; i++)
            {
                SteamPoof poof = new SteamPoof(new Vector2(position.X - range.X / 2 + Calc.Random.NextFloat(range.X), position.Y - range.Y / 2 + Calc.Random.NextFloat(range.Y)), fade);
                scene.Add(poof);
                poofs[i] = poof;
                poof.OnRemoved = onRemoved;
            }
            return poofs;
        }

        public override void Update()
        {
            base.Update();
            if (!_sprite.Animating)
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
