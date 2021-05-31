
using Sandbox;
using Sandbox.UI.Construct;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Lab
{
	[Library( "lab" )]
	public partial class Game : Sandbox.Game
	{
		public override void Spawn()
		{
			base.Spawn();

			new HudEntity();
		}

		/// <summary>
		/// Client joined, create them a LabPawn and spawn them
		/// </summary>
		public override void ClientJoined( Client client )
		{
			base.ClientJoined( client );

			client.Pawn = new LabPawn();
			MoveToSpawnpoint( client.Pawn );
		}

		/// <summary>
		/// Don't do any in game input unless we're holding down RMB
		/// </summary>
		public override void BuildInput( InputBuilder input )
		{
			if ( !input.Down( InputButton.Attack2 ) )
				return;

			base.BuildInput( input );
		}

		/// <summary>
		/// Put the camera at the pawn's eye
		/// </summary>
		public override CameraSetup BuildCamera( CameraSetup camSetup )
		{
			camSetup.Rotation = Local.Client.Pawn.EyeRot;
			camSetup.Position = Local.Client.Pawn.EyePos;

			return base.BuildCamera( camSetup );
		}

	}

}
