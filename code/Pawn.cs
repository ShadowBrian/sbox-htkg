using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace htkgttt;

public partial class Pawn : Player
{
	[Net, Predicted]public ModelEntity Hat { get; set; }//, Coat, Leggings, Shoes;

	[Net, Predicted] bool Died { get; set; } = false;

	public ClothingContainer ClothingContainer { get; set; } = new();

	[Net, Predicted] public King AssociatedKing { get; set; }

	[Net]public List<BaseCarriable> BasicInventory { get; set; } = new List<BaseCarriable>();

	[Net, Predicted]public int CurrentItemUsing { get; set; }

	public Pawn()
	{

	}

	public Pawn( Client client ) : this()
	{
		ClothingContainer.LoadFromClient( client );
	}

	/// <summary>
	/// Called when the entity is first created 
	/// </summary>
	public override void Spawn()
	{
		base.Spawn();

		Health = 100f;

		SetModel( "models/characters/king.vmdl" );

		Tags.Add( "player" );

		List<Clothing> clothesToYeet = new List<Clothing>();

		foreach ( var item in ClothingContainer.Clothing )
		{
			if ( item.Category == Clothing.ClothingCategory.Hat || item.Category == Clothing.ClothingCategory.Facial || (item.Category == Clothing.ClothingCategory.Hair && !item.Model.ToLower().Contains( "brow" )) )
			{
				clothesToYeet.Add( item );
			}
		}

		foreach ( var item in clothesToYeet )
		{
			ClothingContainer.Clothing.Remove( item );
		}

		ClothingContainer.DressEntity( this );

		Hat = new ModelEntity( "models/cosmetics/jester/jester_hat.vmdl" );
		//Coat = new ModelEntity( "models/cosmetics/kingoutfit/king_coat.vmdl" );
		//Leggings = new ModelEntity( "models/cosmetics/kingoutfit/king_leggings.vmdl" );
		//Shoes = new ModelEntity( "models/cosmetics/kingoutfit/king_shoes.vmdl" );

		Hat.SetParent( this, true );
		//Coat.SetParent( this, true );
		//Leggings.SetParent( this, true );
		//Shoes.SetParent( this, true );

		//SetBodyGroup( 2, 1 );
		//SetBodyGroup( 4, 1 );

		Animator = new StandardPlayerAnimator();
		Controller = new WalkController();
		CameraMode = new KingCamera();
		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;


		EnableTouch = true;
		EnableTouchPersists = true;

		CreateHull();

		//SetAnimGraph( "models/characters/king.vanmgrph" );
	}

	/// <summary>
	/// Called every tick, clientside and serverside.
	/// </summary>
	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		if (IsServer )
		{
			if ( !Tags.Has( "player" ) )
			{
				Tags.Add( "player" );
				Log.Trace( "Added player tag" );
			}
		}

		ActiveChild.Simulate( cl );

		if ( Input.Pressed( InputButton.SlotNext ) || Input.MouseWheel > 0 )
		{
			BasicInventory[CurrentItemUsing].ActiveEnd( this, false );
			CurrentItemUsing++;
			if(CurrentItemUsing > 1 )
			{
				CurrentItemUsing = 0;
			}
			BasicInventory[CurrentItemUsing].ActiveStart( this);
		}

		if ( Input.Pressed( InputButton.SlotPrev ) || Input.MouseWheel < 0 )
		{
			BasicInventory[CurrentItemUsing].ActiveEnd( this, false );
			CurrentItemUsing--;
			if ( CurrentItemUsing < 0 )
			{
				CurrentItemUsing = 1;
			}
			BasicInventory[CurrentItemUsing].ActiveStart( this );
		}

		SetAnimParameter( "itemtypeholding", CurrentItemUsing);
		SetAnimParameter( "b_checking_map", Input.Down( InputButton.SecondaryAttack ) );

		if ( !Input.Down( InputButton.SecondaryAttack ) )
		{
			SetBodyGroup( 0, 1 );
			SetBodyGroup( 0, 0 );
		}

		if ( AssociatedKing != null )
		{
			AssociatedKing.AssociatedPlayer = this;

			if ( Vector3.DistanceBetween( AssociatedKing.Position, Position ) > 40f && Vector3.DistanceBetween( AssociatedKing.Position, Position ) < 55f && GroundEntity != null )
			{
				AssociatedKing.Velocity = Vector3.Lerp( AssociatedKing.Velocity, - (AssociatedKing.Position - Position) * Time.Delta * 62f, 0.9f );
			}
			else
			{
				AssociatedKing.Velocity = Vector3.Lerp( AssociatedKing.Velocity, Vector3.Zero, 0.1f);
			}
			AssociatedKing.LookDir = AssociatedKing.Rotation.Forward;
		}
	}

	/// <summary>
	/// Called every frame on the client
	/// </summary>
	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );
	}

	[ConCmd.Server( "killplayer" )]
	public static void KillPlayer()
	{
		Game.Current?.OnKilled( ConsoleSystem.Caller.Pawn );
	}

	[ConCmd.Server( "killplayerking" )]
	public static void KillPlayerKing()
	{
		Game.Current?.OnKilled( (ConsoleSystem.Caller.Pawn as Pawn).AssociatedKing );
	}

	public void HitBlade()
	{
		if ( LifeState != LifeState.Dead )
		{
			KillPlayer();
			LifeState = LifeState.Dead;
		}
	}

	public override void StartTouch( Entity other )
	{

		if ( other.Tags.Has( "KillTrigger" ) || other.Tags.Has( "killtrigger" ) )
		{
			KillPlayer();
		}

		if ( other is PickupTrigger )
		{
			StartTouch( other.Parent );
			return;
		}

		//Inventory?.Add( other, Inventory.Active == null );
	}
}
