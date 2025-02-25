using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace Sandbox;

/// <summary>
/// Creates a networked game lobby and assigns player prefabs to connected clients.
/// </summary>
[Title( "Network Helper" )]
[Category( "Networking" )]
[Icon( "electrical_services" )]
public sealed class KingNetworkHelper : Component, Component.INetworkListener
{
	/// <summary>
	/// Create a server (if we're not joining one)
	/// </summary>
	[Property] public bool StartServer { get; set; } = true;

	/// <summary>
	/// The prefab to spawn for the player to control.
	/// </summary>
	[Property] public GameObject PlayerPrefab { get; set; }

	[Property] public GameObject KingPrefab { get; set; }

	/// <summary>
	/// A list of points to choose from randomly to spawn the player in. If not set, we'll spawn at the
	/// location of the NetworkHelper object.
	/// </summary>
	[Property] public List<GameObject> SpawnPoints { get; set; }

	protected override async Task OnLoad()
	{
		if ( Scene.IsEditor )
			return;

		if ( StartServer && !Networking.IsActive )
		{
			LoadingScreen.Title = "Creating Lobby";
			await Task.DelayRealtimeSeconds( 0.1f );
			Networking.CreateLobby( new() );
		}
	}

	/// <summary>
	/// A client is fully connected to the server. This is called on the host.
	/// </summary>
	public void OnActive( Connection channel )
	{
		Log.Info( $"Player '{channel.DisplayName}' has joined the game" );

		if ( !PlayerPrefab.IsValid() )
			return;

		//
		// Find a spawn location for this player
		//
		var startLocation = FindSpawnLocation().WithScale( 1 );

		// Spawn this object and make the client the owner
		var player = PlayerPrefab.Clone( startLocation, name: $"Player - {channel.DisplayName}" );

		var clothing = ClothingContainer.CreateFromLocalUser();
		var filteredClothing = new List<ClothingContainer.ClothingEntry>();
		foreach ( var item in clothing.Clothing )
		{
			if ( item.Clothing.SlotsUnder != Clothing.Slots.HeadTop )
			{
				filteredClothing.Add( item );
			}
		}
		clothing.Clothing.Clear();
		clothing.AddRange( filteredClothing );

		clothing.Apply( player.Children.First().GetComponent<SkinnedModelRenderer>() );

		player.NetworkSpawn( channel );

		Transform KingLocation = startLocation.WithPosition( startLocation.Position + Vector3.Random.WithZ( 0f ).Normal * 64f );

		var king = KingPrefab.Clone( KingLocation, name: $"King - {channel.DisplayName}" );
		king.NetworkSpawn( channel );
	}

	/// <summary>
	/// Find the most appropriate place to respawn
	/// </summary>
	Transform FindSpawnLocation()
	{
		//
		// If they have spawn point set then use those
		//
		if ( SpawnPoints is not null && SpawnPoints.Count > 0 )
		{
			return Random.Shared.FromList( SpawnPoints, default ).WorldTransform;
		}

		//
		// If we have any SpawnPoint components in the scene, then use those
		//
		var spawnPoints = Scene.GetAllComponents<SpawnPoint>().ToArray();
		if ( spawnPoints.Length > 0 )
		{
			return Random.Shared.FromArray( spawnPoints ).WorldTransform;
		}

		//
		// Failing that, spawn where we are
		//
		return WorldTransform;
	}
}
