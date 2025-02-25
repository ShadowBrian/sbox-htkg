using Sandbox;

namespace htkgttt;
public partial class LightCullComponent : EntityComponent
{
	public PointLightEntity light;

	float brightnessFade;

	Sound burningSound;

	protected override void OnActivate()
	{
		base.OnActivate();
		light = Entity as PointLightEntity;
		light.Color = Color.Orange;
		light.Brightness = 0.1f;
		light.Range = 256;

		light.DynamicShadows = false;

		//burningSound = Sound.FromEntity( "torch_burn", Entity );

		light.RenderDirty();
	}

	[Event.Tick.Client]
	public void OnTick()
	{
		if(light != null && Local.Pawn != null )
		{
			float dist = Vector3.DistanceBetween( light.Position, Local.Pawn.Position + Local.Pawn.Rotation.Forward * 128f );
			bool UnShadowed = Trace.Ray( light.Position, Local.Pawn.Position + Vector3.Up * 45f ).Ignore( Local.Pawn ).Ignore( (Local.Pawn as Pawn).AssociatedKing ).Run().Hit 
							&& Trace.Ray( light.Position, (Local.Pawn as Player).CameraMode.Position ).Ignore( Local.Pawn ).Ignore( (Local.Pawn as Pawn).AssociatedKing ).Run().Hit;

			//DebugOverlay.Line( light.Position, Local.Pawn.Position + Vector3.Up * 45f, UnShadowed ? Color.Red : Color.Green );

			if ( UnShadowed )
			{
				brightnessFade = 0f;
			}

			if( !UnShadowed || dist < 128 )
			{
				brightnessFade = 0.5f;
			}

			light.Brightness = MathX.Lerp( light.Brightness, brightnessFade, 0.5f * Time.Delta );

			if(light.Brightness <= 0.01f )
			{
				light.DynamicShadows = false;
			}
			else
			{
				light.DynamicShadows = true;
			}

			light.RenderDirty();
		}
	}
}
