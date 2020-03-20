using Unity.Entities;
using Zoxel.WorldGeneration;
using UnityEngine;

namespace Zoxel.Voxels
{

    [DisableAutoCreation]
    public class WorldGenerationCompleterSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities.WithAll<WorldGenerationChunk, Chunk>().ForEach((Entity e, ref WorldGenerationChunk worldGenerationChunk, ref Chunk chunk) =>
            {
                if (worldGenerationChunk.state == 5)
                {
                    if (ChunkSpawnSystem.isDebugLog)
                    {
                        Debug.LogError("Chunk is dirty at: " + chunk.Value.chunkPosition + "::" + chunk.Value.voxelDimensions);
                    }
                    ChunkTown chunkTown = World.EntityManager.GetComponentData<ChunkTown>(e);
                    if (chunkTown.buildings.Length > 0)
                    {
                        CharacterSpawnSystem.SpawnNPC(World.EntityManager, chunk.worldID, chunkTown.buildings[0].characterID, chunkTown.buildings[0].position);
                    }
                    /*if ((Bootstrap.instance == null || !Bootstrap.instance.isBiomeMaps)
                        && World.EntityManager.HasComponent<Biome>(e))
                    {
                        World.EntityManager.RemoveComponent<Biome>(e);
                    }
                    if (World.EntityManager.HasComponent<ChunkTown>(e))
                    {
                        World.EntityManager.RemoveComponent<ChunkTown>(e);
                    }*/
                    if (World.EntityManager.HasComponent<ChunkTerrain>(e))
                    {
                        World.EntityManager.RemoveComponent<ChunkTerrain>(e);
                    }
                    chunk.hasUpdatedBack = 0;
                    chunk.hasUpdatedDown = 0;
                    chunk.hasUpdatedForward = 0;
                    chunk.hasUpdatedLeft = 0;
                    chunk.hasUpdatedRight = 0;
                    chunk.hasUpdatedUp = 0;
                    chunk.isGenerating = 0;
                    if (chunk.chunkRenders.Length != 0)
                    {
                        if (World.EntityManager.HasComponent<ChunkBuilder>(e))
                        {
                            World.EntityManager.SetComponentData(e, new ChunkBuilder { });
                        }
                        else
                        {
                            World.EntityManager.AddComponentData(e, new ChunkBuilder { });
                        }
                    }
                    else
                    {
                       // World.EntityManager.AddComponentData(e, new WorldGenerationChunkPostponed { });
                    }
                    // for all characters, set their positions
                    
                    World.EntityManager.RemoveComponent<WorldGenerationChunk>(e);
                }
            });
        }
    }
}
