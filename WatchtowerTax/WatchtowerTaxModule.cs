using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace WatchtowerTax
{

    public class WatchtowerTaxModule : EverestModule
    {
        public static WatchtowerTaxModule Instance;
        public override Type SettingsType => typeof( WatchtowerTaxSettings );
        public static WatchtowerTaxSettings Settings => (WatchtowerTaxSettings)Instance._Settings;

        internal bool doTaxWipe = false;

        public WatchtowerTaxModule()
        {
            Instance = this;
        }

        public override void Load()
        {
            On.Celeste.Lookout.LookRoutine += OnLookoutLookRoutine;
            On.Celeste.AreaData.DoScreenWipe += OnLevelDoScreenWipe;
        }

        public override void Unload()
        {
            On.Celeste.Lookout.LookRoutine -= OnLookoutLookRoutine;
            On.Celeste.AreaData.DoScreenWipe -= OnLevelDoScreenWipe;
        }

        private IEnumerator OnLookoutLookRoutine( On.Celeste.Lookout.orig_LookRoutine orig, Celeste.Lookout self, Celeste.Player player )
        {
            IEnumerator enumerator = orig( self, player );
            while ( enumerator.MoveNext() )
            {
                yield return enumerator.Current;
            }
            if ( Settings.Enabled && player != null )
            {
                doTaxWipe = true;
                player.Die( -Vector2.UnitY );
            }
        }

        private void OnLevelDoScreenWipe( On.Celeste.AreaData.orig_DoScreenWipe orig, Celeste.AreaData self, Scene scene, bool wipeIn, Action onComplete )
        {
            if ( doTaxWipe )
            {
                new TaxWipe( scene, wipeIn, onComplete );
            }
            else
            {
                orig( self, scene, wipeIn, onComplete );
            }
        }

    }
}
