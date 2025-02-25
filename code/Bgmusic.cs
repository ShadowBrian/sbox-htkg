using Sandbox;

public sealed class Bgmusic : Component
{
	[Property] SoundEvent IntroMusic { get; set; }

	[Property] SoundEvent MainLoop { get; set; }

	SoundHandle MusicHandle { get; set; }

	protected override async void OnStart()
	{
		base.OnStart();
		if ( !Game.IsEditor )
		{
			MusicHandle = Sound.Play( IntroMusic );
			await Task.DelaySeconds( 8f );
			MusicHandle = Sound.Play( MainLoop );
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if ( MusicHandle.IsValid() )
		{
			MusicHandle.Dispose();
		}
	}
}
