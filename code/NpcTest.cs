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
		Log.Info( "Clear Shit" );

		var npcs = Entity.All.OfType<NpcTest>().ToArray();

		foreach ( var npc in npcs )
			npc.Delete();
	}

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/citizen/citizen.vmdl" );
		EyePos = Position + Vector3.Up * 64;

		EnableHitboxes = true;
		Think();
	}

	bool Walking;
	float Speed;
	public Vector3 TargetPosition;

	TimeSince TimeSinceThink = 10;

	[Event.Tick.Server]
	public void Tick()
	{
		if ( TimeSinceThink  > 1.0f )
		{
			TimeSinceThink = 0;
			Think();
		}

		var target = TargetPosition;
		var targetDelta = (target - Position);
		if ( targetDelta.Length < 5.0f )
			TimeSinceThink = 1;

		if ( !Walking )
		{
			SetAnimFloat( "wishspeed", 0.0f );
			return;
		}

		var direction = (target - Position).Normal;
		Velocity = direction * Speed * Time.Delta;
		Position += Velocity;

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

		Rotation = Rotation.LookAt( (closestPlayer.Position - Position).WithZ(0).Normal, Vector3.Up );

		var path = NavMesh.BuildPath( Position, closestPlayer.Position );
		if ( path  != null && path.Length > 1 )
		{

			TargetPosition = path[1];

			if ( TargetPosition.Distance( Position ) < 10 && path.Length > 2 )
			{
				TargetPosition = path[2];
			}

			var ents = Physics.GetEntitiesInSphere( TargetPosition, 40 );
			foreach ( var ent in ents )
			{
				if ( ent == this ) continue;
				if ( (Position - ent.Position).Length > 30 ) continue;

				if ( ent is NpcTest )
				{
					var diff = (Position - ent.Position).WithZ( 0 ).Normal;
					TargetPosition += diff * 15 + Vector3.Random.WithZ( 0 );
				}
			}

			Walking = true;
		}
	}

}
