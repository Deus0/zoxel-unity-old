using Unity.Entities;

namespace Zoxel
{
    public struct SinRotator : IComponentData
    {
        public float timeBegun;
        public float multiplier;
    }
}