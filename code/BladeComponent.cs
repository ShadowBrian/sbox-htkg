using Sandbox;

namespace htkgttt;
public partial class BladeComponent : EntityComponent
{
	public AnimatedEntity trap;

	int bladecount = 0;

	protected override void OnActivate()
	{
		base.OnActivate();
		trap = Entity as AnimatedEntity;

		for ( int i = 0; i < 10; i++ )
		{
			if(trap.GetAttachment("blade"+ i ).HasValue )
			{
				bladecount++;
			}
			else
			{
				break;
			}
		}
	}

	[Event.Tick.Client]
	public void OnTick()
	{
		for ( int i = 0; i < bladecount; i++ )
		{
			Transform bladeAttachment = trap.GetAttachment( "blade" + i, true ).Value;
			//DebugOverlay.Line( bladeAttachment.Position + bladeAttachment.Rotation.Forward *50f, bladeAttachment.Position - bladeAttachment.Rotation.Forward * 50f, Time.Delta, false );

			TraceResult bladeresult = Trace.Ray( bladeAttachment.Position + bladeAttachment.Rotation.Forward * 50f, bladeAttachment.Position + bladeAttachment.Rotation.Backward * 50f ).Run();

			if(bladeresult.Entity is Pawn player )
			{
				player.HitBlade();
			}

			if ( bladeresult.Entity is King king )
			{
				king.HitBlade();
			}
		}
		
	}
}
