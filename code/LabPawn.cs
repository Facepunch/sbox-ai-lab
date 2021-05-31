using Sandbox;
using Sandbox.UI;

namespace Lab
{
	public partial class LabPawn : BaseLabPawn
	{
		public override void Simulate( Client cl )
		{
			base.Simulate( cl );

			if ( IsClient && !Input.Down( InputButton.Attack2 ) )
			{
				ScreenControls();
			}
		}

		void ScreenControls()
		{
			var tr = Trace.Ray( EyePos, EyePos + Input.CursorAim * 10000 )
							.Ignore( this )
							.Run();

			if ( !tr.Hit )
				return;

			if ( Input.Pressed( InputButton.Attack1 ) )
			{
				LabTools.Spawn( tr.EndPos, EyeRot, "npc_test" );
			}
		}
	}

}
