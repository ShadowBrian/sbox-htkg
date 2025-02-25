using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace htkgttt
{
	public partial class KillTrigger : ModelEntity
	{
		public override void Spawn()
		{
			base.Spawn();
			SetModel( "models/walls/dungeon/floor_tiles_killtrigger.vmdl" );

			//SetupPhysicsFromAABB( PhysicsMotionType.Static, new Vector3( -2.55f, -2.55f, -0.5f ) * Scale, new Vector3( 2.55f, 2.55f, 0.5f ) * Scale );
			//SetupPhysicsFromCapsule( PhysicsMotionType.Keyframed, new Capsule( Vector3.Zero, Vector3.One * 0.1f, 10f ) );

			SetupPhysicsFromModel( PhysicsMotionType.Keyframed );

			Tags.Add( "trigger" );

			Tags.Add( "killtrigger" );

			EnableSolidCollisions = false;

			EnableTouch = true;
		}
	}
}
