
using Sandbox;

namespace Lab
{
	public partial class LabGame : GameManager
	{

		public override void ClientSpawn()
		{
			base.ClientSpawn();

			Game.RootPanel = new LabHud();
		}

		/// <summary>
		/// Client joined, create them a LabPawn and spawn them
		/// </summary>
		public override void ClientJoined( IClient client )
		{
			base.ClientJoined( client );

			var pawn = new LabPawn();
			client.Pawn = pawn;

			MoveToSpawnpoint( pawn );
		}

		[Event.Client.Frame]
		public void BuildCamera()
		{
			if ( Game.LocalPawn is not LabPawn p ) return;
			Camera.Rotation = Rotation.From( p.ViewAngles );
			Camera.Position = p.Position;
		}

	}

}
