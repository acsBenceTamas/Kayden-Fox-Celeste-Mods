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
        Player _player;

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
            while (!_player.OnGround())
            {
                yield return null;
            }
            yield return 1.0f;
            _player.Facing = Facings.Left;
            yield return 0.5f;
            Add(new Coroutine(WaklLeftLookUp()));
            yield return 0.5f;
            yield return PanCamera(level.Bounds.Top, 0.25f);
            yield return 0.5f;
            yield return PanCamera(_player.CameraTarget.Y, 0.6f);
            EndCutscene(level);
        }

        private IEnumerator WaklLeftLookUp()
        {
            float target = _player.X - 64f;
            Coroutine walk;
            Add(walk = new Coroutine(_player.DummyWalkTo(target, speedMultiplier: 1.2f)));
            while (_player.Center.X > target + 3f)
            {
                Console.WriteLine($"Target: {target} | Player: {_player.Center.X}");
                yield return null;
            }
            _player.StateMachine.State = 11;
            _player.StateMachine.Locked = true;
            _player.DummyAutoAnimate = false;
            _player.Sprite.Play("lookUp");
            Remove(walk);
        }

        private IEnumerator PanCamera(float to, float speed)
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
            _player.StateMachine.Locked = false;
            _player.StateMachine.State = 0;
            _player.DummyAutoAnimate = true;
            _player.Speed = Vector2.Zero;
            _player.Facing = Facings.Left;
            level.Camera.Position = _player.CameraTarget;
        }
    }
}
