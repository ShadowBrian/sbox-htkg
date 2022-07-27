using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace htkgttt
{
	[UseTemplate]
	public partial class HTKGHUD : RootPanel
	{
		Label TimerLabel;
		public HTKGHUD()
		{
			StyleSheet.Load( "/HTKGHUD.scss" );
			AddChild<VoiceList>();
			AddChild<ChatBox>();
			TimerLabel = Add.Label( "timerthing", "Title" );
		}

		public override void Tick()
		{
			base.Tick();
			
			TimeSpan t = TimeSpan.FromSeconds( (Game.Current as HTKGGame).CurrentTimerTime );

			string answer = string.Format( "{0:D2}h:{1:D2}m:{2:D2}s:{3:D3}ms",
							t.Hours,
							t.Minutes,
							t.Seconds,
							t.Milliseconds );

			TimerLabel.Text = "time since start: " + answer;
		}
	}
}
