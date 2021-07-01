using Sandbox;
using System;

public struct CitizenAnimationHelper
{
	AnimEntity Owner;

	public CitizenAnimationHelper( AnimEntity entity )
	{
		Owner = entity;
	}

	public void WithLookAt( Vector3 look )
	{
		Owner.SetAnimLookAt( "aim_eyes", look );
		Owner.SetAnimLookAt( "aim_head", look );
		Owner.SetAnimLookAt( "aim_body", look );
		Owner.SetAnimFloat( "aimat_weight", 0.5f );
	}

	public void WithVelocity( Vector3 Velocity )
	{
		var dir = Velocity;
		var forward = Owner.Rotation.Forward.Dot( dir );
		var sideward = Owner.Rotation.Right.Dot( dir );

		var angle = MathF.Atan2( sideward, forward ).RadianToDegree().NormalizeDegrees();

		Owner.SetAnimFloat( "move_direction", angle );
		Owner.SetAnimFloat( "move_speed", Velocity.Length );
		Owner.SetAnimFloat( "move_groundspeed", Velocity.WithZ( 0 ).Length );
		Owner.SetAnimFloat( "move_y", sideward );
		Owner.SetAnimFloat( "move_x", forward );
	}

	public void WithWishVelocity( Vector3 Velocity )
	{
		var dir = Velocity;
		var forward = Owner.Rotation.Forward.Dot( dir );
		var sideward = Owner.Rotation.Right.Dot( dir );

		var angle = MathF.Atan2( sideward, forward ).RadianToDegree().NormalizeDegrees();

		Owner.SetAnimFloat( "wish_direction", angle );
		Owner.SetAnimFloat( "wish_speed", Velocity.Length );
		Owner.SetAnimFloat( "wish_groundspeed", Velocity.WithZ( 0 ).Length );
		Owner.SetAnimFloat( "wish_y", sideward );
		Owner.SetAnimFloat( "wish_x", forward );
	}
}
