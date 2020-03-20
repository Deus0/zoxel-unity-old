using Unity.Entities;
using Unity.Mathematics;
using System;

namespace Zoxel
{

    // finally the animator component i need to add to any monsters with animator data (timmy)
    [Serializable]
    public struct ChunkStreamPoint : IComponentData
    {
        public int worldID;
        public byte didUpdate;
        public int3 chunkPosition; // when position changes, update the position
        public int3 voxelDimensions;
    }
}
