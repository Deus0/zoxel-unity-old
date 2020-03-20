using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;

namespace Zoxel
{
    [DisableAutoCreation]
    public class SinRotatorSystem : JobComponentSystem
    {
        [BurstCompile]
        struct ForceJob : IJobForEach<SinRotator, Rotation>
        {
            [ReadOnly]
            public float time;

            public void Execute(ref SinRotator sinRotator, ref Rotation rotation)
            {
                rotation.Value = quaternion.Euler(new float3(0, 0, math.sin(sinRotator.multiplier * time)));
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return new ForceJob { time = UnityEngine.Time.time }.Schedule(this, inputDeps);
        }
    }
}
