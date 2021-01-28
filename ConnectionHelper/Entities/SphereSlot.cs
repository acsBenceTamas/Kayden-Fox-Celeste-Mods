using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace ConnectionHelper.Entities
{
    [CustomEntity( "ConnectionHelper/SphereSlot" )]
    public class SphereSlot : Entity
    {
        public bool Finished => switchComponent.Finished;
        protected Switch switchComponent;
        protected bool turnOnSequenceActive = false;

        public SphereSlot( EntityData data, Vector2 offset ) : base( data.Position + offset )
        {
            Add( switchComponent = new Switch( false ) );
            Collider = new Circle( 8 );
        }

        public override void Update()
        {
            base.Update();

            if ( !switchComponent.Activated )
            {
                foreach ( CompanionSphere.Companion companion in Scene.Tracker.GetEntities<CompanionSphere.Companion>() )
                {
                    if ( !companion.FollowingPlayer && CollideCheck( companion ) )
                    {
                        TryTurnOn( companion );
                    }
                }
            }
        }

        public override void Render()
        {
            base.Render();

            Draw.Circle( Position, 8, switchComponent.Activated ? Color.DarkGreen : Color.OrangeRed, 8 );
        }

        public void Deactivate()
        {
            turnOnSequenceActive = false;
            switchComponent.Deactivate();
        }

        public bool Activate()
        {
            return switchComponent.Activate();
        }

        private void TryTurnOn( CompanionSphere.Companion companion )
        {
            if ( !turnOnSequenceActive )
            {
                Add( new Coroutine( TurnOnSequence( companion ) ) );
            }
        }

        private IEnumerator TurnOnSequence( CompanionSphere.Companion companion )
        {
            turnOnSequenceActive = true;
            companion.RunUseCoroutine( Center );
            while ( !companion.Activated && companion.Activating )
            {
                yield return null;
            }
            if ( !switchComponent.Activated && companion.Activated )
            {
                companion.Slot = this;
                Add( new SoundSource( "event:/game/general/touchswitch_any" ) );
                if ( Activate() )
                {
                    SoundEmitter.Play( "event:/game/general/touchswitch_last_oneshot" );
                    Add( new SoundSource( "event:/game/general/touchswitch_last_cutoff" ) );
                }
            }
            turnOnSequenceActive = false;
        }
    }
}
