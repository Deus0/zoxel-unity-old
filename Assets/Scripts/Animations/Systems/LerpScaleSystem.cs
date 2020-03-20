using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Transforms;
using Unity.Burst;

namespace Zoxel
{

    public struct ScaleLerper : IComponentData
    {
        public float createdTime;
        public float lifeTime;
        public float3 scaleBegin;
        public float3 scaleEnd;
        public float delay;
    }

    /// <summary>
    /// Add force to bodies
    /// </summary>
    [DisableAutoCreation]
    public class LerpScaleSystem : JobComponentSystem
    {
        [BurstCompile]
        struct ForceJob : IJobForEach<ScaleLerper, NonUniformScale>
        {
            [ReadOnly]
            public float time;

            public void Execute(ref ScaleLerper lerper, ref NonUniformScale scale)
            {
                if (time - lerper.createdTime >= lerper.delay)
                {
                    scale.Value = math.lerp(lerper.scaleBegin, lerper.scaleEnd,
                        (time - lerper.createdTime - lerper.delay) / lerper.lifeTime);
                }
            }
        }
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return new ForceJob { time = UnityEngine.Time.time }.Schedule(this, inputDeps);
        }
    }
}