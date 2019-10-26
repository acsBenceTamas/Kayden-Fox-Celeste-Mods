using Celeste;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using Microsoft.Xna.Framework;

namespace FactoryHelper.Cutscenes
{
    class CS01_FactoryHelper_BreakFirstFuse : CutsceneEntity
    {
        private readonly Player _player;

        public CS01_FactoryHelper_BreakFirstFuse (Player player)
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
            yield return 1.0f;
            _player.Facing = Facings.Left;
            yield return 0.5f;
            Add(new Coroutine(WaklLeftLookUp()));
            yield return 0.5f;
            yield return PanCameraY(level.Bounds.Top, 0.25f);
            yield return 0.5f;
            _player.DummyAutoAnimate = true;
            yield return PanCameraY(_player.CameraTarget.Y, 0.6f);
            yield return null;
            EndCutscene(level);
        }

        private IEnumerator WaklLeftLookUp()
        {
            float target = _player.X - 64f;
            Coroutine walk;
            _player.StateMachine.State = 0;
            Add(walk = new Coroutine(_player.DummyWalkTo(target, speedMultiplier: 1.2f)));
            while (_player.Center.X > target + 3f)
            {
                yield return null;
            }
            Remove(walk);
            yield return null;
            _player.DummyAutoAnimate = false;
            _player.Sprite.Play("lookUp");
        }

        private IEnumerator PanCameraY(float to, float speed)
        {
            float from = Level.Camera.Y;
            for (float p = 0f; p < 1f; p += Engine.DeltaTime * speed)
            {
                Level.Camera.Y = from + (to - from) * Ease.CubeInOut(p);
                yield return null;
            }
        }

        public override void OnEnd(Level level)
        {
            _player.StateMachine.State = 0;
            _player.DummyAutoAnimate = true;
            _player.Speed = Vector2.Zero;
            _player.Facing = Facings.Left;
            level.Camera.Position = _player.CameraTarget;
        }
    }
}
