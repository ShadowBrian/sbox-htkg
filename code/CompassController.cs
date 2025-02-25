using Sandbox;
using static Sandbox.ParticleModelRenderer;
using static Sandbox.PhysicsGroupDescription.BodyPart;

public sealed class CompassController : Component
{
	[Property] public SkinnedModelRenderer Renderer { get; set; }
	[Property] public GameObject NeedleBone { get; set; }
	protected override void OnUpdate()
	{
		Transform Compass;
		if ( Renderer.TryGetBoneTransform( "compass", out Compass ) )
		{
			Transform HoldBone;
			if ( Renderer.TryGetBoneTransform( "hold_L", out HoldBone ) )
			{
				Rotation desiredDirection = Rotation.LookAt( new Vector3( 0, 1, 0 ), Compass.Rotation.Up );

				var NorthNeedleRotation = Rotation.LookAt( Vector3.VectorPlaneProject( desiredDirection.Forward, Compass.Rotation.Up ), Compass.Rotation.Up );

				NeedleBone.WorldRotation = NorthNeedleRotation;
			}
		}
	}
}
