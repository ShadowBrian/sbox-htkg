using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace htkgttt
{
	public partial class CompassCarriable : BaseCarriable
	{

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "models/items/compass/compass.vmdl" );
		}

		public override void ActiveStart( Entity ent )
		{
			base.ActiveStart( ent );
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
		[Event.Frame]
		public void IndicateNorth()
		{
			Transform NorthNeedle = GetBoneTransform( "needle", true );

			NorthNeedle.Position = (Parent as ModelEntity).GetBoneTransform( "hold_L" ).Position + (GetBoneTransform("compass").Rotation.Right * 2.5f);


			Rotation desiredDirection = Rotation.LookAt( new Vector3( 0, 1, 0 ), GetBoneTransform( "compass" ).Rotation.Up );

			NorthNeedle.Rotation = Rotation.LookAt( Vector3.VectorPlaneProject( desiredDirection.Forward, GetBoneTransform( "compass" ).Rotation.Up ), GetBoneTransform( "compass" ).Rotation.Up );
			

			SetBoneTransform( "needle", NorthNeedle, true );
		}
	}
}
