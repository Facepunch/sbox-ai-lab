﻿using Sandbox.UI;
using Sandbox;

namespace Lab
{
	public static class LabTools
	{
		[ConCmd.Server( "spawn_at" )]
		public static void Spawn( Vector3 vector, Rotation rot, string entity )
		{
			var owner = ConsoleSystem.Caller?.Pawn;

			if ( ConsoleSystem.Caller == null )
				return;

			var ent = TypeLibrary.Create<Entity>( entity );
			ent.Position = vector;
			ent.Rotation = Rotation.LookAt( rot.Forward.WithZ( 0 ) );
		}
	}
}
