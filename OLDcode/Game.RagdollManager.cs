using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace htkgttt;

public partial class Pawn
{
	// TODO - make ragdolls one per entity
	// TODO - make ragdolls dissapear after a load of seconds
	static EntityLimit RagdollLimit = new EntityLimit { MaxTotal = 20 };

	[ClientRpc]
	public static void BecomeRagdollOnClient( Vector3 force, int forceBone, AnimatedEntity BaseEnt )
	{
		// TODO - lets not make everyone write this shit out all the time
		// maybe a CreateRagdoll<T>() on ModelEntity?
		var ent = new ModelEntity();
		ent.Position = BaseEnt.Position;
		ent.Rotation = BaseEnt.Rotation;
		ent.UsePhysicsCollision = true;

		ent.SetupPhysicsFromModel( PhysicsMotionType.Dynamic );


		ent.CopyFrom( BaseEnt );
		ent.CopyBonesFrom( BaseEnt );
		ent.SetRagdollVelocityFrom( BaseEnt );
		ent.DeleteAsync( 20.0f );

		// Copy the clothes over
		foreach ( var child in BaseEnt.Children )
		{

			if ( child is ModelEntity e )
			{
				var clothing = new ModelEntity();
				clothing.CopyFrom( e );
				clothing.SetParent( ent, true );
			}
		}

		ent.PhysicsGroup.AddVelocity( force );

		if ( forceBone >= 0 )
		{
			var body = ent.GetBonePhysicsBody( forceBone );
			if ( body != null )
			{
				body.ApplyForce( force * 1000 );
			}
			else
			{
				ent.PhysicsGroup.AddVelocity( force );
			}
		}

		RagdollLimit.Watch( ent );
	}
}
