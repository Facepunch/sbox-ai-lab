using Sandbox;
using Sandbox.UI;
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

			var npc = new NpcTest
			{
				Position = tr.EndPosition,
				Rotation = Rotation.LookAt( Owner.EyeRotation.Backward.WithZ( 0 ) )
			};

			npc.Tags.Add( "selectable" );
			npc.Tags.Add( "npc" );
		}
	}
}
