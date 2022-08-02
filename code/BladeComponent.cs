using Sandbox;

namespace htkgttt;
public partial class BladeComponent : EntityComponent
{
	public AnimatedEntity trap;

	int bladecount = 0;

	bool ram;

	protected override void OnActivate()
	{
		base.OnActivate();
		trap = Entity as AnimatedEntity;

		ram = trap.Model.Name.Contains( "ram" );

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
			//DebugOverlay.Line( bladeAttachment.Position + bladeAttachment.Rotation.Forward *10f, bladeAttachment.Position, Time.Delta, false );

			if ( !ram )
			{
				TraceResult bladeresult = Trace.Ray( bladeAttachment.Position + bladeAttachment.Rotation.Forward * 50f, bladeAttachment.Position + bladeAttachment.Rotation.Backward * 50f ).Run();

				if ( bladeresult.Entity is Pawn player)
				{
					player.HitBlade();
				}

				if ( bladeresult.Entity is King king )
				{
					king.HitBlade();
				}
			}
			else
			{
				TraceResult bladeresult = Trace.Ray( bladeAttachment.Position + bladeAttachment.Rotation.Forward * 10f, bladeAttachment.Position ).Run();


				if ( bladeresult.Entity is Pawn player && player.GetAnimParameterFloat( "duck" ) <= 0.5f )
				{
					player.HitBlade();
				}

				if ( bladeresult.Entity is King king && king.GetAnimParameterFloat( "duck" ) <= 0.5f )
				{
					king.HitBlade();
				}
			}
		}
		
	}
}
