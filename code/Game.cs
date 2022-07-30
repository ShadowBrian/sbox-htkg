using Sandbox;
using Sandbox.UI.Construct;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

//
// You don't need to put things in a namespace, but it doesn't hurt.
//
namespace htkgttt;

/// <summary>
/// This is your game class. This is an entity that is created serverside when
/// the game starts, and is replicated to the client. 
/// 
/// You can use this to create things like HUDs and declare which player class
/// to use for spawned players.
/// </summary>
public partial class HTKGGame : Sandbox.Game
{
	[Net]public  float CurrentTimerTime { get; set; } = 0f;

	[Net, Predicted]public bool WonGame { get; set; }

	[Net]public string WinningPlayer { get; set; }

	public HTKGGame()
	{
		if ( IsClient )
		{
			HTKGHUD hud = new HTKGHUD();
		}
	}

	bool TimerRebootStarted;
	TimeSince RebootTimer;

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		if ( !IsClient && !WonGame )
		{
			CurrentTimerTime = Time.Now;

		}else if( !IsClient && !TimerRebootStarted )
		{
			TimerRebootStarted = true;
			RebootTimer = 0f;
		}

		if ( !IsClient && TimerRebootStarted && RebootTimer > 20f )
		{
			Global.ChangeLevel( Global.MapName );
		}
	}

	[ConCmd.Server]
	public void KingSatOnToilet(string player)
	{
		(Game.Current as HTKGGame).WonGame = true;
		(Game.Current as HTKGGame).WinningPlayer = player;
	}

	public override void PostCameraSetup( ref CameraSetup camSetup )
	{
		base.PostCameraSetup( ref camSetup );

		camSetup.ZNear = 1f;
	}

	public override void OnKilled(Entity playerPawn)
	{
		base.OnKilled( playerPawn );
		Pawn.BecomeRagdollOnClient( Vector3.Up * 100f, 0, playerPawn as AnimatedEntity );
		var spawnpoints = Entity.All.OfType<SpawnPoint>();
		var randomSpawnPoint = spawnpoints.OrderBy( x => Guid.NewGuid() ).FirstOrDefault();
		playerPawn.Position = randomSpawnPoint.Position;
		playerPawn.Rotation = randomSpawnPoint.Rotation;
		
	}

	public void OnKilledKing( King king )
	{
		Pawn.BecomeRagdollOnClient( Vector3.Up * 100f, 0, king as AnimatedEntity );
		var spawnpoints = Entity.All.OfType<SpawnPoint>();
		var randomSpawnPoint = spawnpoints.OrderBy( x => Guid.NewGuid() ).FirstOrDefault();
		king.Position = randomSpawnPoint.Position;
		king.Rotation = randomSpawnPoint.Rotation;

	}



	/// <summary>
	/// A client has joined the server. Make them a pawn to play with
	/// </summary>
	public override void ClientJoined( Client client )
	{
		base.ClientJoined( client );

		// Create a pawn for this client to play with
		var pawn = new Pawn( client );
		client.Pawn = pawn;

		// Get all of the spawnpoints
		if ( Entity.All.OfType<MazeCreator>().Count() != 0 )
		{
			var spawnpoint = Entity.All.OfType<MazeCreator>().FirstOrDefault().StartPos;
		}
		

		foreach ( PointLightEntity light in Entity.All.OfType<PointLightEntity>() )
		{
			light.Components.GetOrCreate<LightCullComponent>();
		}

		var spawnpoints = Entity.All.OfType<SpawnPoint>();
		// chose a random one
		var randomSpawnPoint = spawnpoints.OrderBy( x => Guid.NewGuid() ).FirstOrDefault();

		// if it exists, place the pawn there
		if ( randomSpawnPoint != null )
		{
			var tx = new Transform();
			tx.Position = randomSpawnPoint.Position;//tx.Position + Vector3.Up * 50.0f; // raise it up
			tx.Rotation = randomSpawnPoint.Rotation;
			King king = new King();
			king.AssociatedPlayer = pawn;
			
			king.Owner = pawn.Owner;
			pawn.AssociatedKing = king;
			tx.Position += (Vector3.Random * 60f).WithZ( 0 );
			pawn.Transform = tx;
			tx.Position += tx.Rotation.Backward * 60f;
			king.Transform = tx;
			MazeMapCarriable map = new MazeMapCarriable();
			map.Position = pawn.Position;
			map.SetParent(pawn,true);
			pawn.ActiveChild = map;
			map.ActiveStart( pawn );
			pawn.BasicInventory.Add( map );

			CompassCarriable compass = new CompassCarriable();
			compass.Position = pawn.Position;
			compass.SetParent( pawn, true );
			pawn.ActiveChild = compass;
			pawn.BasicInventory.Add( compass );
			compass.ActiveEnd( pawn, false );

			pawn.Spawn();
		}
	}
}
