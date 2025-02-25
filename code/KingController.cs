using Sandbox;
using Sandbox.Citizen;
using System.Linq;
public sealed class KingController : Component, Component.IPressable, Component.ICollisionListener
{
	[RequireComponent, Property] PlayerController Controller { get; set; }

	[Property, Sync] public PlayerController TargetPlayer { get; set; }

	[Property, Sync] public GameObject ThroneTarget { get; set; }

	[Property] public SkinnedModelRenderer PantsRenderer { get; set; }

	CitizenAnimationHelper animationHelper { get; set; }

	bool OnThrone;

	Vector3 StartPosition { get; set; }

	void Component.ICollisionListener.OnCollisionStart( Collision collision )
	{
		if ( collision.Other.GameObject.Tags.Has( "killtrigger" ) && !died )
		{
			TriggerDeath();
		}
	}

	protected override void OnStart()
	{
		base.OnStart();
		StartPosition = WorldPosition;
		animationHelper = GetComponentInChildren<CitizenAnimationHelper>();

		ThroneTarget = Scene.GetAllObjects( true ).Where( X => X.Tags.Has( "throne" ) ).Last();

		//Controller.Body.Tags.Add( "king" );
		//Controller.BodyCollider.Tags.Add( "king" );
		//Controller.ColliderObject.Tags.Add( "king" );
		//Controller.BodyCollisionTags.Add( "king" );
	}

	[Property, Sync] bool died { get; set; }

	public async void TriggerDeath()
	{
		/*var ragdoll = Controller.Renderer.GameObject.Clone();
		ragdoll.SetParent( null );
		ragdoll.GetComponent<SkinnedModelRenderer>().CopyFrom( Controller.Renderer );
		ragdoll.WorldPosition = WorldPosition;
		ragdoll.WorldRotation = Controller.Renderer.WorldRotation;
		var modelphys = ragdoll.AddComponent<ModelPhysics>();
		modelphys.Model = Controller.Renderer.Model;
		modelphys.Renderer = ragdoll.GetComponent<SkinnedModelRenderer>();
		ragdoll.NetworkSpawn();*/

		Controller.CreateRagdoll().NetworkSpawn();

		died = true;
		await Task.DelaySeconds( 5f );
		WorldPosition = StartPosition;
		died = false;
	}

	protected override void OnUpdate()
	{
		Controller.Renderer.GameObject.Enabled = !died;
		if ( !OnThrone && Vector3.DistanceBetween( WorldPosition, ThroneTarget.WorldPosition ) < 64f )
		{
			if ( TargetPlayer.IsValid() )
			{
				TargetPlayer.Renderer.Set( "ik.hand_right.enabled", false );
				TargetPlayer.WalkSpeed = 110f;
				TargetPlayer.RunSpeed = 300f;
				TargetPlayer.DuckedSpeed = 70f;
				TargetPlayer = null;
			}
			OnThrone = true;
		}
		if ( WorldPosition.z < -10f && !died )
		{
			TriggerDeath();
			return;
		}
		if ( OnThrone )
		{
			if ( !HelpKingHUD.LocalInstance.Ended )
				HelpKingHUD.LocalInstance.StopTimer( Network.Owner.DisplayName, HelpKingHUD.LocalInstance.Timer );
			animationHelper.Target.LocalRotation = Rotation.Identity;
			animationHelper.Sitting = CitizenAnimationHelper.SittingStyle.Chair;
			animationHelper.SittingPose = 1f;
			animationHelper.SittingOffsetHeight = 6f;
			animationHelper.Target.Set( "face_override", 2 );
			PantsRenderer.SetBodyGroup( 0, 1 );
			animationHelper.Target.SetBodyGroup( "legs", 0 );
			animationHelper.IkLeftHand = null;
			WorldPosition = ThroneTarget.WorldPosition;
			WorldRotation = ThroneTarget.WorldRotation;
			return;
		}

		if ( died )
		{
			return;
		}

		Controller.UseInputControls = false;
		animationHelper.WithVelocity( Controller.Velocity );
		animationHelper.WithWishVelocity( Controller.WishVelocity );
		animationHelper.IsGrounded = Controller.IsOnGround;
		if ( TargetPlayer.IsValid() )
		{
			if ( Vector3.DistanceBetween( TargetPlayer.WorldPosition, WorldPosition ) > 32f )
			{
				Controller.WishVelocity = Controller.Mode.UpdateMove( Controller.EyeAngles, (TargetPlayer.WorldPosition - WorldPosition).WithZ( 0f ).ClampLength( 1f ) * (Rotation.FromYaw( -90f )) );
			}
			else
			{
				Controller.WishVelocity = 0f;
			}

			animationHelper.DuckLevel = TargetPlayer.IsDucking ? 1f : 0f;

			animationHelper.WorldRotation = Rotation.Lerp( animationHelper.WorldRotation, Rotation.LookAt( (TargetPlayer.WorldPosition - WorldPosition).WithZ( 0 ), Vector3.Up ) * Rotation.FromYaw( -60f ), 0.1f );

			animationHelper.LookAt = TargetPlayer.GameObject;

			animationHelper.IkLeftHand = TargetPlayer.Renderer.GetBoneObject( "hold_R" );
			TargetPlayer.Renderer.Set( "ik.hand_right.enabled", true );
			var WT = TargetPlayer.Renderer.WorldTransform;
			var targetpos = ((TargetPlayer.WorldPosition + WorldPosition) / 2f) + Vector3.Up * (40f * (animationHelper.DuckLevel == 0f ? 1f : 0.5f));
			//Gizmo.Draw.LineSphere( targetpos, 10f );
			TargetPlayer.Renderer.Set( "ik.hand_right.position", WT.PointToLocal( targetpos ) );
			TargetPlayer.Renderer.Set( "ik.hand_right.rotation", Rotation.From( 90, 0f, 45 ) );

			//TargetPlayer.Renderer.WorldRotation *= Rotation.FromYaw( -45f );

			if ( Vector3.DistanceBetween( TargetPlayer.WorldPosition, WorldPosition ) > 54f )
			{
				TargetPlayer.Renderer.Set( "ik.hand_right.enabled", false );
				TargetPlayer.WalkSpeed = 110f;
				TargetPlayer.RunSpeed = 300f;
				TargetPlayer.DuckedSpeed = 70f;
				TargetPlayer = null;
			}
		}
		else
		{
			Controller.WishVelocity = 0f;
			if ( !TargetPlayer.IsValid() )
				animationHelper.IkLeftHand = null;
		}
	}

	public bool Press( IPressable.Event e )
	{
		if ( e.Source.GameObject.Network.Owner == Network.Owner )
		{
			TargetPlayer = e.Source as PlayerController;
			TargetPlayer.WalkSpeed = Controller.WalkSpeed;
			TargetPlayer.RunSpeed = Controller.RunSpeed;
			TargetPlayer.DuckedSpeed = Controller.DuckedSpeed;
			return true;
		}
		else
		{
			return false;
		}
	}
}
