using Sandbox;
using Sandbox.UI;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lab
{

	public class FrustumSelect
	{
		Rotation Rotation;
		Ray StartRay;
		Ray EndRay;


		internal void Init( Ray ray, Rotation rotation )
		{
			StartRay = ray;
			Rotation = rotation;
		}

		internal void Update( Ray ray )
		{
			EndRay = ray;
		}

		internal Frustum GetFrustum( float znear = 0, float zfar = float.MaxValue )
		{
			var left = Rotation.Left;
			var up = Rotation.Up;

			var rayA = StartRay.Origin + StartRay.Direction * 100;
			var rayB = EndRay.Origin + EndRay.Direction * 100;

			Frustum f = new Frustum();

			var forward = (StartRay.Direction + EndRay.Direction).Normal;

			if ( left.Dot( (rayA - rayB).Normal ) < 0 )
			{
				f.LeftPlane = new Plane( EndRay.Origin, EndRay.Direction.Cross( up ) );
				f.RightPlane = new Plane( StartRay.Origin, StartRay.Direction.Cross( -up ) );
			}
			else
			{
				f.LeftPlane = new Plane( StartRay.Origin, StartRay.Direction.Cross( up ) );
				f.RightPlane = new Plane( EndRay.Origin, EndRay.Direction.Cross( -up ) );
			}

			if ( up.Dot( (rayA - rayB).Normal ) > 0 )
			{
				f.TopPlane = new Plane( EndRay.Origin, EndRay.Direction.Cross( left ) );
				f.BottomPlane = new Plane( StartRay.Origin, StartRay.Direction.Cross( -left ) );
			}
			else
			{
				f.TopPlane = new Plane( StartRay.Origin, StartRay.Direction.Cross( left ) );
				f.BottomPlane = new Plane( EndRay.Origin, EndRay.Direction.Cross( -left ) );
			}

			f.NearPlane = new Plane( (StartRay.Origin + forward * znear), forward );
			f.FarPlane = new Plane( (StartRay.Origin + forward * zfar), -forward );

			return f;
		}
	}

	public partial class LabPawn : BaseLabPawn
	{
		[ConVar.ClientData( "lab_toolmode" )]
		public string ToolMode { get; set; } = "npc";

		FrustumSelect FrustumSelect = new FrustumSelect();

		[Net]
		public List<Entity> Selected { get; set; }

		public override void Simulate( Client cl )
		{
			base.Simulate( cl );

			if ( Input.Pressed( InputButton.Attack1 ) )
			{
				FrustumSelect.Init( new Ray( EyePos, Input.CursorAim ), EyeRot );
			}

			if ( Input.Down( InputButton.Attack1 ) )
			{
				FrustumSelect.Update( new Ray( EyePos, Input.CursorAim ) );
				Selected.Clear();

				var f = FrustumSelect.GetFrustum();

				foreach ( var ent in Entity.All )
				{
					if ( ent is not NpcTest ) continue;
					if ( !f.IsInside( ent.WorldSpaceBounds, true ) ) continue;

					Selected.Add( ent );
				}
			}

			foreach( var selected in Selected )
			{
				if ( !selected.IsValid() ) continue;

				Sandbox.Debug.Draw.Once.WithColor( Color.Cyan )
					.Circle( selected.Position, Vector3.Up, 50.0f );
			}

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
