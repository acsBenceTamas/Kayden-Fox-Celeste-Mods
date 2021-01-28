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

        protected static Random rng = new Random();

        protected const float connectionLineSpeed = 2.0f;
        protected const int connectionLineSegments = 4;
        protected const int connectionLineSubSegments = 10;
        protected const float connectionLineAmplitude = 3.0f;

        public static Companion ActiveCompanion = null;

        protected Companion[] companions;
        protected HashSet<Companion> activeCompanions = new HashSet<Companion>();
        protected float connectionLineOffset = 0;
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
            Depth = Depths.FGTerrain - 10;
        }

        public override void Awake( Scene scene )
        {
            base.Awake( scene );
            for ( int i = 0; i < companions.Length; i++ )
            {
                Scene.Add( companions[ i ] );
            }
            ActiveCompanion = null;
        }

        public override void Render()
        {
            base.Render();

            if ( !AllCompanionsActive ) return;

            const int subsegmentCount = connectionLineSegments * connectionLineSubSegments;

            for ( int i = 0; i < companions.Length - 1; i++ )
            {
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
                    Draw.Point( pos, bothActive ? Color.Green : requiresDash ? Color.HotPink : Color.LightSteelBlue );
                }
            }
        }

        public override void Update()
        {
            base.Update();

            if ( CollideCheckConnections() )
            {
                if ( ActiveCompanion != null )
                {
                    ActiveCompanion.DetachFromPlayer();
                }
                ResetCompanions();
            }

            connectionLineOffset += Engine.DeltaTime * connectionLineSpeed;

            if ( connectionLineOffset > Math.PI * 2.0f )
            {
                connectionLineOffset -= (float)Math.PI * 2.0f;
            }
        }

        private void ResetCompanions()
        {
            activeCompanions.Clear();
            for ( int i = 0; i < companions.Length; i++ )
            {
                companions[ i ].Reset();
            }
        }

        private bool CollideCheckConnections()
        {
            if ( !AllCompanionsActive ) return false;

            for ( int i = 0; i < companions.Length - 1; i++ )
            {
                foreach ( CrystalStaticSpinner spinner in Scene.Tracker.GetEntities<CrystalStaticSpinner>() )
                {
                    if ( spinner.Collidable && Collide.CheckLine( spinner, companions[ i ].Position, companions[ i + 1 ].Position ) )
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void TryAttachToPlayer( Player player, Companion companion )
        {
            if ( ActiveCompanion == null )
            {
                Audio.Play( "event:/game/general/seed_touch", Position, "count", 1 );
                companion.AttachToPlayer( player );
            }
        }

        private void ResetDone( Companion companion )
        {
            activeCompanions.Add( companion );
        }

        [Tracked(false)]
        public class Companion : Entity
        {
            protected const float distanceModifier = 3.5f;
            protected const float freeHaltDistance = 12.0f;
            protected const float redashDelay = 1.0f;

            public SphereSlot Slot = null;
            public CompanionSphere ParentCollection;
            public bool Activated = false;
            public bool Activating = false;
            public bool FollowingPlayer => player != null;
            protected Wiggler wiggler;
            protected SineWave sine;
            protected Player player;
            protected Vector2 velocity;
            protected float redashTimer = 0.0f;

            protected Vector2 lastPlayerPosition;
            protected bool reachedLastPosition = true;
            protected bool requireDash;
            protected Tween tween;
            protected Coroutine useCoroutine;
            protected readonly Vector2 defaultPosition;

            public Companion( Vector2 position, CompanionSphere parent, bool requireDash ) : base( position )
            {
                this.requireDash = requireDash;
                defaultPosition = position;
                Collider = new Hitbox( 16, 16, -8, -8 );
                ParentCollection = parent;
                Add( sine = new SineWave( 0.6f, 0f ) );
                sine.Randomize();

                Add( new PlayerCollider( OnPlayer, new Circle(16) ) );
            }

            public override void Render()
            {
                base.Render();
                float deltaY = sine.Value * 1f;
                Draw.Circle( Position + Vector2.UnitY * deltaY, 8, requireDash ? Color.Pink : Color.LightBlue, 8 );
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

                if ( redashTimer > 0.0f )
                {
                    redashTimer -= Engine.DeltaTime;
                }

                if ( player != null && ConnectionHelperModule.Settings.ReleaseCompanionButton.Pressed )
                {
                    DetachFromPlayer();
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
                ActiveCompanion = this;
                reachedLastPosition = false;
            }

            public void DetachFromPlayer()
            {
                player = null;
                ActiveCompanion = null;
                redashTimer = redashDelay;
            }

            public void Reset()
            {
                if ( useCoroutine != null && useCoroutine.Active )
                {
                    useCoroutine.Cancel();
                }
                Add( new Coroutine( ResetCoroutine() ) );
            }

            private IEnumerator ResetCoroutine()
            {
                Level level = Scene as Level;
                redashTimer = 0.0f;
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
                    level.ParticlesFG.Emit( requireDash ? P_Burst : P_BurstNoDash, 1, Position + Calc.AngleToVector( dir, 4f ), Vector2.Zero, dir );
                }
                yield return rng.NextFloat() * 0.2f + 0.3f;
                Position = defaultPosition;
                Audio.Play( "event:/game/general/seed_reappear", Position, "count", 1 );
                Visible = true;
                Collidable = true;
                level.Displacement.AddBurst( Position, 0.2f, 8f, 28f, 0.2f );
                ParentCollection.ResetDone( this );
            }

            public IEnumerator UseRoutine( Vector2 target )
            {
                //wiggler.Start();
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
            }

            private void OnPlayer( Player player )
            {
                if ( !Activating && ( ( requireDash && player.DashAttacking ) || ( !requireDash && ConnectionHelperModule.Settings.ReleaseCompanionButton.Pressed) ) 
                    && ParentCollection.AllCompanionsActive && !(Slot != null && Slot.Finished ) )
                {
                    TryAttachToPlayer( player );
                }
            }

            private void TryAttachToPlayer( Player player )
            {
                if ( redashTimer <= 0.0f )
                {
                    ParentCollection.TryAttachToPlayer( player, this );

                    if ( Slot != null && Slot.Active )
                    {
                        Slot.Deactivate();
                        Slot = null;
                    }
                }
            }

            public void RunUseCoroutine( Vector2 target )
            {
                Add( useCoroutine = new Coroutine( UseRoutine( target ) ) );
            }
        }
    }
}
