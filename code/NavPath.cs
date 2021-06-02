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

		var a = Distance( 0, from );
		var b = Distance( 1, from );

		if ( ourdelta.Dot( deltaNormal ) > 0.7 )
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

		var deltaToPoint = (Points[1] - position);
		return (deltaToPoint).WithZ( 0 ).Normal; 
	}

	public void DebugDraw( float time )
	{
		int i = 0;
		var lastPoint = Vector3.Zero;
		foreach ( var point in Points )
		{
			DebugOverlay.Text( point + Vector3.Up * 10, $"{i}", time );
			DebugOverlay.Line( point, point + Vector3.Up * 10, time, true );

			if ( i > 0 )
			{
				DebugOverlay.Line( lastPoint, point, Color.Cyan, time, true );
			}

			lastPoint = point;
			i++;
		}
	}
}
