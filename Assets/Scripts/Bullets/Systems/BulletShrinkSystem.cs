using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Transforms;
using Unity.Burst;

namespace Zoxel
{
    public struct BulletShrink : IComponentData
    {
        public float timeBorn;
        public float lifetime;
        public int id;   // give bullet zoxID later
    }

    /// <summary>
    /// Simple animation system for bullets dying
    /// </summary>
    [DisableAutoCreation]
    public class BulletShrinkSystem : JobComponentSystem
    {
        [BurstCompile]
        struct ShrinkJob : IJobForEach<BulletShrink, Scale>
        {
            public float deltaTime;
            public void Execute(ref BulletShrink bullet, ref Scale scale)
            {
                scale.Value = math.lerp(scale.Value, 0, deltaTime * 0.6f);
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