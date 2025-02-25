using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using static Sandbox.Services.Inventory;

namespace htkgttt
{
	public partial class MazeCreator : Component
	{
		[Property]
		public MazeWallResource PossibleWalls { get; set; }

		[Property] public MapInstance SelectedMap { get; set; }

		public static MazeCreator Instance { get; set; }

		[Sync]
		public WallState[,] maze { get; set; }

		[Property]
		int Complexity { get; set; } = 10;

		public int width = 10;
		public int height = 10;

		[Property]
		public float scale { get; set; } = 1024f;

		public List<GameObject> GeneratedItems { get; set; } = new List<GameObject>();


		public static void RegenerateMaze()
		{
			MazeCreator.Instance.Regenerate();
		}

		public static string hgtk_maze_seed { get; set; } = "";

		public static bool hgtk_use_seed { get; set; } = false;

		public List<string> WallPieces { get; set; } = new List<string>();
		public List<string> PassableWallPieces { get; set; } = new List<string>();
		public List<string> FloorPieces { get; set; } = new List<string>();

		[Sync]
		public int SeedToUse { get; set; } = 0;

		[Property]
		public GameObject PlayerObject { get; set; }

		[Property]
		public GameObject KingObject { get; set; }


		[Property]
		public GameObject Spawnpoint { get; set; }

		GameObject MazeRoot { get; set; }

		protected override void OnAwake()
		{
			base.OnAwake();
			Instance = this;
		}

		protected override void OnStart()
		{
			Instance = this;
			width = Complexity;
			height = width;

			scale /= Complexity;

			if ( !Networking.IsHost )
			{
				return;
			}

			if ( SelectedMap.MapName.ToLower().Contains( "hedge" ) )
			{
				PossibleWalls = ResourceLibrary.Get<MazeWallResource>( "resources/hedgemaze.htkgwall" );
			}
			else
			{
				PossibleWalls = ResourceLibrary.Get<MazeWallResource>( "resources/base_template.htkgwall" );
				WorldPosition += Vector3.Up * 64f;
			}

			MazeWallResource wallResource = PossibleWalls;

			MazeRoot = Scene.CreateObject();
			MazeRoot.Name = "MAZE ROOT";
			MazeRoot.NetworkSpawn();

			WallPieces = wallResource.Walls;
			PassableWallPieces = wallResource.PassableWalls;
			FloorPieces = wallResource.FloorModels;


			if ( hgtk_use_seed )
			{
				string seed = "";

				hgtk_maze_seed = hgtk_maze_seed.Replace( " ", "" );//replace spaces

				foreach ( char item in hgtk_maze_seed )
				{
					seed += GetIndexInAlphabet( item ).ToString();//get each letter's index in alphabet
				}

				long seed2 = long.Parse( seed.Truncate( 14 ) );//truncate length to fit inside an int64

				seed2 = seed2 % int.MaxValue;//% it to fit within an int32

				Game.SetRandomSeed( (int)seed2 );//convert to int32

				Log.Trace( "Set seed to: " + seed2 + " translated from: " + hgtk_maze_seed );
				SeedToUse = (int)seed2;
			}
			else
			{
				SeedToUse = (int)MathF.Floor( float.Parse( (System.DateTime.Now.DayOfYear / 7f) + "" + (System.DateTime.Now.Year + 2) ) );
				Game.SetRandomSeed( SeedToUse );
			}

			GenerateMaze();

			if ( StartPos != Vector3.Zero )
			{
				StartPos = Spawnpoint.WorldPosition;

			}

			foreach ( var item in GeneratedItems )
			{
				item.SetParent( MazeRoot );
				item.NetworkSpawn();
				item.Network.SetOrphanedMode( NetworkOrphaned.Host );
			}
		}

		private static int GetIndexInAlphabet( char value )
		{
			// Uses the uppercase character unicode code point. 'A' = U+0042 = 65, 'Z' = U+005A = 90
			char upper = char.ToUpper( value );

			return (int)upper - (int)'A';
		}

		public void Regenerate()
		{
			foreach ( var gameObject in GeneratedItems )
			{
				gameObject.Destroy();
			}

			GeneratedItems.Clear();

			Log.Info( "Regenerating Maze..." );

			GenerateMaze();

			Log.Info( "Maze Regenerated!" );
		}

		public Vector3 StartPos = Vector3.Zero;

		public void GenerateMaze()
		{
			maze = MazeGenerator.Generate( width, height );

			bool PickedStart = false;
			bool PickedEnd = false;

			bool ThisIsEnd = false;
			bool ThisIsStart = false;


			if ( PossibleWalls.IsValid() )
			{

				for ( int i = 0; i < width; ++i )
				{
					for ( int j = 0; j < height; ++j )
					{
						var cell = maze[i, j];
						var position = WorldPosition + new Vector3( -width / 2f + i * scale, -height / 2f + j * scale, 0f ) - new Vector3( (scale * (Complexity - 1.1f)) / 2f, (scale * (Complexity - 1.1f)) / 2f, 0f );

						GameObject passableWall = Scene.CreateObject();
						passableWall.Name = "Passable Wall";

						passableWall.WorldPosition = position;
						passableWall.WorldScale = scale / 5f;

						var SelectedWall = Game.Random.FromList<string>( PassableWallPieces, "models/walls/basic_template/basic_template_passable.vmdl" );
						if ( SelectedWall.ToLower().Contains( "trap" ) )
						{
							if ( SelectedWall.ToLower().Contains( "ram" ) )
							{
								var trap = PrefabScene.GetPrefab( "prefabs/trap_ram.prefab" ).Clone();
								trap.SetParent( passableWall );
								trap.WorldPosition = position;
								trap.LocalScale = 1f;

							}
							else
							{
								var trap = PrefabScene.GetPrefab( "prefabs/trap_swinging_axe.prefab" ).Clone();
								trap.SetParent( passableWall );
								trap.WorldPosition = position;
								trap.LocalScale = 1f;

							}
						}
						else
						{
							passableWall.AddComponent<SkinnedModelRenderer>().Model = Model.Load( SelectedWall );
							passableWall.AddComponent<ModelCollider>();
						}

						GameObject floorModel = Scene.CreateObject();
						floorModel.Name = "Floor";
						floorModel.AddComponent<SkinnedModelRenderer>().Model = Model.Load( Game.Random.FromList<string>( FloorPieces, "models/walls/basic_template/basic_template_floor.vmdl" ) );
						floorModel.AddComponent<ModelCollider>();
						floorModel.WorldPosition = position;
						floorModel.WorldRotation = Rotation.From( new Angles( 0, MathF.Round( (Game.Random.Float() * 360) / 90 ) * 90f, 0 ) );
						floorModel.WorldScale = scale / 5f;
						GeneratedItems.Add( floorModel );

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
								string RandomWall = Game.Random.FromList<string>( WallPieces, "models/walls/basic_template/basic_template.vmdl" );

								bool HasLights = RandomWall.Contains( "lit" );

								DebugOverlay.Line( position + (new Vector3( 0.5f, 0.5f, 0 ) * scale), position + (new Vector3( -0.5f, 0.5f, 0 ) * scale) );

								GameObject wall = Scene.CreateObject();
								wall.Name = "Wall";
								wall.AddComponent<SkinnedModelRenderer>().Model = Model.Load( RandomWall );
								wall.AddComponent<ModelCollider>();//.SetupPhysicsFromModel( PhysicsMotionType.Static );
								GeneratedItems.Add( wall );

								wall.WorldPosition = position + (new Vector3( 0, 0.5f, 0 ) * scale);
								wall.WorldScale = scale / 5f;

								wall.WorldRotation = Rotation.From( new Angles( 0, -90, 0 ) );

								if ( HasLights )
								{
									SetupLightsForWall( wall );
								}
							}
							else
							{
								ThisIsEnd = false;
							}

						}

						if ( cell.HasFlag( WallState.LEFT ) )
						{
							string RandomWall = Game.Random.FromList<string>( WallPieces, "models/walls/basic_template/basic_template.vmdl" );

							bool HasLights = RandomWall.Contains( "lit" );

							DebugOverlay.Line( position + (new Vector3( -0.5f, 0.5f, 0 ) * scale), position + (new Vector3( -0.5f, -0.5f, 0 ) * scale) );

							GameObject wall = Scene.CreateObject();
							wall.Name = "Wall";
							wall.AddComponent<SkinnedModelRenderer>().Model = Model.Load( RandomWall );
							wall.AddComponent<ModelCollider>();
							GeneratedItems.Add( wall );

							wall.WorldPosition = position + (new Vector3( -0.5f, 0, 0 ) * scale);
							wall.WorldScale = scale / 5f;

							if ( HasLights )
							{
								SetupLightsForWall( wall );
							}
						}

						if ( i == width - 1 )
						{

							if ( cell.HasFlag( WallState.RIGHT ) )
							{
								string RandomWall = Game.Random.FromList<string>( WallPieces, "models/walls/basic_template/basic_template.vmdl" );

								bool HasLights = RandomWall.Contains( "lit" );

								DebugOverlay.Line( position + (new Vector3( 0.5f, 0.5f, 0 )) * scale, position + (new Vector3( 0.5f, -0.5f, 0 ) * scale) );

								GameObject wall = Scene.CreateObject();
								wall.Name = "Wall";
								wall.AddComponent<SkinnedModelRenderer>().Model = Model.Load( RandomWall );
								wall.AddComponent<ModelCollider>();
								GeneratedItems.Add( wall );

								wall.WorldPosition = position + (new Vector3( 0.5f, 0, 0 ) * scale);
								wall.WorldScale = scale / 5f;


								if ( HasLights )
								{
									SetupLightsForWall( wall );
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
									string RandomWall = Game.Random.FromList<string>( WallPieces, "models/walls/basic_template/basic_template.vmdl" );

									bool HasLights = RandomWall.Contains( "lit" );

									DebugOverlay.Line( position + (new Vector3( 0.5f, -0.5f, 0 ) * scale), position + (new Vector3( -0.5f, -0.5f, 0 ) * scale) );

									GameObject wall = Scene.CreateObject();
									wall.Name = "Wall";
									wall.AddComponent<SkinnedModelRenderer>().Model = Model.Load( RandomWall );
									wall.AddComponent<ModelCollider>();
									GeneratedItems.Add( wall );

									wall.WorldPosition = position + (new Vector3( 0, -0.5f, 0 ) * scale);
									wall.WorldScale = scale / 5f;

									wall.WorldRotation = Rotation.From( new Angles( 0, 90, 0 ) );

									if ( HasLights )
									{
										SetupLightsForWall( wall );

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
							passableWall.WorldRotation = Rotation.From( new Angles( 0, -90, 0 ) );

						}

						floorModel.WorldRotation = passableWall.WorldRotation;

						if ( (cell.HasFlag( WallState.UP ) && cell.HasFlag( WallState.RIGHT )) || (cell.HasFlag( WallState.DOWN ) && cell.HasFlag( WallState.RIGHT )) )
						{
							floorModel.GetComponent<ModelRenderer>().Model = Model.Load( FloorPieces.FirstOrDefault( "models/walls/basic_template/basic_template_floor.vmdl" ) );
							floorModel.GetComponent<ModelCollider>().Model = Model.Load( FloorPieces.FirstOrDefault( "models/walls/basic_template/basic_template_floor.vmdl" ) );
							passableWall.Destroy();

						}

						if ( (cell.HasFlag( WallState.DOWN ) && cell.HasFlag( WallState.LEFT )) || (cell.HasFlag( WallState.UP ) && cell.HasFlag( WallState.LEFT )) )
						{
							floorModel.GetComponent<ModelRenderer>().Model = Model.Load( FloorPieces.FirstOrDefault( "models/walls/basic_template/basic_template_floor.vmdl" ) );
							floorModel.GetComponent<ModelCollider>().Model = Model.Load( FloorPieces.FirstOrDefault( "models/walls/basic_template/basic_template_floor.vmdl" ) );
							passableWall.Destroy();
						}

						if ( cell.ToString().Split( ',' ).Length == 2 )
						{
							floorModel.GetComponent<ModelRenderer>().Model = Model.Load( FloorPieces.FirstOrDefault( "models/walls/basic_template/basic_template_floor.vmdl" ) );
							floorModel.GetComponent<ModelCollider>().Model = Model.Load( FloorPieces.FirstOrDefault( "models/walls/basic_template/basic_template_floor.vmdl" ) );
						}

						if ( cell.ToString().Split( ',' ).Length == 2 || Game.Random.Float() > 0.25f )
						{
							passableWall.Destroy();
						}
						else
						{
							GeneratedItems.Add( passableWall );
						}
					}
				}

			}

		}
		public async void SetupLightsForWall( GameObject wall )
		{
			await Task.DelaySeconds( 0.1f );
			GameObject light = Scene.CreateObject();
			light.AddComponent<PointLight>();
			light.WorldPosition = wall.GetComponent<SkinnedModelRenderer>().GetAttachment( "flame" ).Value.Position;
			light.Components.Create<LightCullComponent>();
			light.SetParent( MazeRoot );
			GeneratedItems.Add( light );

			light = Scene.CreateObject();
			light.AddComponent<PointLight>();
			light.WorldPosition = wall.GetComponent<SkinnedModelRenderer>().GetAttachment( "flame_copy" ).Value.Position;
			light.Components.Create<LightCullComponent>();
			light.SetParent( MazeRoot );
			GeneratedItems.Add( light );
		}
	}


}
