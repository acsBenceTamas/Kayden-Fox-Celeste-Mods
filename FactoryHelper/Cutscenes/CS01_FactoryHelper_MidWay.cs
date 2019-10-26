using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace FactoryHelper.Cutscenes
{
    class CS01_FactoryHelper_MidWay : CutsceneEntity
    {
        private readonly Player _player;

        public CS01_FactoryHelper_MidWay (Player player)
        {
            _player = player;
        }

        public override void OnBegin(Level level)
        {
            Add(new Coroutine(Cutscene(level)));
        }

        private IEnumerator Cutscene(Level level)
        {
            _player.StateMachine.State = 11;
            while (!_player.OnGround())
            {
                yield return null;
            }
            yield return PanCamera(_player.CameraTarget + Vector2.UnitX * 120f, 0.5f);
            yield return 1f;
            yield return Textbox.Say("KaydenFox_FactoryMod_1_Factory_A_MidWay", WalkRight, AfterLookDown);
            EndCutscene(level);
        }

        private IEnumerator WalkRight()
        {
            Add(new Coroutine(_player.DummyWalkTo(Level.Bounds.Left + 176f, false, 0.8f)));
            yield return 0.5f;
            yield return PanCamera(_player.CameraTarget + new Vector2(120f, 320f), 0.2f);
        }


        private IEnumerator AfterLookDown()
        {
            yield return PanCamera(_player.CameraTarget, 0.6f);
        }

        private IEnumerator PanCamera(Vector2 to, float speed)
        {
            Vector2 from = Level.Camera.Position;
            for (float p = 0f; p < 1f; p += Engine.DeltaTime * speed)
            {
                Level.Camera.Position = from + (to - from) * Ease.QuadInOut(p);
                yield return null;
            }
        }

        public override void OnEnd(Level level)
        {
            Level.Camera.Position = _player.CameraTarget;
            _player.StateMachine.State = 0;
        }
    }
}
