using Sandbox;
using Sandbox.VR;
using static Sandbox.PlayerController;

public sealed class JesterController : Component, Component.ICollisionListener
{
	[Property] public SkinnedModelRenderer Body { get; set; }
	[Property, Sync] public bool CheckingItem { get; set; }

	[Property, Sync] public bool HoldingMap { get; set; }

	[Property] public GameObject Hat { get; set; }
	[Property] public GameObject Compass { get; set; }
	[Property] public GameObject Map { get; set; }

	[Property] public PlayerController PlayerController { get; set; }

	Vector3 StartPosition { get; set; }

	[Property, Sync] bool died { get; set; }

	void Component.ICollisionListener.OnCollisionStart( Collision collision )
	{
		if ( !died && collision.Other.GameObject.Tags.Has( "killtrigger" ) )
		{
			TriggerDeath();
		}
	}

	public async void TriggerDeath()
	{
		PlayerController.CreateRagdoll().NetworkSpawn();
		/*var ragdoll = PlayerController.Renderer.GameObject.Clone();
		ragdoll.SetParent( null );
		ragdoll.GetComponent<SkinnedModelRenderer>().CopyFrom( PlayerController.Renderer );
		ragdoll.WorldPosition = WorldPosition;
		ragdoll.WorldRotation = PlayerController.Renderer.WorldRotation;
		var modelphys = ragdoll.AddComponent<ModelPhysics>();
		modelphys.Model = PlayerController.Renderer.Model;
		modelphys.Renderer = ragdoll.GetComponent<SkinnedModelRenderer>();
		ragdoll.NetworkSpawn();*/
		died = true;
		await Task.DelaySeconds( 5f );
		WorldPosition = StartPosition;
		died = false;
	}

	Vector3 CameraOffset { get; set; } = new Vector3( 120f, 0, 0 );

	float _cameraDistance;
	float _eyez;
	private void UpdateCameraPosition()
	{
		CameraComponent cam = base.Scene.Camera;
		if ( cam == null )
		{
			return;
		}

		Rotation worldRotation = PlayerController.EyeAngles.ToRotation();
		cam.WorldRotation = worldRotation;
		Vector3 from = base.WorldPosition + Vector3.Up * (PlayerController.BodyHeight - PlayerController.EyeDistanceFromTop);
		if ( PlayerController.IsOnGround && _eyez != 0f )
		{
			from.z = _eyez.LerpTo( from.z, Time.Delta * 50f );
		}

		_eyez = from.z;

		Vector3 vector = worldRotation.Forward * (0f - CameraOffset.x) + worldRotation.Up * CameraOffset.z + worldRotation.Right * CameraOffset.y;
		SceneTrace trace = base.Scene.Trace;
		Vector3 to = from + vector;
		SceneTraceResult sceneTraceResult = trace.FromTo( in from, in to ).IgnoreGameObjectHierarchy( base.GameObject ).WithoutTags( "king" ).Radius( 8f )
			.Run();
		if ( sceneTraceResult.StartedSolid )
		{
			_cameraDistance = _cameraDistance.LerpTo( vector.Length, Time.Delta * 100f );
		}
		else if ( sceneTraceResult.Distance < _cameraDistance )
		{
			_cameraDistance = _cameraDistance.LerpTo( sceneTraceResult.Distance, Time.Delta * 200f );
		}
		else
		{
			_cameraDistance = _cameraDistance.LerpTo( sceneTraceResult.Distance, Time.Delta * 2f );
		}

		from += vector.Normal * _cameraDistance;


		cam.WorldPosition = from;

		ISceneEvent<IEvents>.PostToGameObject( base.GameObject, delegate ( IEvents x )
		{
			x.PostCameraSetup( cam );
		} );
	}

	protected override void OnStart()
	{
		base.OnStart();
		StartPosition = WorldPosition;
		PlayerController.WalkSpeed = 110f;
		PlayerController.RunSpeed = 300f;
		PlayerController.DuckedSpeed = 70f;
	}

	protected override void OnUpdate()
	{
		PlayerController.Renderer.GameObject.Enabled = !died;
		if ( !IsProxy )
		{
			PlayerController.Body.Enabled = !died;
			if ( WorldPosition.z < -10f && !died )
			{
				TriggerDeath();
				return;
			}
			if ( died )
			{
				UpdateCameraPosition();
				return;
			}

			UpdateCameraPosition();

			if ( CheckingItem != Input.Down( "attack2" ) )
			{
				CheckingItem = Input.Down( "attack2" );
				foreach ( var item in PlayerController.Renderer.GetComponentsInChildren<SkinnedModelRenderer>( true ) )
				{
					if ( item.Tags.Has( "clothing" ) && !item.GameObject.Name.ToLower().Contains( "glove" ) )
					{
						item.Enabled = !CheckingItem;
					}
				}
				Hat.Enabled = !CheckingItem;
			}
			CameraOffset = new Vector3( !CheckingItem ? 120f : -10f, 0, 0 );
			PlayerController.Renderer.Tags.Remove( "viewer" );
			PlayerController.Renderer.SetBodyGroup( "head", !CheckingItem ? 0 : 1 );

			if ( Input.MouseWheel.Length > 0.5f )
			{
				HoldingMap = !HoldingMap;
			}
		}
		if ( Compass.IsValid() )
			Compass.Enabled = CheckingItem && !HoldingMap;
		if ( Map.IsValid() )
			Map.Enabled = CheckingItem && HoldingMap;

		if ( Body.IsValid() )
		{
			Body.Set( "b_check_item", CheckingItem );
			Body.Set( "holding_item", HoldingMap ? 0 : 1 );
		}
	}
}
