using Lab.Tools;
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

			if ( up.Dot( (rayA - rayB).Normal ) < 0 )
			{
				f.TopPlane = new Plane( EndRay.Origin, EndRay.Direction.Cross( -left ) );
				f.BottomPlane = new Plane( StartRay.Origin, StartRay.Direction.Cross( left ) );
			}
			else
			{
				f.TopPlane = new Plane( StartRay.Origin, StartRay.Direction.Cross( -left ) );
				f.BottomPlane = new Plane( EndRay.Origin, EndRay.Direction.Cross( left ) );
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

		public FrustumSelect FrustumSelect = new FrustumSelect();

		Tools.Base CurrentTool;

		[Net]
		public List<Entity> Selected { get; set; }

		public override void Simulate( Client cl )
		{
			base.Simulate( cl );

			var toolName = $"tool_{ToolMode}";

			if ( CurrentTool == null || CurrentTool.ClassInfo.Name != toolName )
			{
				CurrentTool = Library.Create<Tools.Base>( toolName );
				CurrentTool.Owner = this;
			}

			if ( Input.Pressed( InputButton.Attack1 ) )
			{
				FrustumSelect.Init( Input.Cursor, EyeRot );
			}

			if ( Input.Down( InputButton.Attack1 ) )
			{
				FrustumSelect.Update( Input.Cursor );

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

			Selected.RemoveAll( x => !x.IsValid() );

			var tr = Trace.Ray( EyePos, EyePos + Input.Cursor.Direction * 10000 )
							.Ignore( this )
							.WithAllTags( "world" )
							.Run();

			if ( !FrustumSelect.IsDragging )
			{
				CurrentTool?.Tick( tr, Selected );
			}

			if ( Input.Released( InputButton.Attack1 ) && !FrustumSelect.IsDragging )
			{
				CurrentTool?.OnClick( tr, Selected );
			}

			foreach( var selected in Selected )
			{
				if ( !selected.IsValid() ) continue;

				Sandbox.Debug.Draw.Once.WithColor( Color.Cyan )
					.Circle( selected.Position, Vector3.Up, 50.0f );
			}

			if ( !Input.Down( InputButton.Attack1 ) )
				FrustumSelect.IsDragging = false;
		}
	}

}
