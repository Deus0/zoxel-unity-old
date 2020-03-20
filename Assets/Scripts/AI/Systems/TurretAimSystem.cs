using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Transforms;
using Unity.Burst;

/// <summary>
/// Flagged for rewrite - Due to using Referencing to lists of character entities
/// </summary>


namespace Zoxel
{
    /// <summary>
    /// This is just to set shooter to triggered? 
    /// </summary>
    [DisableAutoCreation]
    public class AimSystem : JobComponentSystem
    {
        public static float turretTurnSpeed = 1.5f;

        [BurstCompile]
		struct TurretJob : IJobForEach<Aimer, Shooter, Targeter, Translation, Rotation> //  Stats, 
        {
			[ReadOnly]
			public float time;
			[ReadOnly]
			public float deltaTime;

            public void Execute(ref Aimer aimer, ref Shooter shooter, ref Targeter targeter, ref Translation position, ref Rotation rotation)
            {
				if (targeter.hasTarget == 1)
                {
                    float3 positionValue = position.Value;
                    float3 targetPosition = targeter.nearbyCharacter.position + new float3(0, 0.1f, 0);
                    float3 angle = math.normalizesafe(targetPosition - positionValue);
                    ////float3 upAngle = new float3(0, 1, 0);// math.cross(targetPosition, targetPosition);
                    //aimer.targetRotation = Quaternion.Euler(angle);// quaternion.LookRotationSafe(angle, upAngle);
                    aimer.targetRotation = quaternion.LookRotationSafe(angle, new float3(0,1,0));
                    //aimer.targetRotation = quaternion.LookRotationSafe(angle, math.normalizesafe(math.cross(targetPosition, positionValue)));
                    shooter.triggered = 1;
                }
                else
                {
                    //turret.targetRotation = quaternion.EulerXYZ(new float3(90, 0, 0));
                    shooter.triggered = 0;
                }
				rotation.Value = QuaternionHelpers.slerpSafe(rotation.Value, aimer.targetRotation, deltaTime * aimer.turnSpeed);
                position.Value = aimer.originalPosition + math.mul(aimer.targetRotation, new float3(0, 0, aimer.offsetZ / 4f));
                shooter.shootPosition = aimer.originalPosition + math.mul(aimer.targetRotation, new float3(0, 0, aimer.offsetZ + 0.2f));
                shooter.shootRotation = rotation.Value;
            }
		}

		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			TurretJob job = new TurretJob
			{
				time = UnityEngine.Time.time,
				deltaTime = UnityEngine.Time.deltaTime * turretTurnSpeed
			};
			JobHandle handle = job.Schedule(this, inputDeps);
			return handle;
		}
	}
}