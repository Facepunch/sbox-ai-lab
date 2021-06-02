using Sandbox;
using System.Collections.Generic;

public class NavPath
{
	public Vector3 TargetPosition;
	public List<Vector3> Points = new List<Vector3>();

	public bool IsEmpty => Points.Count <= 1;

	public void Update( Vector3 from, Vector3 to )
	{
		bool needsBuild = false;

		if ( !TargetPosition.IsNearlyEqual( to, 5 ) )
		{
			TargetPosition = to;
			needsBuild = true;
		}

		if ( needsBuild )
		{
			Points.Clear();
			NavMesh.BuildPath( from, to, Points );
			Points.Add( NavMesh.GetClosestPoint( to ) );
		}

		if ( Points.Count <= 1 )
		{
			return;
		}

		var ourdelta = from - Points[1];
		var delta = Points[1] - Points[0];
		var deltaNormal = delta.Normal;

		if ( ourdelta.Length < 10 )
		{
			Points.RemoveAt( 0 );
			return;
		}

		// If we're in front of this line then
		// remove it and move on to next one
		if ( ourdelta.Dot( deltaNormal ) >= 0.5f )
		{
			Points.RemoveAt( 0 );
		}
	}

	public float Distance( int point, Vector3 from )
	{
		if ( Points.Count <= point ) return float.MaxValue;

		return Points[point].WithZ( from.z ).Distance( from );
	}

	public Vector3 GetDirection( Vector3 position )
	{
		if ( Points.Count == 1 )
		{
			return (Points[0] - position).WithZ(0).Normal;
		}

		return (Points[1] - position).WithZ( 0 ).Normal; 
	}

	public void DebugDraw( float time )
	{
		var draw = Sandbox.Debug.Draw.ForSeconds( time );
		var lift = Vector3.Up * 10;

		draw.WithColor( Color.White ).Circle( lift + TargetPosition, Vector3.Up, 20.0f );

		int i = 0;
		var lastPoint = Vector3.Zero;
		foreach ( var point in Points )
		{
			//draw.WithColor( Color.Cyan ).Circle( point, Vector3.Up, 10 );
			//DebugOverlay.Text( point + Vector3.Up * 10, $"{i}", time );
			//DebugOverlay.Line( point, point + Vector3.Up * 10, time, true );

			if ( i > 0 )
			{
				draw.WithColor( i == 1 ? Color.Green : Color.Cyan ).Arrow( lastPoint + lift, point + lift, Vector3.Up, 5.0f );
			}

			lastPoint = point;
			i++;
		}
	}
}
