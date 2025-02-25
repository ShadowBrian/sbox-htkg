using htkgttt;
using Sandbox;
using static Sandbox.Citizen.CitizenAnimationHelper;
using static Sandbox.ParticleModelRenderer;
using System.Collections.Generic;
using System.Linq;

public sealed class MazeMap : Component
{
	WallState[,] maze;

	int width, height;

	float scale;

	bool UseSeed;
	int MazeSeed;

	GameObject MapParentEnt { get; set; }

	protected override void OnStart()
	{
		if ( MazeCreator.Instance.IsValid() )
		{
			maze = MazeCreator.Instance.maze;

			width = MazeCreator.Instance.width;
			height = MazeCreator.Instance.height;

			scale = MazeCreator.Instance.scale * 0.0075f * 0.7f;
			MapParentEnt = Scene.CreateObject();
			MapParentEnt.Name = "MAP";
			Generate();
		}
	}

	public void Generate()
	{
		bool PickedStart = false;
		bool PickedEnd = false;

		bool ThisIsEnd = false;
		bool ThisIsStart = false;

		Game.SetRandomSeed( System.DateTime.Now.Month );

		string wallmodel = "models/walls/map_wall.vmdl";

		Vector3 totalPositions = new Vector3();

		float cellnums = 0f;

		List<GameObject> spawnedWalls = new List<GameObject>();


		for ( int i = 0; i < width; ++i )
		{
			for ( int j = 0; j < height; ++j )
			{
				var cell = maze[i, j];
				var mapRotation = Rotation.Identity;
				var position = new Vector3( -width / 2f + (float)i * scale, (-height / 2f + (float)j * scale), 0f );
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
						GameObject wall = Scene.CreateObject();
						SkinnedModelRenderer wallRenderer = wall.AddComponent<SkinnedModelRenderer>();
						wallRenderer.Model = Model.Load( wallmodel );
						wallRenderer.SetBodyGroup( 0, Game.Random.Int( 0, 2 ) );

						wall.WorldPosition = position + mapRotation * (new Vector3( 0, 0.5f, 0 ) * scale);
						wall.WorldScale = scale / 5f;

						wall.WorldRotation = Rotation.From( new Angles( 0, -90, 0 ) );

						spawnedWalls.Add( wall );
					}
					else
					{
						ThisIsEnd = false;
					}

				}

				if ( cell.HasFlag( WallState.LEFT ) )
				{
					GameObject wall = Scene.CreateObject();
					SkinnedModelRenderer wallRenderer = wall.AddComponent<SkinnedModelRenderer>();
					wallRenderer.Model = Model.Load( wallmodel );
					wallRenderer.SetBodyGroup( 0, Game.Random.Int( 0, 2 ) );

					wall.WorldPosition = position + mapRotation * (new Vector3( -0.5f, 0, 0 ) * scale);
					wall.WorldScale = scale / 5f;
					spawnedWalls.Add( wall );
				}

				if ( i == width - 1 )
				{

					if ( cell.HasFlag( WallState.RIGHT ) )
					{
						GameObject wall = Scene.CreateObject();
						SkinnedModelRenderer wallRenderer = wall.AddComponent<SkinnedModelRenderer>();
						wallRenderer.Model = Model.Load( wallmodel );
						wallRenderer.SetBodyGroup( 0, Game.Random.Int( 0, 2 ) );

						wall.WorldPosition = position + mapRotation * (new Vector3( 0.5f, 0, 0 ) * scale);
						wall.WorldScale = scale / 5f;
						spawnedWalls.Add( wall );
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
							GameObject wall = Scene.CreateObject();
							SkinnedModelRenderer wallRenderer = wall.AddComponent<SkinnedModelRenderer>();
							wallRenderer.Model = Model.Load( wallmodel );
							wallRenderer.SetBodyGroup( 0, Game.Random.Int( 0, 2 ) );

							wall.WorldPosition = position + mapRotation * (new Vector3( 0, -0.5f, 0 ) * scale);
							wall.WorldScale = scale / 5f;


							spawnedWalls.Add( wall );
							wall.WorldRotation = Rotation.From( new Angles( 0, 90, 0 ) );
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

		MapParentEnt.WorldPosition = totalPositions;

		foreach ( var item in spawnedWalls )
		{
			item.SetParent( MapParentEnt );
		}

		var ModelRenderer = GetComponent<SkinnedModelRenderer>();
		MapParentEnt.WorldPosition = ModelRenderer.GetAttachment( "mapSpot" ).Value.Position;
		MapParentEnt.WorldRotation = ModelRenderer.GetAttachment( "mapSpot" ).Value.Rotation;
		MapParentEnt.SetParent( GameObject );
		MapParentEnt.LocalPosition = GameObject.Children.ElementAt( 1 ).LocalPosition;//ModelRenderer.GetAttachment( "mapSpot", false ).Value.Position + Vector3.Left * 15f - Vector3.Down * 10f;
		MapParentEnt.LocalRotation = ModelRenderer.GetAttachment( "mapSpot", false ).Value.Rotation;
		//MapParentEnt.WorldPosition = GameObject.GetBounds().Center + MapParentEnt.WorldRotation.Up * 0.75f - MapParentEnt.WorldRotation.Forward * 0.5f + MapParentEnt.WorldRotation.Right * 10f;
	}

}
