using Sandbox;
using System;

public sealed class AddSeedToLeaderboard : Component
{
	protected override void OnStart()
	{
		GetComponent<LeaderboardUI>().StatsName += SeedControl.Seed;
	}
}
