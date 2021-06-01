using Sandbox;
using Sandbox.UI;
using System.Linq;

namespace Lab
{
	public partial class LabPawn : BaseLabPawn
	{
		[UserVar( "lab_toolmode" )]
		public static string ToolMode { get; set; } = "npc";

		public override void Simulate( Client cl )
		{
			base.Simulate( cl );

			var mode = cl.GetUserString( "lab_toolmode" );

			ScreenControls( mode );
		}

		void ScreenControls( string mode )
		{
			if ( Input.Down( InputButton.Attack2 ) )
				return;

			var tr = Trace.Ray( EyePos, EyePos + Input.CursorAim * 10000 )
							.Ignore( this )
							.Run();

			if ( !tr.Hit )
				return;

			switch ( mode )
			{
				case "npc":
					{
						if ( !Input.Pressed( InputButton.Attack1 ) || !IsServer )
							return;

						new NpcTest
						{
							Position = tr.EndPos,
							Rotation = Rotation.LookAt( EyeRot.Forward.WithZ( 0 ) )
						};

						return;
					}
					

				case "seek":
					{
						if ( !Input.Pressed( InputButton.Attack1 ) || !IsServer )
							return;

						//DebugOverlay.Line( tr.EndPos, tr.EndPos + Vector3.Up * 200, Color.Red, 10.0f );

						foreach ( var npc in Entity.All.OfType<NpcTest>() )
						{
							if ( npc.Steer is not NavSteer )
								npc.Steer = new NavSteer();

							npc.Steer.Target = tr.EndPos;
						}

						break;
					}
			}


		}
	}

}
