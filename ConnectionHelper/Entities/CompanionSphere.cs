using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace ConnectionHelper.Entities
{

    [CustomEntity("ConnectionHelper/CompanionSphere")]
    public class CompanionSphere : Entity
    {
        public static ParticleType P_Burst;
        public static ParticleType P_BurstNoDash;
        public static ParticleType P_Idle;
        public static ParticleType P_IdleNoDash;
        public static ParticleType P_Fire;
        public static ParticleType P_FireNoDash;

        protected static Random rng = new Random();

        protected const float connectionLineSpeed = 2.0f;
        protected const int connectionLineSegments = 4;
        protected const int connectionLineSubSegments = 10;
        protected const float connectionLineAmplitude = 3.0f;
        protected const float killCircleExpansionTime = 0.5f;

        public static Companion ActiveCompanion = null;

        protected CompanionSphereManager manager;

        protected Entity killCircle;
        protected Companion[] nearestCompanions = new Companion[ 2 ];

        protected Companion[] companions;
        protected HashSet<Companion> activeCompanions = new HashSet<Companion>();
        protected HashSet<Companion> doneResetting = new HashSet<Companion>();
        protected HashSet<Companion> manualResets = new HashSet<Companion>();
        protected float connectionLineOffset = 0;
        protected float killCircleExpansionSpeed = 64.0f;
        protected bool requiresDash;

        public bool AllCompanionsActive => activeCompanions.Count == companions.Length;

        public CompanionSphere( EntityData data, Vector2 offset )
        {
            requiresDash = data.Bool( "requiresDash", true );
            Vector2[] nodes = data.NodesWithPosition( offset );
            companions = new Companion[ nodes.Length ];
            for ( int i = 0; i < nodes.Length; i++ )
            {
                companions[ i ] = new Companion( nodes[ i ], this, requiresDash );
                activeCompanions.Add( companions[ i ] );
            }
            Depth = Depths.Below + 1;
        }

        public override void Awake( Scene scene )
        {
            base.Awake( scene );
            for ( int i = 0; i < companions.Length; i++ )
            {
                Scene.Add( companions[ i ] );
            }
            ActiveCompanion = null;

            manager = Scene.Tracker.GetEntity<CompanionSphereManager>();

            if ( manager == null )
            {
                Scene.Add( manager = new CompanionSphereManager() );
                manager.AddTag( Tags.Persistent );
            }
        }

        public override void Render()
        {
            base.Render();

            const int subsegmentCount = connectionLineSegments * connectionLineSubSegments;

            for ( int i = 0; i < companions.Length - 1; i++ )
            {
                if ( activeCompanions.Count == 0 || !activeCompanions.Contains( companions[ i ] ) || !activeCompanions.Contains( companions[ i + 1 ] ) )
                { 
                    continue; 
                }

                Vector2 direction = companions[ i + 1 ].Position - companions[ i ].Position;
                bool bothActive = companions[ i + 1 ].Activated && companions[ i ].Activated;
                float lineLength = direction.Length();
                direction.Normalize();
                Vector2 perpendicular = direction.Rotate( (float)( Math.PI / 2 ) );

                for ( int j = 1; j < subsegmentCount; j++ )
                {
                    float amplitude = (float)Math.Sin( connectionLineOffset + 2.0f * Math.PI / connectionLineSubSegments * ( j % connectionLineSubSegments ) ) * connectionLineAmplitude;
                    float longitude = lineLength / subsegmentCount * j;

                    Vector2 pos = companions[ i ].Position + direction * longitude + perpendicular * amplitude;

                    if ( killCircle != null && companions[ i ] == nearestCompanions[0] && companions[ i + 1 ] == nearestCompanions[ 1 ] && Collide.CheckPoint( killCircle, pos ) ) continue;

                    Draw.Point( pos, bothActive ? Color.Green : requiresDash ? Color.HotPink : Color.LightSteelBlue );
                }
            }
        }

        public override void Update()
        {
            base.Update();

            CrystalStaticSpinner murderSpinner = null;

            if ( killCircle == null && CollideCheckConnections( ref murderSpinner ) )
            {
                if ( ActiveCompanion != null )
                {
                    ActiveCompanion.DetachFromPlayer();
                }
                StartKillCircle( murderSpinner );
            }

            connectionLineOffset += Engine.DeltaTime * connectionLineSpeed;

            if ( connectionLineOffset > Math.PI * 2.0f )
            {
                connectionLineOffset -= (float)Math.PI * 2.0f;
            }

            if ( killCircle != null )
            {
                if ( Scene.OnInterval(0.05f ) )
                {
                    foreach ( Companion companion in nearestCompanions )
                    {
                        if ( manualResets.Contains( companion ) ) continue;

                        float dir = Calc.Random.NextFloat( (float)Math.PI * 2f );
                        Vector2 towardsCompanion = ( companion.Position - killCircle.Position ).SafeNormalize() * ( killCircle.Collider as Circle ).Radius;
                        SceneAs<Level>().ParticlesFG.Emit( requiresDash ? P_Burst : P_BurstNoDash, 1, killCircle.Position + towardsCompanion, Vector2.Zero, dir );
                    }
                }

                (killCircle.Collider as Circle).Radius += killCircleExpansionSpeed * Engine.DeltaTime;
                foreach ( Companion companion in nearestCompanions )
                {
                    if ( !manualResets.Contains(companion) && Collide.Check( companion, killCircle ) )
                    {
                        companion.Reset( true );

                        if ( manualResets.Count == nearestCompanions.Length )
                        {
                            killCircle.RemoveSelf();
                            killCircle = null;
                            ResetCompanions();
                            break;
                        }
                    }
                }
            }
        }

        private void StartKillCircle( CrystalStaticSpinner murderSpinner )
        {
            killCircleExpansionSpeed = Math.Max( ( murderSpinner.Position - nearestCompanions[ 0 ].Position ).LengthSquared(), (murderSpinner.Position - nearestCompanions[ 1 ].Position ).LengthSquared() );
            killCircleExpansionSpeed = (float)Math.Sqrt( killCircleExpansionSpeed ) / killCircleExpansionTime;

            Audio.Play( "event:/game/06_reflection/fall_spike_smash", murderSpinner.Position );

            for ( int i = 0; i < 6; i++ )
            {
                float dir = Calc.Random.NextFloat( (float)Math.PI * 2f );
                SceneAs<Level>().ParticlesFG.Emit( requiresDash ? P_Burst : P_BurstNoDash, 1, murderSpinner.Position, Vector2.Zero, dir );
            }
            Scene.Add( killCircle = new Entity( murderSpinner.Position ) { Collider = new Circle( 1.0f ) } );
        }

        private void ResetCompanions()
        {
            activeCompanions.Clear();
            for ( int i = 0; i < companions.Length; i++ )
            {
                if ( !manualResets.Contains( companions[ i ] ) )
                {
                    activeCompanions.Remove( companions[ i ] );
                    companions[ i ].Reset();
                }
            }
            manualResets.Clear();
        }

        private bool CollideCheckConnections( ref CrystalStaticSpinner killer )
        {
            if ( !AllCompanionsActive ) return false;

            for ( int i = 0; i < companions.Length - 1; i++ )
            {
                foreach ( CrystalStaticSpinner spinner in Scene.Tracker.GetEntities<CrystalStaticSpinner>() )
                {
                    if ( spinner.Collidable && Collide.CheckLine( spinner, companions[ i ].Position, companions[ i + 1 ].Position ) )
                    {
                        nearestCompanions[ 0 ] = companions[ i ];
                        nearestCompanions[ 1 ] = companions[ i + 1 ];
                        killer = spinner;
                        return true;
                    }
                }
            }
            killer = null;
            return false;
        }

        private void TryAttachToPlayer( Player player, Companion companion )
        {
            if ( ActiveCompanion == null )
            {
                Audio.Play( "event:/game/general/seed_touch", Position, "count", 1 );
                companion.AttachToPlayer( player );

                for ( int j = 0; j < 8; j++ )
                {
                    SceneAs<Level>().ParticlesFG.Emit( Key.P_Insert, companion.Center, (float)Math.PI / 8f * (float)j );
                }

                SceneAs<Level>().Displacement.AddBurst( companion.Position, 0.2f, 8f, 28f, 0.2f );
            }
        }

        private void ResetDone( Companion companion )
        {
            doneResetting.Add( companion );
            if ( doneResetting.Count == companions.Length )
            {
                foreach ( Companion inactiveCompanion in companions )
                {
                    activeCompanions.Add( inactiveCompanion );
                    doneResetting.Clear();
                }
            }
        }

        [Tracked(false)]
        public class Companion : Entity
        {
            protected const float distanceModifier = 3.5f;
            protected const float freeHaltDistance = 12.0f;

            public SphereSlot Slot = null;
            public CompanionSphere ParentCollection;
            public bool Activated = false;
            public bool Activating = false;
            public bool FollowingPlayer => player != null;
            protected Wiggler wiggler;
            protected SineWave sine;
            protected Player player;
            protected Vector2 velocity;

            protected Sprite sprite;
            protected Sprite shine;

            protected Vector2 lastPlayerPosition;
            protected bool reachedLastPosition = true;
            protected bool requiresDash;
            protected Tween tween;
            protected Coroutine useCoroutine;
            protected readonly Vector2 defaultPosition;

            public Companion( Vector2 position, CompanionSphere parent, bool requiresDash ) : base( position )
            {
                this.requiresDash = requiresDash;
                defaultPosition = position;
                Collider = new Hitbox( 16, 16, -8, -8 );
                ParentCollection = parent;
                Add( sine = new SineWave( 0.6f, 0f ) );
                sine.Randomize();

                Add( new PlayerCollider( OnPlayer, new Circle(16) ) );
                Depth = Depths.Below;

                Add( sprite = ConnectionHelperModule.SpriteBank.Create( "companion_sphere" ) );
                sprite.Play( requiresDash ? "idle" : "noDash", true, true );
                Add( shine = ConnectionHelperModule.SpriteBank.Create( "companion_sphere" ) );
                shine.OnFinish = delegate
                {
                    shine.Visible = false;
                };

                Add( new MirrorReflection() );
                Add( new VertexLight( requiresDash ? Color.Pink : Color.LightBlue, 1f, 32, 48 ) );
                Add( wiggler = Wiggler.Create( 0.4f, 4f, delegate ( float v )
                {
                    sprite.Scale = Vector2.One * ( 1f + v * 0.35f );
                } ) );
            }

            public override void Update()
            {
                base.Update();

                float haltDistance = 0.1f;

                if ( player != null )
                {
                    lastPlayerPosition = player.Center;
                    reachedLastPosition = false;
                    haltDistance = freeHaltDistance;
                }

                if ( !reachedLastPosition )
                {
                    ApproachPosition( lastPlayerPosition, haltDistance );
                }

                if ( player != null && ConnectionHelperModule.Settings.ReleaseCompanionButton.Pressed && ParentCollection.manager.CanInteract )
                {
                    DetachFromPlayer();
                }

                if ( Slot == null )
                {
                    float deltaY = sine.Value * 1f;
                    sprite.Position.Y = deltaY;
                    shine.Position.Y = deltaY;
                }

                if ( Slot == null )
                {
                    if ( Scene.OnInterval( 0.1f ) )
                    {
                        SceneAs<Level>().Particles.Emit( requiresDash ? P_Idle : P_IdleNoDash, 1, base.Center, Vector2.One * 8f );
                    }
                }
                else
                {
                    if ( Scene.OnInterval( 0.03f ) )
                    {
                        Vector2 position = Position + new Vector2( 0f, 1f ) + Calc.AngleToVector( Calc.Random.NextAngle(), 5f );
                        SceneAs<Level>().ParticlesBG.Emit( requiresDash ? P_Fire : P_FireNoDash, position );
                    }
                }

                if ( Scene.OnInterval( 2f ) && sprite.Visible )
                {
                    shine.Play( "flash", restart: true );
                    shine.Visible = true;
                }
            }

            private void ApproachPosition( Vector2 target, float haltDistance )
            {
                Vector2 dir = target - Position;
                float dist = dir.Length() - haltDistance;

                if ( dist > 0 )
                {
                    velocity = dir.SafeNormalize() * ( dist * distanceModifier ) * Engine.DeltaTime;
                }
                else
                {
                    velocity = Vector2.Zero;
                    reachedLastPosition = true;
                }

                Position += velocity;
            }

            public void AttachToPlayer( Player player )
            {
                this.player = player;
                Activated = false;
                ActiveCompanion = this;
                reachedLastPosition = false;
                ParentCollection.manager.Interacted();
                wiggler.Start();
                sprite.Play( requiresDash ? "idle" : "noDash", true );
            }

            public void DetachFromPlayer()
            {
                player = null;
                ActiveCompanion = null;
                ParentCollection.manager.Interacted();
            }

            public void Reset( bool manual = false )
            {
                if ( manual )
                {
                    ParentCollection.manualResets.Add( this );
                }

                if ( useCoroutine != null && useCoroutine.Active )
                {
                    useCoroutine.Cancel();
                }

                if ( tween != null )
                {
                    tween.Stop();
                    tween = null;
                }
                Add( new Coroutine( ResetCoroutine() ) );
            }

            private IEnumerator ResetCoroutine()
            {
                Level level = Scene as Level;
                reachedLastPosition = true;
                player = null;

                if ( Slot != null )
                {
                    Slot.Deactivate();
                    Slot = null;
                }

                if ( tween != null )
                {
                    tween.Stop();
                    tween = null;
                }

                Collidable = false;
                Activating = false;
                Activated = false;
                Audio.Play( "event:/game/general/seed_poof", Position );
                yield return rng.NextFloat() * 0.025f + 0.025f;
                Visible = false;
                Input.Rumble( RumbleStrength.Medium, RumbleLength.Medium );

                for ( int i = 0; i < 6; i++ )
                {
                    float dir = Calc.Random.NextFloat( (float)Math.PI * 2f );
                    level.ParticlesFG.Emit( requiresDash ? P_Burst : P_BurstNoDash, 1, Position + Calc.AngleToVector( dir, 4f ), Vector2.Zero, dir );
                }

                while ( ParentCollection.killCircle != null )
                {
                    yield return null;
                }

                yield return rng.NextFloat() * 0.2f + 0.3f;
                Position = defaultPosition;
                Audio.Play( "event:/game/general/seed_reappear", Position, "count", 1 );
                Visible = true;
                Collidable = true;
                level.Displacement.AddBurst( Position, 0.2f, 8f, 28f, 0.2f );
                sprite.Play( requiresDash ? "idle" : "noDash", true, true );
                ParentCollection.ResetDone( this );
            }

            public IEnumerator UseRoutine( Vector2 target )
            {
                reachedLastPosition = true;
                Activating = true;
                Vector2 from = Position;
                tween = Tween.Create( Tween.TweenMode.Oneshot, Ease.CubeOut, 1f, start: true );

                tween.OnUpdate = delegate ( Tween t )
                {
                    Position = from + t.Eased * ( target - from );
                };

                Add( tween );
                yield return tween.Wait();

                tween = null;
                Input.Rumble( RumbleStrength.Medium, RumbleLength.Medium );

                for ( int j = 0; j < 16; j++ )
                {
                    SceneAs<Level>().ParticlesFG.Emit( Key.P_Insert, Center, (float)Math.PI / 8f * (float)j );
                }

                Activated = true;
                Activating = false;
                sprite.Play( requiresDash ? "idleIdle" : "noDashIdle" );
                sprite.Position.Y = 0;
                shine.Position.Y = 0;
            }

            private void OnPlayer( Player player )
            {
                if ( !Activating && ( ( requiresDash && player.DashAttacking ) || ( !requiresDash && ConnectionHelperModule.Settings.ReleaseCompanionButton.Pressed) ) 
                    && ParentCollection.AllCompanionsActive && !( Slot != null && Slot.Finished ) && ParentCollection.manager.CanInteract )
                {
                    TryAttachToPlayer( player );
                }
            }

            private void TryAttachToPlayer( Player player )
            {
                ParentCollection.TryAttachToPlayer( player, this );

                if ( Slot != null && Slot.Active )
                {
                    Slot.Deactivate();
                    Slot = null;
                }
            }

            public void RunUseCoroutine( Vector2 target )
            {
                Add( useCoroutine = new Coroutine( UseRoutine( target ) ) );
            }
        }

        [Tracked( false )]
        public class CompanionSphereManager : Entity
        {
            public bool CanInteract => releaseTimer <= 0.0f;

            protected const float releaseDelay = 0.3f;

            protected float releaseTimer = 0.0f;

            public override void Update()
            {
                base.Update();

                if ( releaseTimer > 0.0f )
                {
                    releaseTimer -= Engine.DeltaTime;
                }
            }

            public void Interacted()
            {
                releaseTimer = releaseDelay;
            }
        }
    }
}
