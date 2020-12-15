using Celeste;
using Celeste.Mod.Entities;
using FactoryHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

namespace FactoryHelper.Entities
{
    [CustomEntity( "FactoryHelper/FactoryActivatorDashBlock" )]
    public class FactoryActivatorDashBlock : DashBlock
    {
        private readonly HashSet<string> _activationIds = new HashSet<string>();
        private readonly bool permanent;
        private EntityID id;

        public FactoryActivatorDashBlock( EntityData data, Vector2 offset ) 
        : this( data, offset, new EntityID( data.Level.Name, data.ID ) )
        {
        }

        public FactoryActivatorDashBlock( EntityData data, Vector2 offset, EntityID id )
        : base( data.Position + offset, data.Char( "tiletype", '3' ), data.Width, data.Height, data.Bool( "blendin" ), data.Bool( "permanent", defaultValue: true ), data.Bool( "canDash", defaultValue: true ), id )
        {
            string activationIds = data.Attr( "activationIds", "" );

            foreach ( string activationId in activationIds.Split( ',' ) )
            {
                if ( activationId != "" )
                {
                    _activationIds.Add( activationId );
                }
            }
            permanent = data.Bool( "permanent", true );
            this.id = id;
        }

        public override void Awake( Scene scene )
        {
            if ( FactoryHelperModule.Session.PermanentlyRemovedActivatorDashBlocks.Contains( id ) )
            {
                RemoveSelf();
            }
            else
            {
                base.Awake( scene );
            }
        }

        internal void OnBreak()
        {
            SendOutSignals();
            if ( permanent )
            {
                SetSessionTags();
            }
        }

        private void SetSessionTags()
        {
            foreach ( string activationId in _activationIds )
            {
                ActivatePermanently( activationId );
            }
            FactoryHelperModule.Session.PermanentlyRemovedActivatorDashBlocks.Add( id );
        }

        private void SendOutSignals()
        {
            foreach ( FactoryActivator activator in Scene.Tracker.GetComponents<FactoryActivator>() )
            {
                if ( _activationIds.Contains( activator.ActivationId ) )
                {
                    activator.Activate();
                }
            }
        }

        private void ActivatePermanently( string activationId )
        {
            ( Scene as Level ).Session.SetFlag( $"FactoryActivation:{activationId}", true );
        }
    }
}
