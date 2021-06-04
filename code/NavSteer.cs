public class NavSteer
{
	protected NavPath Path { get; private set; }

	public NavSteer()
	{
		Path = new NavPath();
	}

	public virtual void Tick( Vector3 currentPosition )
	{
		using ( Sandbox.Debug.Profile.Scope( "Update Path" ) )
		{
			Path.Update( currentPosition, Target ); 
		}

		Output.Finished = Path.IsEmpty;

		if ( Output.Finished )
			return;

		using ( Sandbox.Debug.Profile.Scope( "Update Direction" ) )
		{
			Output.Direction = (Output.Direction + Path.GetDirection( currentPosition )).Normal;
		}

		//DebugOverlay.Line( position + Vector3.Up * 20, position + Vector3.Up * 20 + Output.Direction * 10 );

		using ( Sandbox.Debug.Profile.Scope( "Path Debug Draw" ) )
		{
			Path.DebugDraw( 0.1f );
		}
	}

	public Vector3 Target { get; set; }

	public NavSteerOutput Output;


	public struct NavSteerOutput
	{
		public bool Finished;
		public Vector3 Direction;
	}
}
