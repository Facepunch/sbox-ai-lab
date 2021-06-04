using Sandbox;
using Sandbox.UI;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lab.Tools
{
	[Library( "tool_pathvis" )]
	public class PathVis : Base
	{
		public override void Tick( TraceResult tr, IList<Entity> selected )
		{
			base.Tick( tr, selected );



			foreach ( var ent in selected )
			{
				if ( ent is NpcTest npc )
				{
					var path = new NavPath();
					path.Update( npc.Position, tr.EndPos );
					path.DebugDraw( Time.Delta );
				}
			}
		}
	}
}
