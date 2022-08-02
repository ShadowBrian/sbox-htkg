using System;
using Sandbox;
namespace htkgttt
{
	public class KingCamera : CameraMode
	{
		[ConVar.Replicated]
		public static bool thirdperson_orbit { get; set; } = false;

		[ConVar.Replicated]
		public static bool thirdperson_collision { get; set; } = true;

		private Angles orbitAngles;
		private float orbitDistance = 150;
		private float orbitHeight = 0;

		public bool checking_map = false;

		float mapmult;

		public override void Update()
		{
			if ( Local.Pawn is not AnimatedEntity pawn )
				return;

			Position = pawn.Position;
			Vector3 targetPos;

			var center = pawn.Position + Vector3.Up * 64;

			mapmult = MathX.Lerp( mapmult, checking_map ? 0f : 1f, 0.1f );

			if(mapmult <= 0.01f )
			{
				pawn.RenderColor = new Color( 1, 1, 1, 1f);
				pawn.SetBodyGroup( "head", 1 );
			}
			else
			{
				pawn.RenderColor = new Color( 1, 1, 1, mapmult );
				pawn.SetBodyGroup( "head", 0 );
				if ( (pawn as Pawn).Hat != null )
				{
					(pawn as Pawn).Hat.RenderColor = new Color( 1, 1, 1, mapmult );
				}
			}

			if ( thirdperson_orbit )
			{
				Position += Vector3.Up * ((pawn.CollisionBounds.Center.z * pawn.Scale) + orbitHeight);
				Rotation = Rotation.From( orbitAngles );

				targetPos = Position + Rotation.Backward * orbitDistance;
			}
			else
			{
				Position = center;
				Rotation = Rotation.FromAxis( Vector3.Up, 4 ) * Input.Rotation;

				float distance = orbitDistance * pawn.Scale;
				if ( !checking_map ){
					targetPos = Position + Input.Rotation.Right * ((pawn.CollisionBounds.Maxs.x + 15) * pawn.Scale * mapmult);
					targetPos += Input.Rotation.Forward * -distance;
				}else
				{
					Rotation *= Rotation.FromPitch( 15f );
					targetPos = pawn.GetBoneTransform("head").Position + Input.Rotation.Right * ((pawn.CollisionBounds.Maxs.x + 15) * pawn.Scale * mapmult) 
						+ pawn.GetBoneTransform( "head" ).Rotation.Left * 10f 
						+ pawn.GetBoneTransform( "head" ).Rotation.Forward * 8f
						 + Input.Rotation.Forward * 8f;
					targetPos -= Vector3.Up * 64f * pawn.GetAnimParameterFloat( "duck" );
					targetPos += Input.Rotation.Forward * -distance;
				}
			}

			if ( thirdperson_collision )
			{
				var tr = Trace.Ray( Position, targetPos )
					.Ignore( pawn )
					.Ignore( (pawn as Pawn).AssociatedKing)
					.WithoutTags("kiltrigger")
					.Radius( 8 )
					.Run();


				if ( checking_map)
				{
					orbitDistance = MathX.Lerp( orbitDistance, 0f, 0.1f );
				}
				else
				{
					orbitDistance = MathX.Lerp( orbitDistance, 130f, 0.1f );
				}

				Position = Vector3.Lerp(Position, tr.EndPosition, 0.5f);
			}
			else
			{
				Position = Vector3.Lerp( Position, targetPos, 0.5f );
			}

			FieldOfView = 70;

			Viewer = null;
		}

		public override void BuildInput( InputBuilder input )
		{

			checking_map = Input.Down( InputButton.SecondaryAttack );

			if ( thirdperson_orbit && input.Down( InputButton.Walk ) )
			{
				if ( input.Down( InputButton.PrimaryAttack ) )
				{
					orbitDistance += input.AnalogLook.pitch;
					orbitDistance = orbitDistance.Clamp( 0, 1000 );
				}
				else if ( input.Down( InputButton.SecondaryAttack ) )
				{
					orbitHeight += input.AnalogLook.pitch;
					orbitHeight = orbitHeight.Clamp( -1000, 1000 );
				}
				else
				{
					orbitAngles.yaw += input.AnalogLook.yaw;
					orbitAngles.pitch += input.AnalogLook.pitch;
					orbitAngles = orbitAngles.Normal;
					orbitAngles.pitch = orbitAngles.pitch.Clamp( -89, 89 );
				}

				input.AnalogLook = Angles.Zero;

				input.Clear();
				input.StopProcessing = true;
			}

			base.BuildInput( input );
		}
	}
}
