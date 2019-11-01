using Celeste;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace FactoryHelper.Cutscenes
{
    class FactoryIntroVignette : Scene
    {

        private Session session;

        private string areaMusic;

        private float fade = 0f;

        private TextMenu menu;

        private float pauseFade = 0f;

        private HudRenderer hud;

        private bool exiting;

        private Coroutine textCoroutine;

        private float textAlpha = 0f;

        private Textbox textbox;

        private EventInstance ringtone;

        public bool CanPause => menu == null;

        public FactoryIntroVignette(Session session, HiresSnow snow = null)
        {
            this.session = session;
            areaMusic = session.Audio.Music.Event;
            session.Audio.Music.Event = null;
            session.Audio.Apply(forceSixteenthNoteHack: false);
            Add(hud = new HudRenderer());
            RendererList.UpdateLists();
            textbox = new Textbox("KaydenFox_FactoryMod_1_Factory_A_Intro");
            textCoroutine = new Coroutine(TextSequence());
        }

        private IEnumerator TextSequence()
        {
            yield return 1f;
            ringtone = Audio.Play("event:/game/02_old_site/sequence_phone_ring_loop");
            yield return 4f;
            ringtone.stop(STOP_MODE.ALLOWFADEOUT);
            Audio.Play("event:/game/02_old_site/sequence_phone_pickup");
            yield return 1f;
            yield return Say(textbox);
            yield return 0.5f;
            Audio.Play("event:/game/02_old_site/sequence_phone_pickup");
            yield return 1f;
            StartGame();
        }

        private IEnumerator Say(Textbox textbox)
        {
            Engine.Scene.Add(textbox);
            while (textbox.Opened)
            {
                yield return null;
            }
        }

        public override void Update()
        {
            if (menu == null)
            {
                base.Update();
                if (!exiting)
                {
                    textCoroutine.Update();
                    if (Input.Pause.Pressed || Input.ESC.Pressed)
                    {
                        OpenMenu();
                    }
                }
            }
            else if (!exiting)
            {
                menu.Update();
            }
            pauseFade = Calc.Approach(pauseFade, (menu != null) ? 1 : 0, Engine.DeltaTime * 8f);
            hud.BackgroundFade = Calc.Approach(hud.BackgroundFade, (menu != null) ? 0.6f : 0f, Engine.DeltaTime * 3f);
            fade = Calc.Approach(fade, 0f, Engine.DeltaTime);
        }

        public void OpenMenu()
        {
            PauseSfx();
            Audio.Play("event:/ui/game/pause");
            Add(menu = new TextMenu());
            menu.Add(new TextMenu.Button(Dialog.Clean("intro_vignette_resume")).Pressed(CloseMenu));
            menu.Add(new TextMenu.Button(Dialog.Clean("intro_vignette_skip")).Pressed(StartGame));
            menu.OnCancel = (menu.OnESC = (menu.OnPause = CloseMenu));
        }

        private void CloseMenu()
        {
            ResumeSfx();
            Audio.Play("event:/ui/game/unpause");
            if (menu != null)
            {
                menu.RemoveSelf();
            }
            menu = null;
        }

        private void StartGame()
        {
            StopSfx();
            textCoroutine = null;
            session.Audio.Music.Event = areaMusic;
            if (menu != null)
            {
                menu.RemoveSelf();
                menu = null;
            }
            FadeWipe fadeWipe = new FadeWipe(this, wipeIn: false, delegate
            {
                Engine.Scene = new LevelLoader(session);
            });
            fadeWipe.OnUpdate = delegate (float f)
            {
                textAlpha = Math.Min(textAlpha, 1f - f);
            };
            exiting = true;
        }

        public override void Render()
        {
            base.Render();
            if (fade > 0f || textAlpha > 0f)
            {
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, RasterizerState.CullNone, null, Engine.ScreenMatrix);
                if (fade > 0f)
                {
                    Draw.Rect(-1f, -1f, 1922f, 1082f, Color.Black * fade);
                }
                Draw.SpriteBatch.End();
            }
        }

        private void PauseSfx()
        {
            foreach(SoundSource sound in Tracker.GetComponents<SoundSource>())
            {
                sound.Pause();
            }
            ringtone?.setPaused(true);
        }

        private void ResumeSfx()
        {
            foreach (SoundSource sound in Tracker.GetComponents<SoundSource>())
            {
                sound.Resume();
            }
            ringtone?.setPaused(false);
        }

        private void StopSfx()
        {
            List<Component> components = new List<Component>();
            components.AddRange(Tracker.GetComponents<SoundSource>());
            foreach (SoundSource sound in components)
            {
                sound.RemoveSelf();
            }
            ringtone?.stop(STOP_MODE.IMMEDIATE);
        }
    }
}
