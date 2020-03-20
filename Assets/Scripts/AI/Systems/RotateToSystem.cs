using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Transforms;
using Unity.Burst;

namespace Zoxel
{
    /// <summary>
    /// what to do after reaches target?
    /// </summary>
    [DisableAutoCreation, UpdateAfter(typeof(MoveToSystem))]
    public class RotateToSystem : JobComponentSystem
    {
        [BurstCompile]
        struct MoveToJob : IJobForEach<Mover, BodyTorque, Translation, Rotation> // AIState, Body, 
        {
            [ReadOnly]
            public float delta;

            public void Execute(ref Mover mover, ref BodyTorque bodyTorque, ref Translation position, ref Rotation rotation)    // ref AIState brain, , 
            {
                if (mover.disabled == 0)
                {
                    float3 normalBetween = math.normalizesafe(mover.target - position.Value);
                    quaternion targetAngle = quaternion.LookRotationSafe(0.05f * normalBetween, math.up());
                    quaternion newAngle = QuaternionHelpers.slerp(rotation.Value, targetAngle, delta);
                    Quaternion newAngle2 = new Quaternion(newAngle.value.x, newAngle.value.y, newAngle.value.z, newAngle.value.w);
                    rotation.Value = (newAngle2);
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return new MoveToJob { delta = UnityEngine.Time.deltaTime }.Schedule(this, inputDeps);
        }
    }
}
//bodyTorque.angle = Quaternion.Slerp(rotation.Value, targetAngle, delta).eulerAngles;


//bodyTorque.angularVelocity = targetAngle.value;// math.mul(math.inverse(rotation.Value), targetAngle);
//bodyTorque.angularVelocity = math.mul(math.inverse(rotation.Value), targetAngle);
//bodyTorque.angularVelocity = math.mul(rotation.Value, targetAngle);

//quaternion differenceQuaternion = math.mul(math.inverse(rotation.Value), targetAngle);
//bodyTorque.angularVelocity = differenceQuaternion;

//bodyTorque.angularVelocity = math.lerp(, differenceQuaternion);// (targetAngle - rotation.Value.value)

//bodyTorque.angle = Quaternion.Slerp(rotation.Value, targetAngle, delta);
//bodyTorque.angularVelocity = targetAngle;

//inv.inverse();
//return inv * b;
// Rotate towards point
//bodyTorque.angularVelocity = QuaternionHelpers.slerpSafe(rotation.Value, targetAngle, mover.turnSpeed);