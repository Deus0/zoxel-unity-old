using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Transforms;
using Unity.Burst;

namespace Zoxel
{
    [System.Serializable]
    public struct ItemBob : IComponentData
    {
        public float3 originalPosition;
        public float additionalY;
        public float sinMultiplier;
        public float timeScale;
    }

    [DisableAutoCreation]
    public class ItemBobSystem : JobComponentSystem
    {

        [BurstCompile]
        struct ItemBobJob : IJobForEach<ItemBob, Translation>
        {
            [ReadOnly]
            public float time;

            public void Execute(ref ItemBob item, ref Translation position)
            {
                float bobAmount = 0.06f;
                float bobSpeed = 1f;
                position.Value = item.originalPosition + new float3(0, item.additionalY + math.sin(time * bobSpeed) * bobAmount - bobAmount * 2f, 0);
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return new ItemBobJob { time = UnityEngine.Time.time }.Schedule(this, inputDeps);
        }
    }
}