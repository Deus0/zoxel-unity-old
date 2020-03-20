using Unity.Entities;

namespace Zoxel.Voxels
{
    public struct ChunkRendererBuilder : IComponentData
    {
        public byte state;
        //public float waitBegin;
    }
    public struct ChunkBuilder : IComponentData
    {
        public byte state;
        //public float waitBegin;
    }
}