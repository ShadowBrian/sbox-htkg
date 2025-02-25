using Sandbox;

namespace htkgttt;
public partial class LightCullComponent : Component
{
	public PointLight light;

	float brightnessFade;

	//Sound burningSound;

	protected override void OnEnabled()
	{
		base.OnEnabled();
		light = GetComponent<PointLight>();
		light.LightColor = Color.Orange;
		light.Attenuation = 0.1f;
		light.Radius = 256;

		light.Shadows = false;

		//burningSound = Sound.FromEntity( "torch_burn", Entity );

		//light.RenderDirty();
	}

	protected override void OnUpdate()
	{
		if ( light.IsValid() )
		{
			float dist = Vector3.DistanceBetween( light.WorldPosition, Scene.Camera.WorldPosition + Scene.Camera.WorldRotation.Forward * 128f );
			bool UnShadowed = Scene.Trace.Ray( light.WorldPosition, Scene.Camera.WorldPosition + Vector3.Up * 45f ).Run().Hit;
			//&& Scene.Trace.Ray( light.WorldPosition, (Local.Pawn as Player).CameraMode.Position ).Ignore( Local.Pawn ).Ignore( (Local.Pawn as Pawn).AssociatedKing ).Run().Hit;

			//DebugOverlay.Line( light.Position, Local.Pawn.Position + Vector3.Up * 45f, UnShadowed ? Color.Red : Color.Green );

			if ( UnShadowed )
			{
				brightnessFade = 0f;
			}

			if ( !UnShadowed || dist < 128 )
			{
				brightnessFade = 0.5f;
			}

			light.Attenuation = MathX.Lerp( light.Attenuation, brightnessFade, 0.5f * Time.Delta );

			if ( light.Attenuation <= 0.01f )
			{
				light.Shadows = false;
			}
			else
			{
				light.Shadows = true;
			}

			//light.RenderDirty();
		}
	}
}
