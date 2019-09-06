using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.AdventureHelper.Entities
{
    class DustTrackSpinnerMultinode : MultipleNodeTrackSpinner
    {
        private DustGraphic dusty;

        private Vector2 previousVector;

        private Vector2 nextVector;

        public DustTrackSpinnerMultinode(EntityData data, Vector2 offset) : base(data, offset)
        {
            base.Add(this.dusty = new DustGraphic(true, false, false));
            var start = base.Path[base.CurrentStart];
            var next = base.Path[(base.CurrentStart + 1) % base.Path.Length];
            this.dusty.EyeDirection = (this.dusty.EyeTargetDirection = (next - start).SafeNormalize());
            this.dusty.OnEstablish = new Action(this.Establish);
            base.Depth = -50;
        }
        private void Establish()
        {
            var current = base.Path[base.CurrentStart];
            var next = base.Path[(base.CurrentStart + 1) % base.Path.Length];
            var previous = base.Path[(base.CurrentStart - 1 + base.Path.Length) % base.Path.Length];
            nextVector = (next - current).SafeNormalize();
            previousVector = (current - previous).SafeNormalize();
            bool flag = base.Scene.CollideCheck<Solid>(new Rectangle((int)(base.X + nextVector.X * 4f) - 2, (int)(base.Y + nextVector.Y * 4f) - 2, 4, 4));
            bool flag2 = !flag;
            if (flag2)
            {
                nextVector = -nextVector;
                flag = base.Scene.CollideCheck<Solid>(new Rectangle((int)(base.X + nextVector.X * 4f) - 2, (int)(base.Y + nextVector.Y * 4f) - 2, 4, 4));
            }
            bool flag3 = flag;
            if (flag3)
            {
                float num = (current - next).Length();
                int num2 = 8;
                while ((float)num2 < num && flag)
                {
                    flag = (flag && base.Scene.CollideCheck<Solid>(new Rectangle((int)(base.X + nextVector.X * 4f + previousVector.X * (float)num2) - 2, (int)(base.Y + nextVector.Y * 4f + previousVector.Y * (float)num2) - 2, 4, 4)));
                    num2 += 8;
                }
                bool flag4 = flag;
                if (flag4)
                {
                    List<DustGraphic.Node> list = null;
                    bool flag5 = nextVector.X < 0f;
                    if (flag5)
                    {
                        list = this.dusty.LeftNodes;
                    }
                    else
                    {
                        bool flag6 = nextVector.X > 0f;
                        if (flag6)
                        {
                            list = this.dusty.RightNodes;
                        }
                        else
                        {
                            bool flag7 = nextVector.Y < 0f;
                            if (flag7)
                            {
                                list = this.dusty.TopNodes;
                            }
                            else
                            {
                                bool flag8 = nextVector.Y > 0f;
                                if (flag8)
                                {
                                    list = this.dusty.BottomNodes;
                                }
                            }
                        }
                    }
                    bool flag9 = list != null;
                    if (flag9)
                    {
                        foreach (DustGraphic.Node node in list)
                        {
                            node.Enabled = false;
                        }
                    }
                    this.dusty.Position -= nextVector;
                    this.dusty.EyeDirection = (this.dusty.EyeTargetDirection = Calc.AngleToVector(Calc.AngleLerp(this.previousVector.Angle(), this.nextVector.Angle(), 0.3f), 1f));
                }
            }
        }
        public override void Update()
        {
            base.Update();
            bool flag = this.Moving && this.PauseTimer < 0f && base.Scene.OnInterval(0.02f);
            if (flag)
            {
                base.SceneAs<Level>().ParticlesBG.Emit(DustStaticSpinner.P_Move, 1, this.Position, Vector2.One * 4f);
            }
        }

        public override void OnPlayer(Player player)
        {
            base.OnPlayer(player);
            this.dusty.OnHitPlayer();
        }

        public override void OnTrackEnd()
        {
            base.OnTrackEnd();
            var current = this.Path[CurrentStart];
            var previous = this.Path[(CurrentStart - 1 + Path.Length) % Path.Length];
            previousVector = (previous - current).SafeNormalize();
            nextVector = Calc.AngleToVector(Angle, 1f);
            this.dusty.EyeTargetDirection = Calc.AngleToVector(Calc.AngleLerp(this.previousVector.Angle(), this.Angle, 1.0f), 1f);
        }
    }
}
