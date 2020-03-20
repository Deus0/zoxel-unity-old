using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;

namespace Zoxel.Voxels
{
    public struct World : IComponentData
    {
        // links to what chunks are needed here!
        public int id;  // chunks all contain worldID for checking!
        public int3 centralPosition;
        public float3 size; // size of worlds - to spawn chunks
        public float3 scale;
        public int3 voxelDimensions; // 16 16 16
        public byte bounds;
        // meta
        public int mapID;
        public int modelID;
        public int skeletonID;

        //public NativeHashMap<float3, Entity> chunks;
        //public BlitableArray<Entity> chunks;
        public BlitableArray<int> chunkIDs;
        public BlitableArray<int3> chunkPositions;

        public void Dispose()
        {
            //if (chunks.Count() > 0) {
            //    chunks.Dispose();
            //}
        }
    }
}