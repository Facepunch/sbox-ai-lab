using Lab;
using Sandbox;
using System;
using System.Collections.Generic;
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

	[Event.Tick.Server]
	public void Tick()
	{
		if ( Steer != null )
		{
			Steer.Tick( Position );

			if ( !Steer.Output.Finished && GroundEntity != null )
			{
				Velocity += Steer.Output.Direction.WithZ( 0 ).Normal * Speed * Time.Delta * 5;

				SetAnimLookAt( "lookat_pos", EyePos + Steer.Output.Direction.WithZ( 0 ) * 10 );
				SetAnimLookAt( "aimat_pos", EyePos + Steer.Output.Direction.WithZ( 0 ) * 10 );
				SetAnimFloat( "aimat_weight", 0.5f );
			}
		}

		//if ( Velocity.Length > Speed )
		//	Velocity = Velocity.Normal * Speed;

		Move( Time.Delta );

		var walkVelocity = Velocity.WithZ( 0 );
		if ( walkVelocity.Length > 1 )
		{
			var turnSpeed = walkVelocity.Length.LerpInverse( 0, 100, true );
			var targetRotation = Rotation.LookAt( walkVelocity.Normal, Vector3.Up );
			Rotation = Rotation.Lerp( Rotation, targetRotation, turnSpeed * Time.Delta * 3 );
		}

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

	protected virtual void Move( float timeDelta )
	{
		var targetPos = Position + Velocity * timeDelta;

		var bbox = BBox.FromHeightAndRadius( 64, 10 );
	//	DebugOverlay.Box( Position, bbox.Mins, bbox.Maxs, Color.Green );

		MoveHelper move = new( Position, Velocity );
		move.MaxStandableAngle = 80;
		move.Trace = move.Trace.Ignore( this ).Size( bbox.Mins, bbox.Maxs );

		if ( !Velocity.IsNearlyZero() )
		{
			move.TryUnstuck();
			move.TryMoveWithStep( timeDelta, 30 );


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
//		SetAnimLookAt( "lookat_pos", closestPlayer.EyePos );
//		SetAnimLookAt( "aimat_pos", closestPlayer.EyePos );
	//	SetAnimFloat( "aimat_weight", 0.5f );

	}

}

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

public class NavSteer
{
	NavPath Path;

	public NavSteer()
	{
		Path = new NavPath();
	}

	internal void Tick( Vector3 position )
	{
		Path.Update( position, Target );

		Output.Finished = Path.IsEmpty;

		if ( Output.Finished )
			return;

		Output.Direction = (Output.Direction + Path.GetDirection( position )).Normal;

		//DebugOverlay.Line( position + Vector3.Up * 20, position + Vector3.Up * 20 + Output.Direction * 10 );

		Path.DebugDraw( 0.1f );
	}

	public Vector3 Target { get; set; }

	public NavSteerOutput Output;


	public struct NavSteerOutput
	{
		public bool Finished;
		public Vector3 Direction;
	}
}
