using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Transforms;
using Unity.Burst;

namespace Zoxel
{
    public struct PositionEntityLerper : IComponentData
    {
        public float createdTime;
        public float lifeTime;
        public float3 positionBegin;
        public Entity positionEnd;
    }
}