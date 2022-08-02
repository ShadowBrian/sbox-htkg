using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace htkgttt
{
	public partial class MazeMapCarriable : BaseCarriable
	{

		WallState[,] maze;

		int width, height;

		float scale;

		public ModelEntity Compass;

		[Net]Entity MapParentEnt {  get; set; }

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "models/items/map/map.vmdl" );

			if ( MazeCreator.Instance != null )
			{
				width = MazeCreator.Instance.width;
				height = MazeCreator.Instance.height;

				maze = MazeCreator.Instance.maze;

				scale = MazeCreator.Instance.scale * 0.0075f * 0.7f;
				MapParentEnt = new Entity();
				Generate();
			}
		}

		public override void ActiveEnd( Entity ent, bool dropped )
		{
			//
			// If we're just holstering, then hide us
			//
			if ( !dropped )
			{
				EnableDrawing = false;
				foreach ( var item in MapParentEnt.Children )
				{
					item.EnableDrawing = false;
				}
			}
		}

		public override void ActiveStart( Entity ent )
		{
			EnableDrawing = true;
			foreach ( var item in MapParentEnt.Children )
			{
				item.EnableDrawing = true;
			}

			if ( ent is Player player )
			{
				var animator = player.GetActiveAnimator();
				if ( animator != null )
				{
					SimulateAnimator( animator );
				}
			}

			//
			// If we're the local player (clientside) create viewmodel
			// and any HUD elements that this weapon wants
			//
			if ( IsLocalPawn )
			{
				DestroyViewModel();
				DestroyHudElements();

				CreateViewModel();
				CreateHudElements();
			}

				
		}

		public override void SimulateAnimator( PawnAnimator anim )
		{
			anim.SetAnimParameter( "holdtype", 4 );
			anim.SetAnimParameter( "aim_body_weight", 1.0f );
			anim.SetAnimParameter( "holdtype_handedness", 2 );
			anim.SetAnimParameter( "holdtype_pose_hand", 0.06f );
		}

		public override void SimulateAnimator( CitizenAnimationHelper anim )
		{
			anim.HoldType = CitizenAnimationHelper.HoldTypes.HoldItem;
			anim.Handedness = CitizenAnimationHelper.Hand.Left;
			anim.AimBodyWeight = 1.0f;
		}

		/*[Event.Tick]
		public void DebugTick()
		{
			if ( MapParentEnt != null && IsServer)
			{
				DebugOverlay.Line( MapParentEnt.Position, MapParentEnt.Position + Vector3.Up );
				DebugOverlay.Box( MapParentEnt, Color.Red );
			}
		}*/

		public void Generate()
		{
			bool PickedStart = false;
			bool PickedEnd = false;

			bool ThisIsEnd = false;
			bool ThisIsStart = false;

			Rand.SetSeed( System.DateTime.Now.Month );

			string wallmodel = "models/walls/map_wall.vmdl";

			Vector3 totalPositions = new Vector3();

			float cellnums = 0f;

			List<Entity> spawnedWalls = new List<Entity>();


			for ( int i = 0; i < width; ++i )
			{
				for ( int j = 0; j < height; ++j )
				{
					var cell = maze[i, j];
					var mapRotation = Rotation.Identity; //GetAttachment( "mapSpot" ).Value.Rotation;
					var position = new Vector3( -width / 2f + (float)i * scale, (-height / 2f + (float)j * scale), 0f );//GetAttachment("mapSpot").Value.Position + mapRotation*
					totalPositions += position;
					cellnums++;
					if ( i == 0 )
					{
						if ( !PickedEnd && j == width - 1 )
						{
							PickedEnd = true;
							ThisIsEnd = true;
						}
					}

					if ( cell.HasFlag( WallState.UP ) ) 
					{


						if ( !ThisIsEnd )
						{
							DebugOverlay.Line( position + (mapRotation * new Vector3( 0.5f, 0.5f, 0 ) * scale), position + (mapRotation * new Vector3( -0.5f, 0.5f, 0 ) * scale) );

							ModelEntity wall = new ModelEntity( wallmodel );

							wall.SetBodyGroup( 0, Rand.Int( 0, 3 ) );

							wall.Position = position + mapRotation * (new Vector3( 0, 0.5f, 0 ) * scale);
							wall.Scale = scale / 5f;

							//wall.SetParent( MapParentEnt );

							//wall.LocalRotation = Rotation.From( new Angles( -90, 0, 0 ) );
							wall.Rotation = Rotation.From( new Angles( 0, -90, 0 ) );

							spawnedWalls.Add( wall );
						}
						else
						{
							ThisIsEnd = false;
						}

					}

					if ( cell.HasFlag( WallState.LEFT ) )
					{
						DebugOverlay.Line( position + (mapRotation * new Vector3( -0.5f, 0.5f, 0 ) * scale), position + (mapRotation * new Vector3( -0.5f, -0.5f, 0 ) * scale) );

						ModelEntity wall = new ModelEntity( wallmodel );
						wall.SetBodyGroup( 0, Rand.Int( 0, 3 ) );

						wall.Position = position + mapRotation * (new Vector3( -0.5f, 0, 0 ) * scale);
						wall.Scale = scale / 5f;
						//wall.SetParent( MapParentEnt );
						spawnedWalls.Add( wall );
						//wall.LocalRotation = Rotation.From( new Angles( 0, 0, 90 ) );// * Rotation.From( new Angles( -90, 0, 0 ) );
					}

					if ( i == width - 1 )
					{

						if ( cell.HasFlag( WallState.RIGHT ) )
						{

							DebugOverlay.Line( position + (mapRotation * new Vector3( 0.5f, 0.5f, 0 )) * scale, position + (mapRotation * new Vector3( 0.5f, -0.5f, 0 ) * scale) );

							ModelEntity wall = new ModelEntity( wallmodel );

							wall.SetBodyGroup( 0, Rand.Int( 0, 3 ) );

							wall.Position = position + mapRotation * (new Vector3( 0.5f, 0, 0 ) * scale);
							wall.Scale = scale / 5f;
							//wall.SetParent( MapParentEnt );
							spawnedWalls.Add( wall );
							//wall.LocalRotation = Rotation.From( new Angles( 0, 0, -90 ) );// * Rotation.From( new Angles( -90, 0, 0 ) );
						}
					}

					if ( j == 0 )
					{

						if ( !PickedStart && i == width - 1 )
						{
							PickedStart = true;
							ThisIsStart = true;
						}

						if ( cell.HasFlag( WallState.DOWN ) )
						{
							if ( !ThisIsStart )
							{
								DebugOverlay.Line( position + (mapRotation * new Vector3( 0.5f, -0.5f, 0 ) * scale), position + (mapRotation * new Vector3( -0.5f, -0.5f, 0 ) * scale) );

								ModelEntity wall = new ModelEntity( wallmodel );

								wall.SetBodyGroup( 0, Rand.Int( 0, 3 ) );

								wall.Position = position + mapRotation * (new Vector3( 0, -0.5f, 0 ) * scale);
								wall.Scale = scale / 5f;


								//wall.SetParent( MapParentEnt );
								spawnedWalls.Add( wall );
								wall.Rotation = Rotation.From( new Angles( 0, 90, 0 ) );
								//wall.LocalRotation = Rotation.From( new Angles( -90, 0, 0 ) );
							}
							else
							{
								ThisIsStart = false;
							}
						}
					}
				}
			}

			totalPositions /= cellnums;

			MapParentEnt.Position = totalPositions;

			foreach ( var item in spawnedWalls )
			{
				item.SetParent( MapParentEnt );
			}

			MapParentEnt.SetParent( this );
			MapParentEnt.Position = GetAttachment( "mapSpot" ).Value.Position;
			MapParentEnt.Rotation = GetAttachment( "mapSpot" ).Value.Rotation;

			MapParentEnt.Position = this.WorldSpaceBounds.Center + MapParentEnt.Rotation.Up*0.75f - MapParentEnt.Rotation.Forward*0.5f + MapParentEnt.Rotation.Right * 0.5f;
		}

	}
}
