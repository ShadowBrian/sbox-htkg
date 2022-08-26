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
	public partial class HTKGHUD : HudEntity<RootPanel>
	{
		Label TimerLabel, BestLabel;

		TimeSince RebootMapTimer;

		bool TriggeredMapReboot;

		[ConVar.Replicated( "hgtk_maze_seed" )]
		public static string hgtk_maze_seed { get; set; } = "";

		[ConVar.Replicated( "hgtk_use_seed" )]
		public static bool hgtk_use_seed { get; set; } = false;

		public int SeedToUse { get; set; } = 0;


		private static int GetIndexInAlphabet( char value )
		{
			// Uses the uppercase character unicode code point. 'A' = U+0042 = 65, 'Z' = U+005A = 90
			char upper = char.ToUpper( value );

			return (int)upper - (int)'A';
		}


		public HTKGHUD()
		{
			RootPanel.StyleSheet.Load( "/HTKGHUD.scss" );
			RootPanel.AddChild<VoiceList>();
			RootPanel.AddChild<ChatBox>();
			TimerLabel = RootPanel.Add.Label( "timerthing", "Title" );
			BestLabel = RootPanel.Add.Label( "No best score yet.", "Title2" );

			WaitingForScores();
		}

		public async Task WaitingForScores()
		{
			await Task.DelaySeconds( 1f );
			while( SeedToUse == 0 )
			{
				await Task.DelaySeconds( 1f );
				Log.Trace( "Waiting for seed...");
				SeedToUse = (Game.Current as HTKGGame).MazeSeed;
			}
			Log.Trace( "fetching scores for " + Global.MapName + SeedToUse );

			LeaderboardResult results = await GameServices.Leaderboard.Query( ident: Global.GameIdent, bucket: Global.MapName + SeedToUse );

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

			results = await GameServices.Leaderboard.Query( ident: Global.GameIdent, bucket: Global.MapName + SeedToUse, playerid: Local.PlayerId );

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

		[Event.Tick.Client]
		public void Tick()
		{

			TimeSpan t = TimeSpan.FromSeconds( (Game.Current as HTKGGame).CurrentTimerTime );

			string answer = string.Format( "{0:D2}h:{1:D2}m:{2:D2}s:{3:D3}ms",
							t.Hours,
							t.Minutes,
							t.Seconds,
							t.Milliseconds );

			TimerLabel.Text = "Timer: " + answer;

			if ( (Game.Current as HTKGGame).WonGame )
			{
				TimerLabel.Text += "\nWinner: " + (Game.Current as HTKGGame).WinningPlayer;
				if ( !TriggeredMapReboot )
				{
					RebootMapTimer = 0;
					TriggeredMapReboot = true;
				}
				TimerLabel.Text += "\nRestarting map in " + MathF.Round( MathF.Abs( RebootMapTimer - 20f ) ) + " seconds.";
			}
		}
	}
}
