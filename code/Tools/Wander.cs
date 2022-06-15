using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lab.Tools
{
	[Library( "tool_wander" )]
	public class Wander : Base
	{
		public override void OnClick( TraceResult tr, IList<Entity> selected )
		{
			if ( !Host.IsServer ) return;

			foreach ( var ent in selected )
			{
				if ( ent is NpcTest npc )
				{
					var wander = new Sandbox.Nav.Wander();
					wander.MinRadius = 500;
					wander.MaxRadius = 2000;
					npc.Steer = wander;

					if ( !wander.FindNewTarget( npc.Position ) )
					{
						DebugOverlay.Text("COULDN'T FIND A WANDERING POSITION!", npc.EyePosition, 5.0f );
					}
				}
			}
		}
	}
}
