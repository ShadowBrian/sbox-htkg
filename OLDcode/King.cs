﻿using Sandbox;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace htkgttt;

public partial class King : AnimatedEntity
{
	ModelEntity Hair, Coat, Leggings, Shoes, Roll, Brows;

	[Net, Predicted] public Pawn AssociatedPlayer { get; set; }

	ThroneToilet toil;

	[Net, Predicted] public KingAnimationHelper animHelper { get; set; }

	[Net] bool SetupDone { get; set; }

	[Net] float NextGroanTime { get; set; }

	[Net] TimeSince TimeSinceLastGroan { get; set; }

	[ConVar.Replicated( "hgtk_use_seed" )]
	public static bool hgtk_use_seed { get; set; } = false;

	/// <summary>
	/// Called when the entity is first created 
	/// </summary>
	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/characters/king.vmdl" );

		Hair = new ModelEntity( "models/cosmetics/kingoutfit/hair_crown.vmdl" );
		Coat = new ModelEntity( "models/cosmetics/kingoutfit/king_coat.vmdl" );
		Leggings = new ModelEntity( "models/cosmetics/kingoutfit/king_leggings.vmdl" );
		Shoes = new ModelEntity( "models/cosmetics/kingoutfit/king_shoes.vmdl" );
		Brows = new ModelEntity( "models/citizen_clothes/hair/eyebrows/models/eyebrows.vmdl" );

		Roll = new ModelEntity( "models/items/bogroll/bogroll_w.vmdl" );

		//SetAnimGraph( "models/characters/king.vanmgrph" );

		Hair.SetParent( this, true );
		Coat.SetParent( this, true );
		Leggings.SetParent( this, true );
		Shoes.SetParent( this, true );
		Brows.SetParent( this, true );
		Roll.SetParent( this, true );

		EyePosition = Position + Vector3.Up * 64;
		Tags.Add( "Player" );//CollisionGroup = CollisionGroup.Player;
		SetupPhysicsFromCapsule( PhysicsMotionType.Keyframed, Capsule.FromHeightAndRadius( 72, 8 ) );

		SetBodyGroup( 2, 1 );
		SetBodyGroup( 4, 1 );

		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;

		toil = All.OfType<ThroneToilet>().FirstOrDefault();

		animHelper = new KingAnimationHelper( this );
		SetupDone = true;


	}

	[Net] public Vector3 InputVelocity { get; set; }

	[Net] public Vector3 LookDir { get; set; }

	[Net, Predicted] public bool BladeHit { get; set; }

	[Net] public bool OnToilet { get; set; }

	public override void OnKilled()
	{
		var spawnpoints = Entity.All.OfType<SpawnPoint>();
		var randomSpawnPoint = spawnpoints.OrderBy( x => Guid.NewGuid() ).FirstOrDefault();
		Position = randomSpawnPoint.Position;
		Rotation = randomSpawnPoint.Rotation;
		GroundEntity = null;
	}

	public void HitBlade()
	{
		Log.Trace( "killed king" );
		Pawn.KillPlayerKing();
	}

	public async Task DoFart()
	{
		Sound fart = PlaySound( OnToilet ? "longfart" : "fart" );
		await Task.DelaySeconds( 0.5f );
		PlaySound( "groan" );
	}

	public async void DoForcedScoreSubmit( Client cl, float score )
	{
		Leaderboard? board = await Leaderboard.FindOrCreate( Global.MapName + MazeCreator.Instance.SeedToUse, true );

		if ( board.HasValue )
		{
			Log.Trace( "Found leaderboard, submitting!" );
			Log.Trace( "Submitting " + (int)MathF.Round( (float)TimeSpan.FromSeconds( score ).TotalMilliseconds ) + "milliseconds from " + score + " seconds" );
			LeaderboardUpdate? result = await board.Value.Submit( cl, (int)MathF.Round( (float)TimeSpan.FromSeconds( score ).TotalMilliseconds ), true );

			if ( result.HasValue )
			{
				if ( result.Value.RankChange > 0 )
				{
					Log.Trace( "Rank changed by " + result.Value.RankChange + " spots." );
				}
				else
				{
					Log.Trace( "Your rank didn't change :(" );
				}
			}
			else
			{
				Log.Trace( "Some kind of error occured trying to submit your score!" );
			}
		}
	}

	public async void DoScoreSubmit( Client cl, float score )
	{
		Leaderboard? board = await Leaderboard.FindOrCreate( Global.MapName + MazeCreator.Instance.SeedToUse, true );

		if ( board.HasValue )
		{
			Log.Trace( "Found leaderboard, submitting!" );
			Log.Trace( "Submitting " + (int)MathF.Round( (float)TimeSpan.FromSeconds( score ).TotalMilliseconds ) + "milliseconds from " + score + " seconds" );
			LeaderboardUpdate? result = await board.Value.Submit( cl, (int)MathF.Round( (float)TimeSpan.FromSeconds( score ).TotalMilliseconds) );

			if ( result.HasValue )
			{
				if ( result.Value.RankChange > 0 )
				{
					Log.Trace( "Rank changed by " + result.Value.RankChange + " spots." );
				}
				else
				{
					Log.Trace( "Your rank didn't change :(" );
				}
			}
			else
			{
				Log.Trace( "Some kind of error occured trying to submit your score!" );
			}
		}
	}

	/// <summary>
	/// Called every tick, clientside and serverside.
	/// </summary>
	[Event.Tick.Server]
	public void Tick()
	{
		if ( !SetupDone )
		{
			return;
		}

		if ( AssociatedPlayer == null )
		{
			Delete();
			return;
		}

		if ( TimeSinceLastGroan > NextGroanTime )
		{
			if ( !OnToilet )
			{
				TimeSinceLastGroan = 0f;
				NextGroanTime = Rand.Float( 15f, 50f );
				DoFart();
			}
			else
			{
				TimeSinceLastGroan = 0f;
				NextGroanTime = Rand.Float( 5f, 15f );
				DoFart();
			}
		}

		if ( GroundEntity is KillTrigger )
		{
			OnKilled();
		}

		animHelper.WithLookAt( EyePosition + LookDir );
		animHelper.WithVelocity( Velocity );
		animHelper.WithWishVelocity( InputVelocity );

		animHelper.DuckLevel = MathX.Lerp( animHelper.DuckLevel, AssociatedPlayer.GetAnimParameterFloat( "duck" ), 0.1f );

		animHelper.IsGrounded = GroundEntity != null;

		if ( !animHelper.IsGrounded )
		{
			animHelper.WithVelocity( -Vector3.Up * 400f );
			Move( Time.Delta );
		}


		if ( Vector3.DistanceBetween( Position, toil.Position ) < 55f && !OnToilet && IsServer && !(Game.Current as HTKGGame).WonGame )
		{
			if ( (Game.Current as HTKGGame).CurrentTimerTime > 20f )
			{
				//GameServices.UpdateLeaderboard( AssociatedPlayer.Client.PlayerId, (Game.Current as HTKGGame).CurrentTimerTime, Global.MapName + MazeCreator.Instance.SeedToUse );
				DoScoreSubmit( AssociatedPlayer.Client, (Game.Current as HTKGGame).CurrentTimerTime );
			}

			OnToilet = true;
		}

		if ( OnToilet )
		{
			SetBodyGroup( 2, 1 );
			SetBodyGroup( 2, 0 );
			Leggings.SetBodyGroup( 0, 1 );

			Parent = toil;
			Position = Vector3.Lerp( Position, toil.Position, 0.1f );
			Rotation = Rotation.Slerp( Rotation, toil.Rotation, 1f );

			animHelper.SitState = 1;

			animHelper.SitPose = 1f;

			animHelper.SitHeightOffset = 7.5f;
		}

		float dist = Vector3.DistanceBetween( AssociatedPlayer.Position, Position );
		if ( dist < 55f )
		{
			Vector3 DirectionDifference = (AssociatedPlayer.Position - Position).Normal * 5f;

			Vector3 ClosenessDifference = -Vector3.Up * 10f * (dist / 55f);

			SetAnimParameter( "b_IK_arm", 1 );
			SetAnimParameter( "left_hand_ik.position", AssociatedPlayer.GetBoneTransform( "hold_R" ).Position - (GetBoneTransform( "hand_L" ).Rotation.Forward * 5f) + ClosenessDifference );//AssociatedPlayer.GetBoneTransform( "hand_R" ).Position + AssociatedPlayer.GetBoneTransform( "hand_R" ).PointToLocal( AssociatedPlayer.GetBoneTransform( "hold_R" ).Position ) );

			SetAnimParameter( "left_hand_ik.rotation", Rotation.LookAt( AssociatedPlayer.GetBoneTransform( "arm_lower_R" ).Position - GetBoneTransform( "arm_lower_L" ).Position - Vector3.Up * 5 ) );

			AssociatedPlayer.SetAnimParameter( "b_IK_arm", 2 );
			AssociatedPlayer.SetAnimParameter( "right_hand_ik.position", (AssociatedPlayer.Position + Position) / 2f + Vector3.Up * (45f + (OnToilet ? -5f : -(animHelper.DuckLevel * 25f))) - AssociatedPlayer.GetBoneTransform( "hand_R" ).Rotation.Forward * 5f + ClosenessDifference ); // GetBoneTransform( "hand_L" ).Position + GetBoneTransform( "hand_L" ).PointToLocal( GetBoneTransform( "hold_L" ).Position ) );

			AssociatedPlayer.SetAnimParameter( "right_hand_ik.rotation", Rotation.LookAt( -(AssociatedPlayer.GetBoneTransform( "arm_lower_R" ).Position - GetBoneTransform( "arm_lower_L" ).Position) + Vector3.Up * 5 ) );
		}
		else
		{
			SetAnimParameter( "b_IK_arm", 0 );

			AssociatedPlayer.SetAnimParameter( "b_IK_arm", 0 );
		}



		if ( !OnToilet )
		{
			Move( Time.Delta );
			var walkVelocity = (AssociatedPlayer.Position - Position).WithZ( 0 );
			if ( walkVelocity.Length > 0.1f && Vector3.DistanceBetween( AssociatedPlayer.Position, Position ) < 55f )
			{
				var turnSpeed = 100f;// walkVelocity.Length.LerpInverse( 0, 100, true );
				var targetRotation = Rotation.LookAt( walkVelocity.Normal, Vector3.Up ) * Rotation.From( new Angles( 0, -50f, 0 ) );
				Rotation = Rotation.Lerp( Rotation, targetRotation, turnSpeed * Time.Delta * 20.0f );
			}
		}
	}

	public override void StartTouch( Entity other )
	{

		if ( other.Tags.Has( "KillTrigger" ) || other.Tags.Has( "killtrigger" ) )
		{
			HitBlade();
		}

		if ( other is PickupTrigger )
		{
			StartTouch( other.Parent );
			return;
		}

		//Inventory?.Add( other, Inventory.Active == null );
	}

	protected virtual void Move( float timeDelta )
	{
		var bbox = BBox.FromHeightAndRadius( 32f + (32f * MathF.Abs( animHelper.DuckLevel - 1f )), 4 );
		//DebugOverlay.Box( Position, bbox.Mins, bbox.Maxs, Color.Green );

		MoveHelper move = new( Position, Velocity );
		move.MaxStandableAngle = 50;
		move.Trace = move.Trace.Ignore( this ).Size( bbox );

		if ( !Velocity.IsNearlyZero( 0.001f ) )
		{
			//	Sandbox.Debug.Draw.Once
			//						.WithColor( Color.Red )
			//						.IgnoreDepth()
			//						.Arrow( Position, Position + Velocity * 2, Vector3.Up, 2.0f );

			//using ( Sandbox.Debug.Profile.Scope( "TryUnstuck" ) )
			move.TryUnstuck();

			//using ( Sandbox.Debug.Profile.Scope( "TryMoveWithStep" ) )
			move.TryMoveWithStep( timeDelta, 30 );
		}

		//using ( Sandbox.Debug.Profile.Scope( "Ground Checks" ) )
		//{
		var tr = move.TraceDirection( Vector3.Down * 10.0f );

		if ( move.IsFloor( tr ) )
		{
			GroundEntity = tr.Entity;

			if ( !tr.StartedSolid )
			{
				move.Position = tr.EndPosition;
			}

			if ( InputVelocity.Length > 0 )
			{
				var movement = move.Velocity.Dot( InputVelocity.Normal );
				move.Velocity = move.Velocity - movement * InputVelocity.Normal;
				move.ApplyFriction( tr.Surface.Friction * 10.0f, timeDelta );
				move.Velocity += movement * InputVelocity.Normal;

			}
			else
			{
				move.ApplyFriction( tr.Surface.Friction * 10.0f, timeDelta );
			}
		}
		else
		{
			GroundEntity = null;
			move.Velocity += Vector3.Down * 900 * timeDelta;
			//Sandbox.Debug.Draw.Once.WithColor( Color.Red ).Circle( Position, Vector3.Up, 10.0f );
		}
		//}

		Position = move.Position;
		Velocity = move.Velocity;
	}

	/// <summary>
	/// Called every frame on the client
	/// </summary>
	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );

		// Update rotation every frame, to keep things smooth
		//Rotation = Input.Rotation;
		//EyeRotation = Rotation;
	}
}
