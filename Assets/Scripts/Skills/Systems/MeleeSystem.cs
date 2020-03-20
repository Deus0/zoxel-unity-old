using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Transforms;

/// <summary>
/// Flagged for rewrite - Due to using Referencing to lists of character entities
/// </summary>

namespace Zoxel
{
    /// <summary>
    /// Check if hits other body
    ///     check angle of hit
    ///     check timing
    ///     check animations
    /// </summary>
    [DisableAutoCreation]
    public class MeleeDamageSystem : JobComponentSystem
    {

        [BurstCompile]
        struct MeleeJob : IJobForEach<MeleeAttack, Targeter, Translation, Rotation>
        {
            [ReadOnly]
            public float time;

            public void Execute(ref MeleeAttack attack, ref Targeter targeter, ref Translation position, ref Rotation rotator)
            {
                if (attack.triggered == 1)
                {
                    if (targeter.hasTarget == 1 && targeter.nearbyCharacter.distance <= targeter.Value.attackRange * 1.5f)
                    {
                        // begins attack animation (todo)
                        if (time - attack.lastAttacked >= attack.attackCooldown)
                        {
                            targeter.currentAngle = rotator.Value;
                            attack.lastAttacked = time;
                            attack.didHit = 1;
                            attack.triggered = 0;
                            // check angle
                            //float targetAngle = Unity.Mathematics.math.dot(position.Value, targeter.targetPosition);
                            /*targeter.currentAngle = UnityEngine.Vector3.Angle(targeter.targetAngle,
                                new UnityEngine.Quaternion(
                                rotator.Value.value.x,
                                rotator.Value.value.y,
                                rotator.Value.value.z,
                                rotator.Value.value.w).ToEulerAngles());*/
                            /*float angle = new UnityEngine.Quaternion(
                                rotator.Value.value.x, 
                                rotator.Value.value.y, 
                                rotator.Value.value.z, 
                                rotator.Value.value.w).ToEulerAngles().y;
                            if (angle >= targetAngle - 45 && angle <= targetAngle + 45)*/
                            //if (targeter.currentAngle <= 30)
                            //if (targeter.targetAngle.y)
                            //if (targeter.currentAngle.value.y >= targeter.targetAngle.value.y - 45 
                            //    && targeter.currentAngle.value.y <= targeter.targetAngle.value.y + 45)
                            {
                            }
                        }
                    }
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return new MeleeJob{ time = UnityEngine.Time.time }.Schedule(this, inputDeps);
        }
    }
}