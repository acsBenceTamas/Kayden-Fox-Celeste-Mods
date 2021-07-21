using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.AdventureHelper.Entities
{
    class StarTrackSpinnerMultinode : MultipleNodeTrackSpinner
    {
        public Sprite Sprite;
        private bool hasStarted;
        private bool trail;
        private int colorID;

        public StarTrackSpinnerMultinode(EntityData data, Vector2 offset) : base(data, offset)
        {
            base.Add(this.Sprite = GFX.SpriteBank.Create("moonBlade"));
            this.colorID = Calc.Random.Choose(0, 1, 2);
            this.Sprite.Play("idle" + this.colorID, false, false);
            base.Depth = -50;
            base.Add(new MirrorReflection());
        }
        public override void OnTrackStart()
        {
            this.colorID++;
            this.colorID %= 3;
            this.Sprite.Play("spin" + this.colorID, false, false);
            if (this.hasStarted)
            {
                Audio.Play("event:/game/05_mirror_temple/bladespinner_spin", this.Position);
            }
            this.hasStarted = true;
            this.trail = true;

        }
        public override void OnTrackEnd()
        {
            this.trail = false;
        }
        public override void Update()
        {
            base.Update();
            if (this.trail && base.Scene.OnInterval(0.03f))
            {
                base.SceneAs<Level>().ParticlesBG.Emit(StarTrackSpinner.P_Trail[this.colorID], 1, this.Position, Vector2.One * 3f);
            }
        }
    }
}
