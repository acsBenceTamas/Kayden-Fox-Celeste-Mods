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
    class CS01_FactoryHelper_Ending : CutsceneEntity
    {
        private readonly Player _player;
        private Coroutine _running;
        private SteamWall _steamWall;

        public CS01_FactoryHelper_Ending(Player player)
        {
            _player = player;
        }

        public override void OnBegin(Level level)
        {
            level.Add(_steamWall = new SteamWall(level.Camera.Left - level.Bounds.Left));
            _steamWall.Speed = 64f;
            Add(new Coroutine(Cutscene(level)));
            Add(new Coroutine(FadeOutSteamWall()));
        }

        private IEnumerator FadeOutSteamWall()
        {
            while (_steamWall.Fade > 0f)
            {
                _steamWall.Fade -= Engine.DeltaTime / 6;
                _steamWall.Speed -= Engine.DeltaTime * 2f;
                yield return null;
            }
            Scene.Remove(_steamWall);
        }

        private IEnumerator Cutscene(Level level)
        {
            _player.StateMachine.State = 11;
            _player.StateMachine.Locked = true;
            _player.Drop();
            yield return null;
            Add(_running = new Coroutine(_player.DummyRunTo(level.Bounds.Left + 500, true)));
            yield return null;
            _player.DummyAutoAnimate = false;
            yield return AwaitStop(level);
            _player.StateMachine.State = 11;
            _player.DummyAutoAnimate = false;
            _player.Sprite.Rate = 1.2f;
            _player.Sprite.Play("sitDown");
            yield return 2f;
            yield return Textbox.Say("KaydenFox_FactoryMod_1_Factory_A_Ending", StandUpWalkLeft, WaitABit, WalkRightEnding);
            EndCutscene(level);
        }

        private IEnumerator StandUpWalkLeft()
        {
            _player.DummyAutoAnimate = true;
            yield return 0.5f;
            _player.Facing = Facings.Left;
            yield return 0.5f;
            Add(new Coroutine(_player.DummyWalkTo(_player.Position.X - 32f, speedMultiplier: 0.5f)));
            yield return null;
            while(_player.Speed.X < 0)
            {
                yield return null;
            }
        }

        private IEnumerator WaitABit()
        {
            yield return 1.5f;
        }

        private IEnumerator WalkRightEnding()
        {
            Add(new Coroutine(_player.DummyWalkTo(_player.Position.X + 128f, speedMultiplier: 0.5f)));
            yield return 0.5f;
            BadelineDummy badeline;
            Level.Add(badeline = new BadelineDummy(_player.Position - new Vector2(250f, 24f)));
            badeline.FloatSpeed = 20f;
            yield return badeline.FloatTo(badeline.Position + Vector2.UnitX * 48f);
            EndCutscene(Level);
        }

        private IEnumerator AwaitStop(Level level)
        {
            while(_player.X < level.Bounds.Left + 500 - 2)
            {
                yield return null;
            }
            Remove(_running);
            yield return null;
            PlayerJump(1);
            yield return null;
            while(!_player.OnGround())
            {
                yield return null;
            }
            _player.DummyFriction = true;
            yield return null;
        }

        private void PlayerJump(int direction)
        {
            _player.Facing = (Facings)direction;
            _player.DummyFriction = false;
            _player.DummyAutoAnimate = true;
            _player.Speed.X = direction * 120;
            _player.Jump();
            _player.AutoJump = true;
            _player.AutoJumpTimer = 2f;
        }

        public override void Update()
        {
            base.Update();
            Level.Camera.Position = _player.CameraTarget;
        }

        public override void OnEnd(Level level)
        {
            level.CompleteArea(true, false, false);
        }
    }
}
