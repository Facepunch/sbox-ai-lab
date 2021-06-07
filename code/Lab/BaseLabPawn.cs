using Sandbox;
using Sandbox.UI;

namespace Lab
{
	public partial class BaseLabPawn : Sandbox.AnimEntity
	{
		public override void Spawn()
		{
			base.Spawn();

			Tags.Add( "player", "pawn" );
			SetModel( "models/light_arrow.vmdl" );
		}

		public override void Simulate( Client cl )
		{
			base.Simulate( cl );

			Rotation = Input.Rotation;
			EyeRot = Rotation;

			var maxSpeed = 500;
			if ( Input.Down( InputButton.Run ) ) maxSpeed = 1000;

			Velocity += Input.Rotation * new Vector3( Input.Forward, Input.Left, Input.Up ) * maxSpeed * 5 * Time.Delta;
			if ( Velocity.Length > maxSpeed ) Velocity = Velocity.Normal * maxSpeed;

			Velocity = Velocity.Approach( 0, Time.Delta * maxSpeed * 3 );

			Position += Velocity * Time.Delta;

			EyePos = Position;

			if ( IsClient )
			{
				Local.Hud.SetClass( "driving", Input.Down( InputButton.Attack2 ) );
			}
		}

		public override void FrameSimulate( Client cl )
		{
			base.FrameSimulate( cl );

			Rotation = Input.Rotation;
			EyeRot = Rotation;
			Position += Velocity * Time.Delta;
		}
	}

}
