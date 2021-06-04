﻿using Sandbox;
using Sandbox.UI;
using System.Linq;

namespace Lab
{
	public partial class LabPawn : BaseLabPawn
	{
		[ConVar.ClientData( "lab_toolmode" )]
		public string ToolMode { get; set; } = "npc";

		public override void Simulate( Client cl )
		{
			base.Simulate( cl );

			//GetClientOwner()?.GetUserString( "lab_toolmode" );  

			//var mode = cl.GetUserString( "lab_toolmode" );

			ScreenControls( ToolMode );
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


				case "wander":
					{
						if ( !Input.Pressed( InputButton.Attack1 ) || !IsServer )
							return;

						//DebugOverlay.Line( tr.EndPos, tr.EndPos + Vector3.Up * 200, Color.Red, 10.0f );

						foreach ( var npc in Entity.All.OfType<NpcTest>() )
						{
							var wander = new Sandbox.Nav.Wander();
							wander.MinRadius = 500;
							wander.MaxRadius = 2000;
							npc.Steer = wander;

							if ( !wander.FindNewTarget( npc.Position ) )
							{
								DebugOverlay.Text( npc.EyePos, "COULDN'T FIND A WANDERING POSITION!", 5.0f );
							}
						}

						break;
					}
			}


		}
	}

}
