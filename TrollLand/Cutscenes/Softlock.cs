using System.Collections;
using Celeste;
using Monocle;

namespace TrollLand.Cutscenes
{
    [Tracked(false)]
    class Softlock : CutsceneEntity
    {
        public override void OnBegin(Level level)
        {
            TrollLandModule.Session.InSoftLock = true;
            Add(new Coroutine(CheckEnd(level)));
        }

        public override void Update()
        {
            base.Update();
        }

        private IEnumerator CheckEnd(Level level)
        {
            while (TrollLandModule.Session.InSoftLock)
            {
                yield return null;
            }
            EndCutscene(level);
        }
        public override void OnEnd(Level level)
        {
        }
    }
}
