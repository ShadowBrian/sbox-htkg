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

		public ModelEntity Compass;

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "models/items/map/map.vmdl" );

			if ( MazeCreator.Instance != null )
			{
				width = MazeCreator.Instance.width;
				height = MazeCreator.Instance.height;

				maze = MazeCreator.Instance.maze;

				Generate();
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

		public void Generate()
		{
			bool PickedStart = false;
			bool PickedEnd = false;

			bool ThisIsEnd = false;
			bool ThisIsStart = false;

			Rand.SetSeed( System.DateTime.Now.Month );

			string wallmodel = "models/walls/map_wall.vmdl";

			float scale = 0.75f;

			for ( int i = 0; i < width; ++i )
			{
				for ( int j = 0; j < height; ++j )
				{
					var cell = maze[i, j];
					var mapRotation = GetAttachment( "mapSpot" ).Value.Rotation;
					var position = GetAttachment("mapSpot").Value.Position + mapRotation*new Vector3( -width / 2f + i * scale, -height / 2f + j * scale, 0f );

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
							DebugOverlay.Line( position + (mapRotation* new Vector3( 0.5f, 0.5f, 0 ) * scale), position + (mapRotation * new Vector3( -0.5f, 0.5f, 0 ) * scale) );

							ModelEntity wall = new ModelEntity( wallmodel );

							wall.SetBodyGroup( 0, Rand.Int( 0, 3 ) );

							wall.Position = position + mapRotation * (new Vector3( 0, 0.5f, 0 ) * scale);
							wall.Scale = scale / 5f;

							wall.SetParent( this );

							wall.LocalRotation = Rotation.From( new Angles( -90, 0, 0 ) );
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
						wall.SetParent( this );

						wall.LocalRotation = Rotation.From( new Angles( 0, 0, 90 ) ) * Rotation.From( new Angles( -90, 0, 0 ) );
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
							wall.SetParent( this );

							wall.LocalRotation = Rotation.From( new Angles(0, 0, -90 ) ) * Rotation.From( new Angles( -90, 0, 0 ) );
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

								
								wall.SetParent( this );

								wall.LocalRotation = Rotation.From( new Angles( -90, 0, 0 ) );
							}
							else
							{
								ThisIsStart = false;
							}
						}
					}
				}
			}
		}
	}
}
