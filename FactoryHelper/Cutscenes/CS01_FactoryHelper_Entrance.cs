using Celeste;
using Monocle;
using System;
using System.Collections;

namespace FactoryHelper.Cutscenes
{
    class CS01_FactoryHelper_Entrance : CutsceneEntity
    {
        private readonly Player _player;

        public CS01_FactoryHelper_Entrance(Player player)
        {
            _player = player;
        }

        public override void OnBegin(Level level)
        {
            level.Remove(level.Tracker.GetEntity<MiniTextbox>());
            Add(new Coroutine(Cutscene(level)));
        }

        private IEnumerator Cutscene(Level level)
        {
            _player.StateMachine.State = 11;
            yield return 0.5f;
            yield return Textbox.Say("KaydenFox_FactoryMod_1_Factory_A_Entrance", WalkLeft, TurnBack);
            OnEnd(level);
        }

        private IEnumerator WalkLeft()
        {
            _player.Facing = Facings.Left;
            yield return 1.5f;
            Add(new Coroutine(_player.DummyWalkTo(_player.X - 16, false, 0.8f)));
            yield return 2.0f;
        }

        private IEnumerator TurnBack()
        {
            _player.Facing = Facings.Right;
            yield return 1.0f;
        }

        public override void OnEnd(Level level)
        {
            _player.StateMachine.State = 0;
        }
    }
}
