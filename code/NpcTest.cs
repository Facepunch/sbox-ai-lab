using Lab;
using Sandbox;
using System;
using System.Diagnostics;
using System.IO;
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

	float Speed;

	NavPath Path = new NavPath();
	public NavSteer Steer;

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/citizen/citizen.vmdl" );
		EyePos = Position + Vector3.Up * 64;
		CollisionGroup = CollisionGroup.Player;
		SetupPhysicsFromCapsule( PhysicsMotionType.Keyframed, Capsule.FromHeightAndRadius( 72, 16 ) );

		EnableHitboxes = true;

		this.SetMaterialGroup( Rand.Int( 0, 3 ) );

		new ModelEntity( "models/citizen_clothes/trousers/trousers.smart.vmdl", this );
		new ModelEntity( "models/citizen_clothes/jacket/labcoat.vmdl", this );
		new ModelEntity( "models/citizen_clothes/shirt/shirt_longsleeve.scientist.vmdl" , this );

		if ( Rand.Int(3) == 1 )
		{
			new ModelEntity( "models/citizen_clothes/hair/hair_femalebun.black.vmdl" , this );
		}
		else if ( Rand.Int( 10 ) == 1 )
		{
			new ModelEntity( "models/citizen_clothes/hat/hat_hardhat.vmdl" , this );
		}

		SetBodyGroup( 1, 0 );

		Speed = Rand.Float( 100, 300 );
	}

	public Sandbox.Debug.Draw Draw => Sandbox.Debug.Draw.Once;

	[Event.Tick.Server]
	public void Tick()
	{
		using var _a = Sandbox.Debug.Profile.Scope( "NpcTest::Tick" );

		if ( Steer != null )
		{
			using var _b = Sandbox.Debug.Profile.Scope( "Steer" );

			Steer.Tick( Position );

			if ( !Steer.Output.Finished && GroundEntity != null )
			{
				Velocity += Steer.Output.Direction.WithZ( 0 ).Normal * Speed * Time.Delta * 5;

				using ( Sandbox.Debug.Profile.Scope( "Set Anim Vars" ) )
				{
					SetAnimLookAt( "lookat_pos", EyePos + Steer.Output.Direction.WithZ( 0 ) * 10 );
					SetAnimLookAt( "aimat_pos", EyePos + Steer.Output.Direction.WithZ( 0 ) * 10 );
					SetAnimFloat( "aimat_weight", 0.5f );
				}

				var p = Position + Vector3.Up * 5;
				Draw.WithColor( Color.Yellow ).Arrow( p, p + (Velocity.Normal) * 100, Vector3.Up, 10.0f );
			}
		}

		//if ( Velocity.Length > Speed )
		//	Velocity = Velocity.Normal * Speed;

		using ( Sandbox.Debug.Profile.Scope( "Move" ) )
		{
			Move( Time.Delta );
		}

		var walkVelocity = Velocity.WithZ( 0 );
		if ( walkVelocity.Length > 1 )
		{
			var turnSpeed = walkVelocity.Length.LerpInverse( 0, 100, true );
			var targetRotation = Rotation.LookAt( walkVelocity.Normal, Vector3.Up );
			Rotation = Rotation.Lerp( Rotation, targetRotation, turnSpeed * Time.Delta * 3 );
		}

		using ( Sandbox.Debug.Profile.Scope( "Set Anim Vars" ) )
		{
			SetAnimBool( "b_grounded", true );
			SetAnimBool( "b_noclip", false );
			SetAnimBool( "b_swim", false );
			SetAnimFloat( "forward", Vector3.Dot( Rotation.Forward, Velocity ) );
			SetAnimFloat( "sideward", Vector3.Dot( Rotation.Right, Velocity ) );
			SetAnimFloat( "wishspeed", Speed );
			SetAnimFloat( "walkspeed_scale", 2.0f / 10.0f );
			SetAnimFloat( "runspeed_scale", 2.0f / 320.0f );
			SetAnimFloat( "duckspeed_scale", 2.0f / 80.0f );
		}
		
	}

	protected virtual void Move( float timeDelta )
	{
		var bbox = BBox.FromHeightAndRadius( 64, 10 );
	//	DebugOverlay.Box( Position, bbox.Mins, bbox.Maxs, Color.Green );

		MoveHelper move = new( Position, Velocity );
		move.MaxStandableAngle = 80;
		move.Trace = move.Trace.Ignore( this ).Size( bbox.Mins, bbox.Maxs );

		if ( !Velocity.IsNearlyZero( 0.01f ) )
		{
			using ( Sandbox.Debug.Profile.Scope( "TryUnstuck" ) )
				move.TryUnstuck();

			using ( Sandbox.Debug.Profile.Scope( "TryMoveWithStep" ) )
				move.TryMoveWithStep( timeDelta, 30 );
		}

		using ( Sandbox.Debug.Profile.Scope( "Ground Checks" ) )
		{
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
		}

		Position = move.Position;
		Velocity = move.Velocity;
	}
}
