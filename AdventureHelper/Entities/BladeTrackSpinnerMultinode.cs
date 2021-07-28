using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.AdventureHelper.Entities
{
    class BladeTrackSpinnerMultinode : MultipleNodeTrackSpinner
    {
        public Sprite Sprite;
        private bool hasStarted;

        public BladeTrackSpinnerMultinode(EntityData data, Vector2 offset) : base(data, offset)
        {
            base.Add(this.Sprite = GFX.SpriteBank.Create("templeBlade"));
            this.Sprite.Play("idle", false, false);
            base.Depth = -50;
            base.Add(new MirrorReflection());
        }
        public override void OnTrackStart()
        {
            this.Sprite.Play("spin", false, false);
            bool flag = this.hasStarted;
            if (flag)
            {
                Audio.Play("event:/game/05_mirror_temple/bladespinner_spin", this.Position);
            }
            this.hasStarted = true;
        }

        public override void Update()
        {
            bool reachedDestination = PauseTimer > 0f;
            bool wasPaused = !base.Moving;
            base.Update();
            if (wasPaused && base.Moving && !reachedDestination)
            {
                if (this.hasStarted)
                {
                    this.Sprite.Play("spin", false, false);
                    Audio.Play("event:/game/05_mirror_temple/bladespinner_spin", this.Position);
                }
            }
        }
    }
}
