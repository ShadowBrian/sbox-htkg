using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace htkgttt
{
	[GameResource( "Maze Wall Tileset", "htkgwall", "A resource that holds the wall data" )]
	public class MazeWallResource : GameResource
	{
		[Category( "Walls" ), ResourceType( "vmdl" ), Description( "List of different wall models to spawn in the maze" )]
		public List<string> Walls { get; set; }

		[Category( "Walls" ), ResourceType( "vmdl" ), Description( "List of different wall models to spawn in the maze with passable geometry (open doors, broken walls etc.)" )]
		public List<string> PassableWalls { get; set; }

		[Category( "Floors" ), ResourceType( "vmdl" ), Description( "List of different floor models to spawn in the maze" )]
		public List<string> FloorModels { get; set; }
	}
}
