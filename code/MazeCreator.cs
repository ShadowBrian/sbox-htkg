using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using SandboxEditor;

namespace htkgttt
{
	[Library( "ent_htk_mazegenerator" )]
	[HammerEntity]
	public class MazeCreator : Entity
	{
		[Property, ResourceType( "htkgwall" )]
		public string PossibleWalls { get; set; } = "";

		public static MazeCreator Instance;

		public WallState[,] maze;

		[Property( "Maze Complexity", "How many cells in X and Y should the maze have?" )]
		int Complexity { get; set; } = 10;

		public int width = 10;
		public int height = 10;

		[Property( "Maze Size", "Size of the maze" )]
		public float scale { get; set; } = 1024f;

		public List<Entity> GeneratedItems { get; set; } = new List<Entity>();

		[ConCmd.Admin( "hgtk_regenerate" )]
		public static void RegenerateMaze()
		{
			MazeCreator.Instance.Regenerate();
		}

		public List<string> WallPieces { get; set; } = new List<string>();
		public List<string> PassableWallPieces { get; set; } = new List<string>();
		public List<string> FloorPieces { get; set; } = new List<string>();

		public override void Spawn()
		{
			Instance = this;
			width = Complexity;
			height = width;

			scale /= Complexity;

			// Store a reference to the food resource.
			MazeWallResource wallResource;
			var getSuccess = ResourceLibrary.TryGet( PossibleWalls, out wallResource );

			if ( getSuccess )
			{
				WallPieces = wallResource.Walls;
				PassableWallPieces = wallResource.PassableWalls;
				FloorPieces = wallResource.FloorModels;
			}

			GenerateMaze();
		}

		public void Regenerate()
		{
			foreach ( Entity entity in GeneratedItems )
			{
				entity.Delete();
			}

			GeneratedItems.Clear();

			Log.Trace( "Regenerating Maze..." );

			GenerateMaze();

			Log.Trace( "Maze Regenerated!" );
		}

		public Vector3 StartPos = Vector3.Zero;

		public void GenerateMaze()
		{
			maze = MazeGenerator.Generate( width, height );

			bool PickedStart = false;
			bool PickedEnd = false;

			bool ThisIsEnd = false;
			bool ThisIsStart = false;

			Rand.SetSeed( (int)MathF.Floor( System.DateTime.Now.DayOfYear / 7f ) + System.DateTime.Now.Year + 2 );

			if ( PossibleWalls != "" )

				for ( int i = 0; i < width; ++i )
				{
					for ( int j = 0; j < height; ++j )
					{
						var cell = maze[i, j];
						var position = Position + new Vector3( -width / 2f + i * scale, -height / 2f + j * scale, 0f ) - new Vector3( (scale * (Complexity - 1.1f)) / 2f, (scale * (Complexity - 1.1f)) / 2f, 0f );

						AnimatedEntity passableWall = new AnimatedEntity();

						passableWall.SetModel( Rand.FromList<string>( PassableWallPieces, "models/walls/basic_template/basic_template_passable.vmdl" ) );
						passableWall.SetupPhysicsFromModel( PhysicsMotionType.Static );
						passableWall.Position = position;
						passableWall.Scale = scale / 5f;

						if ( passableWall.GetAttachment( "blade" ).HasValue )
						{
							passableWall.Components.Add( new BladeComponent() );
						}

						ModelEntity floorModel = new ModelEntity();

						floorModel.SetModel( Rand.FromList<string>( FloorPieces, "models/walls/basic_template/basic_template_floor.vmdl" ) );
						floorModel.SetupPhysicsFromModel( PhysicsMotionType.Static );
						floorModel.Position = position;
						floorModel.Rotation = Rotation.From( new Angles( 0, MathF.Round( (Rand.Float() * 360) / 90 ) * 90f, 0 ) );
						floorModel.Scale = scale / 5f;
						GeneratedItems.Add( floorModel );

						if ( floorModel.GetModelName().Contains( "left" ) || floorModel.GetModelName().Contains( "right" ) )
						{
							KillTrigger trig = new KillTrigger();
							trig.Position = floorModel.Position;
							trig.Rotation = floorModel.Rotation;
							trig.Scale = floorModel.Scale;
							GeneratedItems.Add( trig );
						}

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
								string RandomWall = Rand.FromList<string>( WallPieces, "models/walls/basic_template/basic_template.vmdl" );

								bool HasLights = RandomWall.Contains( "lit" );

								DebugOverlay.Line( position + (new Vector3( 0.5f, 0.5f, 0 ) * scale), position + (new Vector3( -0.5f, 0.5f, 0 ) * scale) );

								ModelEntity wall = new ModelEntity( RandomWall );
								wall.SetupPhysicsFromModel( PhysicsMotionType.Static );
								GeneratedItems.Add( wall );

								wall.Position = position + (new Vector3( 0, 0.5f, 0 ) * scale);
								wall.Scale = scale / 5f;

								wall.Rotation = Rotation.From( new Angles( 0, -90, 0 ) );

								if ( HasLights )
								{
									PointLightEntity light = new PointLightEntity();
									light.Position = wall.GetAttachment( "flame" ).Value.Position;
									light.Components.Create<LightCullComponent>();

									GeneratedItems.Add( light );

									light = new PointLightEntity();
									light.Position = wall.GetAttachment( "flame_copy" ).Value.Position;
									light.Components.Create<LightCullComponent>();

									GeneratedItems.Add( light );
								}
							}
							else
							{
								ThisIsEnd = false;
							}

						}

						if ( cell.HasFlag( WallState.LEFT ) )
						{
							string RandomWall = Rand.FromList<string>( WallPieces, "models/walls/basic_template/basic_template.vmdl" );

							bool HasLights = RandomWall.Contains( "lit" );

							DebugOverlay.Line( position + (new Vector3( -0.5f, 0.5f, 0 ) * scale), position + (new Vector3( -0.5f, -0.5f, 0 ) * scale) );

							ModelEntity wall = new ModelEntity( RandomWall );
							wall.SetupPhysicsFromModel( PhysicsMotionType.Static );
							GeneratedItems.Add( wall );

							wall.Position = position + (new Vector3( -0.5f, 0, 0 ) * scale);
							wall.Scale = scale / 5f;

							if ( HasLights )
							{
								PointLightEntity light = new PointLightEntity();
								light.Position = wall.GetAttachment( "flame" ).Value.Position;
								light.Components.Create<LightCullComponent>();

								GeneratedItems.Add( light );

								light = new PointLightEntity();
								light.Position = wall.GetAttachment( "flame_copy" ).Value.Position;
								light.Components.Create<LightCullComponent>();

								GeneratedItems.Add( light );
							}
						}

						if ( i == width - 1 )
						{

							if ( cell.HasFlag( WallState.RIGHT ) )
							{
								string RandomWall = Rand.FromList<string>( WallPieces, "models/walls/basic_template/basic_template.vmdl" );

								bool HasLights = RandomWall.Contains( "lit" );

								DebugOverlay.Line( position + (new Vector3( 0.5f, 0.5f, 0 )) * scale, position + (new Vector3( 0.5f, -0.5f, 0 ) * scale) );

								ModelEntity wall = new ModelEntity( RandomWall );
								wall.SetupPhysicsFromModel( PhysicsMotionType.Static );
								GeneratedItems.Add( wall );

								wall.Position = position + (new Vector3( 0.5f, 0, 0 ) * scale);
								wall.Scale = scale / 5f;


								if ( HasLights )
								{
									PointLightEntity light = new PointLightEntity();
									light.Position = wall.GetAttachment( "flame" ).Value.Position;
									light.Components.Create<LightCullComponent>();

									GeneratedItems.Add( light );

									light = new PointLightEntity();
									light.Position = wall.GetAttachment( "flame_copy" ).Value.Position;
									light.Components.Create<LightCullComponent>();

									GeneratedItems.Add( light );
								}
							}
						}

						if ( j == 0 )
						{

							if ( !PickedStart && i == width - 1 )
							{
								PickedStart = true;
								StartPos = position;
								ThisIsStart = true;
							}

							if ( cell.HasFlag( WallState.DOWN ) )
							{
								if ( !ThisIsStart )
								{
									string RandomWall = Rand.FromList<string>( WallPieces, "models/walls/basic_template/basic_template.vmdl" );

									bool HasLights = RandomWall.Contains( "lit" );

									DebugOverlay.Line( position + (new Vector3( 0.5f, -0.5f, 0 ) * scale), position + (new Vector3( -0.5f, -0.5f, 0 ) * scale) );

									ModelEntity wall = new ModelEntity( RandomWall );
									wall.SetupPhysicsFromModel( PhysicsMotionType.Static );
									GeneratedItems.Add( wall );

									wall.Position = position + (new Vector3( 0, -0.5f, 0 ) * scale);
									wall.Scale = scale / 5f;

									wall.Rotation = Rotation.From( new Angles( 0, 90, 0 ) );

									if ( HasLights )
									{
										PointLightEntity light = new PointLightEntity();
										light.Position = wall.GetAttachment( "flame" ).Value.Position;
										light.Components.Create<LightCullComponent>();

										GeneratedItems.Add( light );

										light = new PointLightEntity();
										light.Position = wall.GetAttachment( "flame_copy" ).Value.Position;
										light.Components.Create<LightCullComponent>();

										GeneratedItems.Add( light );
									}
								}
								else
								{
									ThisIsStart = false;
								}
							}
						}

						if ( cell.HasFlag( WallState.LEFT ) || cell.HasFlag( WallState.RIGHT ) )
						{
							passableWall.Rotation = Rotation.From( new Angles( 0, -90, 0 ) );

						}

						if ( cell.HasFlag( WallState.UP ) || cell.HasFlag( WallState.DOWN ) )
						{
							//floorModel.Rotation = Rotation.From( new Angles( 0, -90, 0 ) );
						}

						floorModel.Rotation = passableWall.Rotation;

						if ( (cell.HasFlag( WallState.UP ) && cell.HasFlag( WallState.RIGHT )) || (cell.HasFlag( WallState.DOWN ) && cell.HasFlag( WallState.RIGHT )) )
						{
							/*passableWall.Rotation = Rotation.From( new Angles( 0, -45, 0 ) );
							passableWall.Scale = (scale / 5f) * 1.25f;*/

							floorModel.SetModel( FloorPieces.FirstOrDefault( "models/walls/basic_template/basic_template_floor.vmdl" ) );

							passableWall.Delete();

						}

						if ( (cell.HasFlag( WallState.DOWN ) && cell.HasFlag( WallState.LEFT )) || (cell.HasFlag( WallState.UP ) && cell.HasFlag( WallState.LEFT )) )
						{
							/*passableWall.Rotation = Rotation.From( new Angles( 0, 45, 0 ) );
							passableWall.Scale = (scale / 5f) * 1.25f;*/

							floorModel.SetModel( FloorPieces.FirstOrDefault( "models/walls/basic_template/basic_template_floor.vmdl" ) );

							passableWall.Delete();
						}

						if ( cell.ToString().Split( ',' ).Length == 2 )
						{
							floorModel.SetModel( FloorPieces.FirstOrDefault( "models/walls/basic_template/basic_template_floor.vmdl" ) );
						}

						if ( cell.ToString().Split( ',' ).Length == 2 || Rand.Float() > 0.25f )
						{
							passableWall.Delete();
						}
						else
						{
							GeneratedItems.Add( passableWall );
						}
					}
				}

			/*if ( All.OfType<Pawn>().FirstOrDefault() != null )
			{
				foreach ( var pawn in All.OfType<Pawn>() )
				{
					pawn.Position = StartPos;
				}
			}*/

		}



	}
}
