using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.AdventureHelper.Entities
{
    [CustomEntity("AdventureHelper/CustomCrystalHeart")]
    [TrackedAs(typeof(HeartGem))]
    public class CustomCrystalHeart : HeartGem
    {
        protected Sprite spriteOutline;
        protected EntityID entityID;
        protected Color color;
        protected string spriteId;

        protected DynData<HeartGem> baseData;

        public CustomCrystalHeart(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            baseData = new DynData<HeartGem>(this);

            entityID = new EntityID(data.Level.Name, data.ID);
            color = Calc.HexToColor(data.Attr("color", "00a81f"));
            spriteId = data.Attr("path", "");
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);

            Sprite sprite = baseData.Get<Sprite>("sprite");
            Remove(sprite);
            if (!string.IsNullOrWhiteSpace(spriteId))
            {
                sprite = GFX.SpriteBank.Create(spriteId);
                sprite.Play("spin");
                if (IsGhost)
                    sprite.Color = Color.White * 0.8f;

                switch (spriteId)
                {
                    case "heartgem0":
                        color = Color.Aqua;
                        baseData["shineParticle"] = P_BlueShine;
                        break;
                    case "heartgem1":
                        color = Color.Red;
                        baseData["shineParticle"] = P_RedShine;
                        break;
                    case "heartgem2":
                        color = Color.Gold;
                        baseData["shineParticle"] = P_GoldShine;
                        break;
                    case "heartgem3":
                        color = Calc.HexToColor("dad8cc");
                        baseData["shineParticle"] = P_FakeShine;
                        break;
                }
            }
            else
            {
                sprite = AdventureHelperModule.SpriteBank.Create("adventureHelper_recolorHeart");
                spriteOutline = AdventureHelperModule.SpriteBank.Create("adventureHelper_recolorHeartOutline");
                if (IsGhost)
                    sprite.Color = Color.Lerp(color, Color.White, 0.8f) * 0.8f;
                else
                    sprite.Color = color;

                baseData["shineParticle"] = new ParticleType(P_BlueShine) { Color = color };
            }
            

            sprite.OnLoop = anim =>
            {
                if (Visible && anim == "spin" && baseData.Get<bool>("autoPulse"))
                {
                    if (IsFake)
                    {
                        Audio.Play("event:/new_content/game/10_farewell/fakeheart_pulse", Position);
                    }
                    else
                    {
                        Audio.Play("event:/game/general/crystalheart_pulse", Position);
                    }
                    ScaleWiggler.Start();
                    (Scene as Level).Displacement.AddBurst(Position, 0.35f, 8f, 48f, 0.25f);
                }
            };

            Remove(ScaleWiggler);
            ScaleWiggler = Wiggler.Create(0.5f, 4f, f => sprite.Scale = Vector2.One * (1f + f * 0.25f));
            Add(ScaleWiggler);

            baseData["sprite"] = sprite;
            Add(sprite);
            if (spriteOutline != null)
                Add(spriteOutline);

            baseData.Get<VertexLight>("light").Color = Color.Lerp(color, Color.White, 0.5f);
        }

        public override void Update()
        {
            base.Update();
            Sprite sprite = baseData.Get<Sprite>("sprite");
            if (spriteOutline != null)
            {
                spriteOutline.Position = sprite.Position;
                spriteOutline.Scale = sprite.Scale;
                if (spriteOutline.CurrentAnimationID != sprite.CurrentAnimationID)
                {
                    spriteOutline.Play(sprite.CurrentAnimationID);
                }
                spriteOutline.SetAnimationFrame(sprite.CurrentAnimationFrame);
            }
        }
    }

}
