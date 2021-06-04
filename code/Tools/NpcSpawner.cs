using Sandbox;
using Sandbox.UI;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lab.Tools
{
	[Library( "tool_npc" )]
	public class NpcSpawner : Base
	{
		public override void OnClick( TraceResult tr, IList<Entity> selected )
		{
			if ( !Host.IsServer ) return;

			new NpcTest
			{
				Position = tr.EndPos,
				Rotation = Rotation.LookAt( Owner.EyeRot.Backward.WithZ( 0 ) )
			};
		}
	}
}
