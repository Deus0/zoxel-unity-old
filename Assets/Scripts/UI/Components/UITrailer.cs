using Unity.Entities;
using Unity.Mathematics;

namespace Zoxel
{

    public struct UITrailer : IComponentData
    {
        public Entity character;
        public float3 position;
        public float heightAddition;
    }
}