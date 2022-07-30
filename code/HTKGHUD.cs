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
		Label TimerLabel, BestLabel;

		TimeSince RebootMapTimer;

		bool TriggeredMapReboot;

		public HTKGHUD()
		{
			StyleSheet.Load( "/HTKGHUD.scss" );
			AddChild<VoiceList>();
			AddChild<ChatBox>();
			TimerLabel = Add.Label( "timerthing", "Title" );
			BestLabel = Add.Label( "No best score yet.", "Title2" );

			WaitingForScores();
			
		}

		public async Task WaitingForScores()
		{
			LeaderboardResult results = await GameServices.Leaderboard.Query( ident: Global.GameIdent, bucket: Global.MapName );

			if ( results.Entries.Count > 0 )
			{
				TimeSpan t = TimeSpan.FromSeconds( float.Parse( results.Entries.First().Rating.ToString() ) );

				string answer = string.Format( "{0:D2}h:{1:D2}m:{2:D2}s:{3:D3}ms",
								t.Hours,
								t.Minutes,
								t.Seconds,
								t.Milliseconds );

				BestLabel.Text = "Global Best: " + answer;
			}
			else
			{
				BestLabel.Text = "No best global score yet.";
			}

			results = await GameServices.Leaderboard.Query( ident: Global.GameIdent, bucket: Global.MapName, playerid: Local.PlayerId );

			if ( results.Entries.Count > 0 )
			{
				TimeSpan t = TimeSpan.FromSeconds( float.Parse( results.Entries.First().Rating.ToString() ) );

				string answer = string.Format( "{0:D2}h:{1:D2}m:{2:D2}s:{3:D3}ms",
								t.Hours,
								t.Minutes,
								t.Seconds,
								t.Milliseconds );

				BestLabel.Text += "\nPersonal Best: " + answer;
			}
			else
			{
				BestLabel.Text += "\nNo personal best score yet.";
			}
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

			TimerLabel.Text = "Timer: " + answer;

			if( (Game.Current as HTKGGame).WonGame )
			{
				TimerLabel.Text += "\nWinner: " + (Game.Current as HTKGGame).WinningPlayer;
				if(!TriggeredMapReboot )
				{
					RebootMapTimer = 0;
					TriggeredMapReboot = true;
				}
				TimerLabel.Text += "\nRestarting map in " + MathF.Round(MathF.Abs(RebootMapTimer - 10f)) + " seconds.";
			}
		}
	}
}
