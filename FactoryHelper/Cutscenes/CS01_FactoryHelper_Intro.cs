using Celeste;
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
    class CS01_FactoryHelper_Intro : CutsceneEntity
    {
        private SoundSource ringtone;
        private Player player;

        public CS01_FactoryHelper_Intro()
        {
            Depth = Depths.Top*2;
        }

        public override void OnBegin(Level level)
        {
            Add(new Coroutine(Cutscene(level)));
        }

        private IEnumerator Cutscene(Level level)
        {
            player = level.Tracker.GetEntity<Player>();
            player.StateMachine.State = 11;
            player.StateMachine.Locked = true;
            Add(ringtone = new SoundSource());
            ringtone.Position = player.Position;
            ringtone.Play("event:/game/02_old_site/sequence_phone_ring_loop");
            yield return 4f;
            ringtone.Param("end", 1f);
            Audio.Play("event:/game/02_old_site/sequence_phone_pickup");
            yield return 1f;
            yield return Textbox.Say("KaydenFox_FactoryMod_1_Factory_A_Intro");
            yield return 0.5f;
            Audio.Play("event:/game/02_old_site/sequence_phone_pickup");
            yield return 1f;
            EndCutscene(level);
        }

        public override void OnEnd(Level level)
        {
            //level.OnEndOfFrame += delegate
            //{
                player.StateMachine.Locked = false;
                player.StateMachine.State = 0;
                level.UnloadLevel();
                level.CanRetry = true;
                level.Session.Level = "1-01-exterior";
                level.Session.RespawnPoint = level.GetSpawnPoint(new Vector2(level.Bounds.Left, level.Bounds.Top));
                level.LoadLevel(Player.IntroTypes.WalkInRight, true);
                Draw.Rect(color: Color.Black, x: level.Camera.X - 5f, y: level.Camera.Y - 5f, width: 370f, height: 190f);
            //};
        }

        public override void Render()
        {
            Level level = Scene as Level;
            Draw.Rect(color: Color.Black, x: level.Camera.X - 5f, y: level.Camera.Y - 5f, width: 370f, height: 190f);
            base.Render();
        }
    }
}
