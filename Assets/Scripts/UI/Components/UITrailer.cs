using Unity.Entities;
using Unity.Mathematics;

namespace Zoxel
{

    public struct UITrailer : IComponentData
    {
        public float3 position;
        public float heightAddition;
    }
}