using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Transforms;
using Unity.Burst;

namespace Zoxel
{
    /// <summary>
    /// Add force to bodies
    /// </summary>
    [DisableAutoCreation]
    public class PositionalForceSystem : JobComponentSystem
    {
        [BurstCompile]
        struct ForceJob : IJobForEach<BodyForce, Translation, Rotation>
        {
            [ReadOnly]
            public float deltaTime;

            public void Execute(ref BodyForce body, ref Translation position, ref Rotation rotation)
            {
                float decreaseForce = 0.5f; // 0.95f;
                if (body.acceleration.x != 0)
                {
                    body.worldVelocity.x += body.acceleration.x;
                    body.acceleration.x *= decreaseForce;
                }
                if (body.acceleration.y != 0)
                {
                    body.worldVelocity.y += body.acceleration.y;
                    body.acceleration.y *= decreaseForce;
                }
                if (body.acceleration.z != 0)
                {
                    body.worldVelocity.z += body.acceleration.z;
                    body.acceleration.z *= decreaseForce;
                }
                body.velocity.x += body.localAcceleration.x;
                body.velocity.y += body.localAcceleration.y;
                body.velocity.z += body.localAcceleration.z;
                if (body.velocity.x != 0 || body.velocity.y != 0 || body.velocity.z != 0)
                {
                    float3 newTranslation = position.Value;
                    float3 rotatedVelocity = math.mul(rotation.Value, body.velocity * deltaTime);
                    //if (body.grounded == 1)
                    {
                    //    rotatedVelocity.y = 0;
                    }
                    newTranslation += rotatedVelocity;
                    position.Value = newTranslation;
                }
                if (body.worldVelocity.x != 0 || body.worldVelocity.y != 0 || body.worldVelocity.z != 0)
                {
                    float3 newTranslation = position.Value;
                    newTranslation += body.worldVelocity * deltaTime;
                    position.Value = newTranslation;
                    body.worldVelocity = float3.zero;
                }
                body.localAcceleration = float3.zero;
            }
        }
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return new ForceJob { deltaTime = UnityEngine.Time.deltaTime }.Schedule(this, inputDeps);
        }
    }
}