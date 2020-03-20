using Unity.Entities;
using Unity.Mathematics;

namespace Zoxel
{
    public struct PositionLerper : IComponentData
    {
        public float createdTime;
        public float lifeTime;
        public float3 positionBegin;
        public float3 positionEnd;
    }
}