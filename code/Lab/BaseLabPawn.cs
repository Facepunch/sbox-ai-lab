
using Sandbox;

namespace Lab
{
	public partial class BaseLabPawn : Sandbox.AnimatedEntity
	{

		[ClientInput]
		public Vector3 CursorPosition { get; set; }
		[ClientInput]
		public Vector3 CursorForward { get; set; }
		[ClientInput] 
		public Vector3 InputDirection { get; set; }
		[ClientInput] 
		public Angles ViewAngles { get; set; }

		public override void Spawn()
		{
			base.Spawn();

			Tags.Add( "player", "pawn" );
			SetModel( "models/light_arrow.vmdl" );
		}

		public override void Simulate( IClient cl )
		{
			base.Simulate( cl );

			var maxSpeed = 500;
			if ( Input.Down( InputButton.Run ) ) maxSpeed = 1000;

			Velocity += Rotation.From( ViewAngles ) * new Vector3( InputDirection.x, InputDirection.y, InputDirection.z ) * maxSpeed * 5 * Time.Delta;
			if ( Velocity.Length > maxSpeed ) Velocity = Velocity.Normal * maxSpeed;

			Velocity = Velocity.Approach( 0, Time.Delta * maxSpeed * 3 );
			Position += Velocity * Time.Delta;

			if ( Game.IsClient )
			{
				Game.RootPanel.SetClass( "driving", Input.Down( InputButton.SecondaryAttack ) );
			}
		}

		public override void BuildInput()
		{
			base.BuildInput();

			InputDirection = Input.AnalogMove;
			CursorPosition = Camera.Position;
			CursorForward = Screen.GetDirection( Mouse.Position );

			if ( !Input.Down( InputButton.SecondaryAttack ) )
				return;

			var look = Input.AnalogLook;

			if ( ViewAngles.pitch > 90f || ViewAngles.pitch < -90f )
			{
				look = look.WithYaw( look.yaw * -1f );
			}

			var viewAngles = ViewAngles;
			viewAngles += look;
			viewAngles.pitch = viewAngles.pitch.Clamp( -89f, 89f );
			viewAngles.roll = 0f;
			ViewAngles = viewAngles.Normal;
		}

		public override void FrameSimulate( IClient cl )
		{
			base.FrameSimulate( cl );

			Position += Velocity * Time.Delta;
		}
	}

}
