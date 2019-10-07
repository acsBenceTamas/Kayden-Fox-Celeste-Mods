using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace FactoryHelper.Entities
{
    [CustomEntity("FactoryHelper/RustyJumpthruPlatform")]
    public class RustyJumpthruPlatform : JumpThru
    {
        private readonly int _columns;

        public RustyJumpthruPlatform(Vector2 position, int width)
            : base(position, width, safe: true)
        {
            _columns = width / 8;
            Depth = -60;
        }

        public RustyJumpthruPlatform(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Width)
        {
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);

            SurfaceSoundIndex = 7;
            MTexture mTexture = GFX.Game["objects/FactoryHelper/jumpThru/rustyMetal"];
            int num = mTexture.Width / 8;
            for (int i = 0; i < _columns; i++)
            {
                int x;
                int y;
                if (i == 0)
                {
                    x = 0;
                    y = ((!CollideCheck<Solid>(Position + new Vector2(-1f, 0f))) ? 1 : 0);
                }
                else if (i == _columns - 1)
                {
                    x = num - 1;
                    y = ((!CollideCheck<Solid>(Position + new Vector2(1f, 0f))) ? 1 : 0);
                }
                else
                {
                    x = 1 + Calc.Random.Next(num - 2);
                    y = Calc.Random.Choose(0, 1);
                }
                Image image = new Image(mTexture.GetSubtexture(x * 8, y * 8, 8, 8))
                {
                    X = i * 8
                };
                Add(image);
            }
        }
    }
}
