using Sandbox;

public sealed class GroundThrone : Component
{
	protected override void OnStart()
	{
		var tr = Scene.Trace.Ray( WorldPosition, WorldPosition - Vector3.Up * 256 ).Run();
		WorldPosition = tr.EndPosition;
	}
}
