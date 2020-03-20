using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Transforms;
using Unity.Burst;


namespace Zoxel
{
    [UpdateAfter(typeof(PositionalForceSystem))]
    [DisableAutoCreation]
    public class RotationalForceSystem : JobComponentSystem
    {
        [BurstCompile]
        struct ForceJob : IJobForEach<BodyTorque, Translation, Rotation>
        {
            [ReadOnly]
            public float deltaTime;

            public void Execute(ref BodyTorque body, ref Translation position, ref Rotation rotation)
            {
                /*body.velocity += body.torque / (4 * 360);
                body.angle += body.velocity;
                body.torque = float3.zero;
                body.velocity *= 0.8f;
                rotation.Value = quaternion.Euler(body.angle);*/
                //rotation.Value = quaternion.Euler(body.torque);
            }
        }
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return new ForceJob { deltaTime = UnityEngine.Time.deltaTime }.Schedule(this, inputDeps);
        }
    }
}