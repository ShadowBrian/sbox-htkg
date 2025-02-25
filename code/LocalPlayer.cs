using Sandbox;

public sealed class LocalPlayer : Component
{
	public static GameObject ThisPlayer;

	protected override void OnAwake()
	{
		base.OnAwake();
		if ( !IsProxy )
			ThisPlayer = GameObject;
	}
}
