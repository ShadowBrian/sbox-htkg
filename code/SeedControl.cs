using Sandbox;
using System;

public sealed class SeedControl : Component
{
	public static int Seed => (int)MathF.Floor( System.DateTime.Now.DayOfYear / 7f ) + System.DateTime.Now.Year + 3;
}
