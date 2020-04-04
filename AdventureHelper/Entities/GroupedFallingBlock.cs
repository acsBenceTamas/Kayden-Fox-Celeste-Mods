using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.AdventureHelper.Entities
{
	[Tracked(false)]
	[CustomEntity( "AdventureHelper/GroupedFallingBlock")]
    class GroupedFallingBlock : Solid
	{
		public static ParticleType P_FallDustA = FallingBlock.P_FallDustA;
		public static ParticleType P_FallDustB = FallingBlock.P_FallDustB;
		public static ParticleType P_LandDust = FallingBlock.P_LandDust;

		private TileGrid _tiles;
		private readonly char _tileType;
		private GroupedFallingBlock _master;
		private bool _awake;
		private TileGrid _highlight;
		private bool _climbFall;

		public List<GroupedFallingBlock> Group;
		public List<JumpThru> Jumpthrus;
		public Point GroupBoundsMin;
		public Point GroupBoundsMax;
		public bool Triggered;
		public float FallDelay;

		public bool HasStartedFalling
		{
			get;
			private set;
		}
		public bool HasGroup
		{
			get;
			private set;
		}
		public bool MasterOfGroup
		{
			get;
			private set;
		}
		public Vector2 GroupPosition => new Vector2( GroupBoundsMin.X, GroupBoundsMin.Y );

		public GroupedFallingBlock( Vector2 position, float width, float height, char tileType, bool climbFall)
		: base( position, width, height, safe: true )
		{
			this._climbFall = climbFall;
			this._tileType = tileType;
			Depth = -9000;
			Add( new LightOcclude() );
			SurfaceSoundIndex = SurfaceIndex.TileToIndex[ tileType ];
			Add( new TileInterceptor( _tiles, highPriority: false ) );
		}

		public GroupedFallingBlock( EntityData data, Vector2 offset )
			: this( data.Position + offset, data.Width, data.Height, data.Char( "tiletype", '3' ), data.Bool( "climbFall", defaultValue: true ) )
		{
		}
		public override void Awake( Scene scene )
		{
			base.Awake( scene );
			_awake = true;
			if ( !HasGroup )
			{
				MasterOfGroup = true;
				Group = new List<GroupedFallingBlock>();
				Jumpthrus = new List<JumpThru>();
				GroupBoundsMin = new Point( (int)base.X, (int)base.Y );
				GroupBoundsMax = new Point( (int)base.Right, (int)base.Bottom );
				AddToGroupAndFindChildren( this );
				Rectangle rectangle = new Rectangle( GroupBoundsMin.X / 8, GroupBoundsMin.Y / 8, ( GroupBoundsMax.X - GroupBoundsMin.X ) / 8 + 1, ( GroupBoundsMax.Y - GroupBoundsMin.Y ) / 8 + 1 );
				VirtualMap<char> virtualMap = new VirtualMap<char>( rectangle.Width, rectangle.Height, '0' );
				foreach ( GroupedFallingBlock item in Group )
				{
					int num = (int)( item.X / 8f ) - rectangle.X;
					int num2 = (int)( item.Y / 8f ) - rectangle.Y;
					int num3 = (int)( item.Width / 8f );
					int num4 = (int)( item.Height / 8f );
					for ( int i = num; i < num + num3; i++ )
					{
						for ( int j = num2; j < num2 + num4; j++ )
						{
							virtualMap[ i, j ] = _tileType;
						}
					}
				}
				_tiles = GFX.FGAutotiler.GenerateMap( virtualMap, new Autotiler.Behaviour
				{
					EdgesExtend = false,
					EdgesIgnoreOutOfLevel = false,
					PaddingIgnoreOutOfLevel = false
				} ).TileGrid;
				_tiles.Position = new Vector2( (float)GroupBoundsMin.X - base.X, (float)GroupBoundsMin.Y - base.Y );
				Add( _tiles );
			}
			if (MasterOfGroup)
			{
				Add( new Coroutine( Sequence() ) );
				Group.Sort( ( block, otherBlock ) => { return otherBlock.Bottom.CompareTo( block.Bottom ); } );
			}
		}

		public void Trigger()
		{
			if ( MasterOfGroup )
			{
				Triggered = true;
			}
			else
			{
				_master.Triggered = true;
			}
		}

		private void AddToGroupAndFindChildren( GroupedFallingBlock from )
		{
			if ( from.X < (float)GroupBoundsMin.X )
			{
				GroupBoundsMin.X = (int)from.X;
			}
			if ( from.Y < (float)GroupBoundsMin.Y )
			{
				GroupBoundsMin.Y = (int)from.Y;
			}
			if ( from.Right > (float)GroupBoundsMax.X )
			{
				GroupBoundsMax.X = (int)from.Right;
			}
			if ( from.Bottom > (float)GroupBoundsMax.Y )
			{
				GroupBoundsMax.Y = (int)from.Bottom;
			}
			from.HasGroup = true;
			Group.Add( from );
			if ( from != this )
			{
				from._master = this;
			}
			foreach ( JumpThru item in Scene.CollideAll<JumpThru>( new Rectangle( (int)from.X - 1, (int)from.Y, (int)from.Width + 2, (int)from.Height ) ) )
			{
				if ( !Jumpthrus.Contains( item ) )
				{
					AddJumpThru( item );
				}
			}
			foreach ( JumpThru item2 in Scene.CollideAll<JumpThru>( new Rectangle( (int)from.X, (int)from.Y - 1, (int)from.Width, (int)from.Height + 2 ) ) )
			{
				if ( !Jumpthrus.Contains( item2 ) )
				{
					AddJumpThru( item2 );
				}
			}
			foreach ( GroupedFallingBlock entity in Scene.Tracker.GetEntities<GroupedFallingBlock>() )
			{
				if ( !entity.HasGroup && entity._tileType == _tileType && ( base.Scene.CollideCheck( new Rectangle( (int)from.X - 1, (int)from.Y, (int)from.Width + 2, (int)from.Height ), entity ) || base.Scene.CollideCheck( new Rectangle( (int)from.X, (int)from.Y - 1, (int)from.Width, (int)from.Height + 2 ), entity ) ) )
				{
					AddToGroupAndFindChildren( entity );
				}
			}
		}

		private void AddJumpThru( JumpThru jp )
		{
			Jumpthrus.Add( jp );
			foreach ( GroupedFallingBlock entity in Scene.Tracker.GetEntities<GroupedFallingBlock>() )
			{
				if ( !entity.HasGroup && entity._tileType == _tileType && Scene.CollideCheck( new Rectangle( (int)jp.X - 1, (int)jp.Y, (int)jp.Width + 2, (int)jp.Height ), entity ) )
				{
					AddToGroupAndFindChildren( entity );
				}
			}
		}

		public override void OnStaticMoverTrigger( StaticMover sm )
		{
			if (MasterOfGroup)
			{
				Triggered = true;
			}
			else
			{
				_master.Triggered = true;
			}
		}

		public override void OnShake( Vector2 amount )
		{
			if ( MasterOfGroup )
			{
				base.OnShake( amount );
				_tiles.Position += amount;
				foreach ( JumpThru jumpthru in Jumpthrus )
				{
					foreach ( Component component in jumpthru.Components )
					{
						Image image = component as Image;
						if ( image != null )
						{
							image.Position += amount;
						}
					}
				}
			}
		}

		private IEnumerator Sequence()
		{
			while ( !Triggered && !PlayerFallCheck() )
			{
				yield return null;
			}
			while ( FallDelay > 0f )
			{
				FallDelay -= Engine.DeltaTime;
				yield return null;
			}
			HasStartedFalling = true;
			while ( true )
			{
				ShakeSfx();
				StartShaking();
				Input.Rumble( RumbleStrength.Medium, RumbleLength.Medium );
				yield return 0.2f;
				float timer = 0.4f;
				while ( timer > 0f && PlayerWaitCheck() )
				{
					yield return null;
					timer -= Engine.DeltaTime;
				}
				StopShaking();
				foreach ( GroupedFallingBlock block in Group )
				{
					for ( int i = 2; i < Width; i += 4 )
					{
						if ( block.CollideCheck<Solid>( block.TopLeft + new Vector2( i, -2f ) ) )
						{
							SceneAs<Level>().Particles.Emit( P_FallDustA, 2, new Vector2( block.X + i, block.Y ), Vector2.One * 4f, (float)Math.PI / 2f );
						}
						SceneAs<Level>().Particles.Emit( P_FallDustB, 2, new Vector2( block.X + i, block.Y ), Vector2.One * 4f );
					}
				}
				float speed = 0f;
				float maxSpeed = 160f;
				while ( true )
				{
					Level level = SceneAs<Level>();
					speed = Calc.Approach( speed, maxSpeed, 500f * Engine.DeltaTime );

					bool breakCollideCheck = false;
					List<GroupedFallingBlock> hitters = new List<GroupedFallingBlock>();
					foreach ( GroupedFallingBlock block in Group )
					{
						if ( block.MoveVCollideSolids( speed * Engine.DeltaTime, thruDashBlocks: true ) )
						{
							breakCollideCheck = true;
							hitters.Add( block );
						}
					}
					foreach ( JumpThru jp in Jumpthrus )
					{
						jp.MoveV( speed * Engine.DeltaTime );
					}
					if ( breakCollideCheck )
					{
						foreach ( GroupedFallingBlock block in Group )
						{
							if ( !hitters.Contains( block ) )
							{
								block.MoveV( -speed * Engine.DeltaTime );
							}
						}
						foreach ( JumpThru jp in Jumpthrus )
						{
							jp.MoveV( -speed * Engine.DeltaTime );
						}
						break;
					}
					if ( GroupBoundsMin.Y > (float)( level.Bounds.Bottom + 16 ) || ( GroupBoundsMin.Y > (float)( level.Bounds.Bottom - 1 ) && CollideCheck<Solid>( GroupPosition + new Vector2( 0f, 1f ) ) ) )
					{
						Collidable = ( Visible = false );
						yield return 0.2f;
						if ( level.Session.MapData.CanTransitionTo( level, new Vector2( Center.X, Bottom + 12f ) ) )
						{
							yield return 0.2f;
							SceneAs<Level>().Shake();
							Input.Rumble( RumbleStrength.Strong, RumbleLength.Medium );
						}
						foreach ( GroupedFallingBlock block in Group )
						{
							block.RemoveSelf();
							block.DestroyStaticMovers();
						}
						foreach ( JumpThru jp in Jumpthrus )
						{
							jp.RemoveSelf();
							jp.DestroyStaticMovers();
						}
						yield break;
					}
					yield return null;
				}
				ImpactSfx();
				Input.Rumble( RumbleStrength.Strong, RumbleLength.Medium );
				SceneAs<Level>().DirectionalShake( Vector2.UnitY,  0.3f );
				StartShaking();
				LandParticles();
				yield return 0.2f;
				StopShaking();

				bool collideSolidTiles = false;
				foreach ( GroupedFallingBlock block in Group )
				{
					if ( block.CollideCheck<SolidTiles>( block.Position + new Vector2( 0f, 1f ) ) )
					{
						collideSolidTiles = true;
						break;
					}
				}
				if ( collideSolidTiles ) break;

				bool collidePlatforms;
				do
				{
					collidePlatforms = false;
					foreach ( GroupedFallingBlock block in Group )
					{
						foreach ( Platform platform	in Scene.Tracker.GetEntities<Platform>() )
						{
							if ( platform is GroupedFallingBlock && Group.Contains(platform as GroupedFallingBlock ) )
							{
								continue;
							}
							if ( block.CollideCheck( platform, block.Position + new Vector2( 0f, 1f ) ) )
							{
								collidePlatforms = true;
								break;
							}
						}
						if ( collidePlatforms ) break;
					}
					if ( collidePlatforms ) yield return 0.1f;
				}
				while ( collidePlatforms );
			}
			Safe = true;
		}

		private bool PlayerFallCheck()
		{
			foreach ( GroupedFallingBlock block in Group )
			{
				if ( block._climbFall )
				{
					if ( block.HasPlayerRider() )
					{
						return true;
					}
				}
				else
				{
					if ( block.HasPlayerOnTop() )
					{
						return true;
					}
				}
			}
			foreach ( JumpThru jp in Jumpthrus )
			{
				if ( jp.HasPlayerRider() )
				{
					return true;
				}
			}
			return false;
		}
		private bool PlayerWaitCheck()
		{
			if ( Triggered )
			{
				return true;
			}
			if ( PlayerFallCheck() )
			{
				return true;
			}
			if ( _climbFall )
			{
				foreach ( GroupedFallingBlock block in Group )
				{
					if ( !block.CollideCheck<Player>( Position - Vector2.UnitX ) )
					{
						if ( CollideCheck<Player>( Position + Vector2.UnitX ) )
						{
							return true;
						}
					}
					else
					{
						return true;
					}
				}
			}
			return false;
		}

		private void LandParticles()
		{
			foreach ( GroupedFallingBlock block in Group )
			{
				for ( int i = 2; i <= block.Width; i += 4 )
				{
					foreach ( Solid solid in Scene.Tracker.GetEntities<Solid>() )
					{
						if ( solid is GroupedFallingBlock ) continue;
						if ( block.CollideCheck( solid, block.Position + new Vector2( i, 3f ) ) )
						{
							SceneAs<Level>().ParticlesFG.Emit( P_FallDustA, 1, new Vector2( block.X + i, block.Bottom ), Vector2.One * 4f, (float)-Math.PI / 2f );
							float direction = ( !( i < block.Width / 2f ) ) ? 0f : ( (float)Math.PI );
							SceneAs<Level>().ParticlesFG.Emit( P_LandDust, 1, new Vector2( block.X + i, block.Bottom ), Vector2.One * 4f, direction );
						}
					}
				}
			}
		}

		private void ShakeSfx()
		{
			Vector2 center = GetGroupCenter();
			if ( _tileType == '3' )
			{
				Audio.Play( "event:/game/01_forsaken_city/fallblock_ice_shake", center );
			}
			else if ( _tileType == '9' )
			{
				Audio.Play( "event:/game/03_resort/fallblock_wood_shake", center );
			}
			else if ( _tileType == 'g' )
			{
				Audio.Play( "event:/game/06_reflection/fallblock_boss_shake", center );
			}
			else
			{
				Audio.Play( "event:/game/general/fallblock_shake", center );
			}
		}

		private void ImpactSfx()
		{
			Vector2 bottomCenter = GetGroupBottomCenter();
			if ( _tileType == '3' )
			{
				Audio.Play( "event:/game/01_forsaken_city/fallblock_ice_impact", bottomCenter );
			}
			else if ( _tileType == '9' )
			{
				Audio.Play( "event:/game/03_resort/fallblock_wood_impact", bottomCenter );
			}
			else if ( _tileType == 'g' )
			{
				Audio.Play( "event:/game/06_reflection/fallblock_boss_impact", bottomCenter );
			}
			else
			{
				Audio.Play( "event:/game/general/fallblock_impact", bottomCenter );
			}
		}

		private Vector2 GetGroupCenter()
		{
			float area = 0;
			float sumX = 0;
			float sumY = 0;
			foreach ( GroupedFallingBlock block in Group )
			{
				float blockArea = block.Width * block.Height;
				float distX = block.CenterX - CenterX;
				float distY = block.CenterY - CenterY;
				area += blockArea;
				sumX += distX * blockArea;
				sumY += distY * blockArea;
			}
			return Center + new Vector2( sumX / area, sumY / area );
		}

		private Vector2 GetGroupBottomCenter()
		{
			float area = 0;
			float sumX = 0;
			float maxY = float.MinValue;
			foreach ( GroupedFallingBlock block in Group )
			{
				float blockArea = block.Width * block.Height;
				float distX = block.CenterX - CenterX;
				area += blockArea;
				sumX += distX * blockArea;
				if ( block.Bottom > maxY )
				{
					maxY = block.Bottom;
				}
			}
			return new Vector2( CenterX + sumX / area, maxY );
		}
	}
}
