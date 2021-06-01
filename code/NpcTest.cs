using Lab;
using Sandbox;
using System;
using System.Linq;
using System.Threading.Tasks;

[Library( "npc_test", Title = "Npc Test", Spawnable = true )]
public partial class NpcTest : AnimEntity
{

	[ServerCmd( "npc_clear" )]
	public static void NpcClear( )
	{
		foreach ( var npc in Entity.All.OfType<NpcTest>().ToArray() )
			npc.Delete();
	}

	[ServerCmd( "npc_speed" )]
	public static void NpcSpeed( int i )
	{
		if ( i > 0 ) NpcMoveSpeed = 5;
		else NpcMoveSpeed = 1;
	}

	static float NpcMoveSpeed = 1.0f;

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/citizen/citizen.vmdl" );
		EyePos = Position + Vector3.Up * 64;
		CollisionGroup = CollisionGroup.Player;
		SetupPhysicsFromCapsule( PhysicsMotionType.Keyframed, Capsule.FromHeightAndRadius( 72, 16 ) );

		EnableHitboxes = true;
		Think();

		this.SetMaterialGroup( Rand.Int( 0, 3 ) );
	}

	bool Walking;
	float Speed;
	public Vector3 TargetPosition;

	TimeSince TimeSinceThink = 10;

	[Event.Tick.Server]
	public void Tick()
	{
		if ( TimeSinceThink > 0.1f )
		{
			TimeSinceThink = 0;
			Think();
		}

		var target = TargetPosition;
		var targetDelta = (target - Position);
		if ( targetDelta.Length < 5.0f )
			TimeSinceThink = 1;

	//	DebugOverlay.Line( Position, TargetPosition, 0.1f );

		if ( !Walking )
		{
			SetAnimFloat( "wishspeed", 0.0f );
			return;
		}

		var direction = (target - Position).Normal;	
		Velocity += direction * Speed * Time.Delta * 10;
		if ( Velocity.Length > 100 )
			Velocity = Velocity.Normal * 100;

		Move( Time.Delta );

		var walkVelocity = Velocity.WithZ( 0 );
		if ( walkVelocity.Length > 1 )
		{
			var turnSpeed = walkVelocity.Length.LerpInverse( 0, 100, true );
			var targetRotation = Rotation.LookAt( walkVelocity.Normal, Vector3.Up );
			Rotation = Rotation.Lerp( Rotation, targetRotation, turnSpeed * Time.Delta * 10 );
		}

		SetAnimBool( "b_grounded", true );
		SetAnimBool( "b_noclip", false );
		SetAnimBool( "b_swim", false );
		SetAnimFloat( "forward", Vector3.Dot( Rotation.Forward, direction ) );
		SetAnimFloat( "sideward", Vector3.Dot( Rotation.Right, direction ) );
		SetAnimFloat( "wishspeed", Speed );
		SetAnimFloat( "walkspeed_scale", 2.0f / 190.0f );
		SetAnimFloat( "runspeed_scale", 2.0f / 320.0f );
		SetAnimFloat( "duckspeed_scale", 2.0f / 80.0f );
	}

	protected virtual void Move( float timeDelta )
	{
		if ( Velocity.IsNearlyZero() )
			return;

		var targetPos = Position + Velocity * timeDelta;

		var bbox = new BBox( (Vector3.One * -16).WithZ( 0 ), (Vector3.One * 16).WithZ( 64 ) );
	//	DebugOverlay.Box( Position, bbox.Mins, bbox.Maxs, Color.Green );

		MoveHelper move = new( Position, Velocity );
		move.Trace = move.Trace.Ignore( this ).Size( bbox.Mins, bbox.Maxs );

		if ( !Velocity.IsNearlyZero() )
		{
			move.TryUnstuck();

			move.TryMove( timeDelta );

			// We didn't get where we wanted to go, lets try stepping up
			if ( !targetPos.IsNearlyEqual( move.Position, 0.1f ) )
			{
				var stepMove = move;
				stepMove.Velocity = Velocity;
				stepMove.Position = Position;

				float StepSize = 20.0f;
				var stepTrace = stepMove.TraceDirection( Vector3.Up * StepSize );
				if ( !stepTrace.Hit )
				{
					stepMove.Position = stepTrace.EndPos;
					stepMove.TryMove( timeDelta );

					var distBump = targetPos.Distance( move.Position.WithZ( targetPos.z ) );
					var distStep = targetPos.Distance( stepMove.Position.WithZ( targetPos.z ) );

					if ( distStep < distBump )
					{
						stepTrace = stepMove.TraceDirection( Vector3.Down * StepSize );
						if ( stepTrace.Hit )
						{
							stepMove.Position = stepTrace.EndPos;
							move = stepMove;
						}
					}
				}
				DebugOverlay.Text( Position, "BUMP", 1.0f );
			}
		}

	//	DebugOverlay.Box( Position + Time.Delta * Velocity, bbox.Mins, bbox.Maxs, Color.Blue );

		var tr = move.TraceDirection( Vector3.Down * 2 );

		if ( move.IsFloor( tr ) )
		{
			GroundEntity = tr.Entity;
			move.ApplyFriction( tr.Surface.Friction * 8.0f, timeDelta );
		}
		else
		{
			GroundEntity = null;
			move.Velocity += Vector3.Down * 900 * timeDelta;
		}

		Position = move.Position;
		Velocity = move.Velocity;
	}

	void Think()
	{
		Walking = false;

		var closestPlayer = Entity.All.OfType<BaseLabPawn>().OrderBy( x => Vector3.DistanceBetween( Position, x.Position ) ).FirstOrDefault();
		if ( closestPlayer == null ) return;

		SetAnimLookAt( "lookat_pos", closestPlayer.EyePos );
		SetAnimLookAt( "aimat_pos", closestPlayer.EyePos );
		SetAnimFloat( "aimat_weight", 0.5f );

		var distFromPlayer = (closestPlayer.Position - Position).Length;
		if ( distFromPlayer < 100 ) return;

		Speed = (distFromPlayer - 50) * 0.5f;
		if ( Speed > 300 ) Speed = 300;

		var path = NavMesh.BuildPath( Position, closestPlayer.Position );

		if ( path == null )
		{
			DebugOverlay.Text( EyePos, "Path was NULL", 0.1f );
			return;
		}

		var lastPoint  = Position;
		foreach ( var point in path )
		{
			DebugOverlay.Line( lastPoint, point, 0.5f );
			lastPoint = point;
		}

		if ( path != null && path.Length > 1 )
		{
			TargetPosition = path[1];

			if ( TargetPosition.Distance( Position ) < 10 && path.Length > 2 )
			{
				TargetPosition = path[2];
			}

			Walking = true;
		}
	}

}
