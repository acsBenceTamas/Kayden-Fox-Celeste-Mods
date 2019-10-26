using Celeste;
using FactoryHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryHelper.Cutscenes
{
    class CS01_FactoryHelper_MachineHeart : CutsceneEntity
    {
        private readonly Player _player;
        private readonly MachineHeart _machineHeart;

        public CS01_FactoryHelper_MachineHeart(Player player, MachineHeart machineHeart)
        {
            _player = player;
            _machineHeart = machineHeart;
        }

        public override void OnBegin(Level level)
        {
            Add(new Coroutine(Cutscene(level)));
        }

        private IEnumerator Cutscene(Level level)
        {
            _player.StateMachine.State = 11;
            yield return 0.5f;
            Vector2 panTo = (_machineHeart.Position - _player.Position) / 2 + _player.Position + Vector2.UnitY * 10;
            yield return ZoomAndPan(panTo, 2f, 0.7f, true);
            yield return 0.5f;
            yield return Textbox.Say("KaydenFox_FactoryMod_1_Factory_A_MachineHeart");
            yield return 0.2f;
            yield return ZoomAndPan(_player.CameraTarget, 1f, 1f);
            yield return 0.2f;
            EndCutscene(level);
        }

        private IEnumerator ZoomAndPan(Vector2 panTo, float zoomTo, float speed, bool centered = false)
        {
            Vector2 panFrom = Level.Camera.Position;
            if (centered)
            {
                panTo -= 1 / zoomTo * new Vector2(320, 180) / 2;
            }
            float zoomFrom = Level.Camera.Zoom;
            for (float p = 0f; p < 1f; p += Engine.DeltaTime * speed)
            {
                Level.Camera.Position = panFrom + (panTo - panFrom) * Ease.CubeInOut(p);
                Level.Camera.Zoom = zoomFrom + (zoomTo - zoomFrom) * Ease.CubeInOut(p);
                yield return null;
            }
        }

        public override void OnEnd(Level level)
        {
            _player.StateMachine.State = 0;
            level.Camera.Position = _player.CameraTarget;
            Level.Camera.Zoom = 1f;
        }
    }
}
