using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using SandboxEditor;
namespace htkgttt
{
	[Library( "ent_htk_toilet" )]
	[HammerEntity]
	[EditorModel( "models/items/throne_toilet/throne_toilet.vmdl" )]
	public class ThroneToilet : ModelEntity
	{

		public override void Spawn()
		{
			SetModel( "models/items/throne_toilet/throne_toilet.vmdl" );
			SetupPhysicsFromModel( PhysicsMotionType.Keyframed );
			EnableTouch = true;
			base.Spawn();
		}

		public override void Simulate( Client cl )
		{
			base.Simulate( cl );
		}

		public override void StartTouch( Entity other )
		{
			base.StartTouch( other );
			if(other is King king )
			{
				king.SetParent( this );
				king.Position = Position;
				king.Rotation = Rotation;
			}
		}

	}
}
