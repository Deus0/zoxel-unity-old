using Unity.Entities;
using Zoxel.WorldGeneration;
using UnityEngine;

namespace Zoxel.Voxels
{

    [DisableAutoCreation]
    public class ChunkMapStarterSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities.WithAll<Chunk>().ForEach((Entity e, ref Chunk chunk) =>
            {
                if (chunk.isMapDirty == 1)
                {
                    chunk.isMapDirty = 0;
                    if (World.EntityManager.HasComponent<ChunkMap>(e))
                    {
                        ChunkMap builder = World.EntityManager.GetComponentData<ChunkMap>(e);
                        builder.dirty = 1;
                        if (Bootstrap.instance && Bootstrap.instance.isBiomeMaps)
                        {
                            builder.isBiome = 1;
                        }
                        World.EntityManager.SetComponentData(e, builder);
                    }
                    else
                    {
                        ChunkMap newMap = new ChunkMap { dirty = 1 };
                        if (Bootstrap.instance && Bootstrap.instance.isBiomeMaps)
                        {
                            newMap.isBiome = 1;
                        }
                        newMap.Initialize((int)chunk.Value.voxelDimensions.x, (int)chunk.Value.voxelDimensions.z);
                        newMap.highestHeight = (int)chunk.Value.voxelDimensions.y;
                        newMap.chunkPosition = chunk.Value.chunkPosition;
                        World.EntityManager.AddComponentData(e, newMap);
                    }
                }
            });
        }
    }
}
