
using Sandbox;
using System.Collections.Generic;

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
			IsDragging = false;
		}

		internal void Update( Ray ray )
		{
			EndRay = ray;

			IsDragging = Vector3.DistanceBetween( StartRay.Project( 100 ), EndRay.Project( 100 ) ) > 5.0f;
		}

		public bool IsDragging { get; internal set; }

		internal Frustum GetFrustum( float znear = 0, float zfar = float.MaxValue )
		{
			var left = Rotation.Left;
			var up = Rotation.Up;

			var rayA = StartRay.Project( 100 );
			var rayB = EndRay.Project( 100 );

			Frustum f = new Frustum();

			var forward = (StartRay.Forward + EndRay.Forward).Normal;

			if ( left.Dot( (rayA - rayB).Normal ) < 0 )
			{
				f.LeftPlane = new Plane( EndRay.Position, EndRay.Forward.Cross( up ) );
				f.RightPlane = new Plane( StartRay.Position, StartRay.Forward.Cross( -up ) );
			}
			else
			{
				f.LeftPlane = new Plane( StartRay.Position, StartRay.Forward.Cross( up ) );
				f.RightPlane = new Plane( EndRay.Position, EndRay.Forward.Cross( -up ) );
			}

			if ( up.Dot( (rayA - rayB).Normal ) < 0 )
			{
				f.TopPlane = new Plane( EndRay.Position, EndRay.Forward.Cross( -left ) );
				f.BottomPlane = new Plane( StartRay.Position, StartRay.Forward.Cross( left ) );
			}
			else
			{
				f.TopPlane = new Plane( StartRay.Position, StartRay.Forward.Cross( -left ) );
				f.BottomPlane = new Plane( EndRay.Position, EndRay.Forward.Cross( left ) );
			}

			f.NearPlane = new Plane( (StartRay.Position + forward * znear), forward );
			f.FarPlane = new Plane( (StartRay.Position + forward * zfar), -forward );

			return f;
		}
	}

	public partial class LabPawn : BaseLabPawn
	{
		[ConVar.ClientData( "lab_toolmode" )]
		public string ToolMode { get; set; } = "npc";

		public FrustumSelect FrustumSelect = new FrustumSelect();

		Tools.Base CurrentTool;

		[Net]
		public IList<Entity> Selected { get; set; }

		public override void Simulate( IClient cl )
		{
			base.Simulate( cl );

			var toolName = $"tool_{ToolMode}";

			if ( CurrentTool == null || CurrentTool.ClassName != toolName )
			{
				CurrentTool = TypeLibrary.Create<Tools.Base>( toolName );
				CurrentTool.Owner = this;
			}

			var cursorRay = new Ray( CursorPosition, CursorForward );

			if ( Input.Pressed( InputButton.PrimaryAttack ) )
			{
				FrustumSelect.Init( cursorRay, Rotation.From ( ViewAngles ) );
			}

			if ( Input.Down( InputButton.PrimaryAttack ) )
			{
				FrustumSelect.Update( cursorRay );

				if ( FrustumSelect.IsDragging )
				{
					Selected.Clear();

					var f = FrustumSelect.GetFrustum();

					foreach ( var ent in Entity.All )
					{
						if ( !ent.Tags.Has( "selectable" ) ) continue;
						if ( !f.IsInside( ent.WorldSpaceBounds, true ) ) continue;

						Selected.Add( ent );
					}
				}
			}

			for( int i = Selected.Count - 1; i>= 0; i-- )
			{
				if ( Selected[i].IsValid() ) continue;
				Selected.RemoveAt( i );
			}

			var tr = Trace.Ray( Position, Position + CursorForward * 10000 )
							.Ignore( this )
							.WorldOnly()
							.Run();

			if ( !FrustumSelect.IsDragging )
			{
				CurrentTool?.Tick( tr, Selected );
			}

			if ( Input.Released( InputButton.PrimaryAttack ) && !FrustumSelect.IsDragging )
			{
				CurrentTool?.OnClick( tr, Selected );
			}

			foreach( var selected in Selected )
			{
				if ( !selected.IsValid() ) continue;

				Sandbox.Debug.Draw.Once.WithColor( Color.Cyan )
					.Circle( selected.Position, Vector3.Up, 50.0f );
			}

			if ( !Input.Down( InputButton.PrimaryAttack ) )
				FrustumSelect.IsDragging = false;
		}
	}

}
