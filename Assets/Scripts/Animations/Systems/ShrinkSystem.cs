using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Transforms;
using Unity.Burst;

namespace Zoxel
{
    public struct ShrinkComponent : IComponentData
    {
        // who fired it ID!
        public byte placeholder;
    }

    //[ReadOnly]

    [DisableAutoCreation]
    public class ShrinkSystem2 : JobComponentSystem
    {
        [BurstCompile]
        struct ShrinkJob : IJobForEach<ShrinkComponent, Scale>
        {
            public float deltaTime;

            public void Execute(ref ShrinkComponent component, ref Scale scale)
            {
                scale.Value = math.lerp(scale.Value, 0, deltaTime);
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            ShrinkJob job = new ShrinkJob { deltaTime = UnityEngine.Time.deltaTime };
            JobHandle handle = job.Schedule(this, inputDeps);
            return handle;
        }
    }
    [DisableAutoCreation]
    public class ShrinkSystem3 : JobComponentSystem
    {
        [BurstCompile]
        struct ShrinkJob : IJobForEach<ShrinkComponent, NonUniformScale>
        {
            [ReadOnly]
            public float deltaTime;

            public void Execute(ref ShrinkComponent component, ref NonUniformScale scale)
            {
                scale.Value = math.lerp(scale.Value, 0, deltaTime);
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            ShrinkJob job = new ShrinkJob { deltaTime = UnityEngine.Time.deltaTime };
            JobHandle handle = job.Schedule(this, inputDeps);
            return handle;
        }
    }
}