using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Collections;
using System.Collections.Generic;
using Zoxel.Voxels;

namespace Zoxel.WorldGeneration
{

    /// <summary>
    /// Needs to take in meta data for a character before spawning
    /// </summary>
    [DisableAutoCreation]
    public class MonsterSpawnSystem : ComponentSystem
    {
        public CameraSystem cameraSystem;
        public CharacterSpawnSystem characterSpawnSystem;
        public WorldSpawnSystem worldSpawnSystem;

        private int3 FindNewPosition(ref Chunk chunk)
        {
            //float3 chunkPosition = chunk.Value.chunkPosition;
            var voxelDimensions = chunk.Value.voxelDimensions;
            // int worldID = chunk.worldID;
            // find first y position of value not air
            int voxelIndex;
            int3 checkVoxelPosition;
            int randomPositionX = (int)math.floor(UnityEngine.Random.Range(0, voxelDimensions.x - 1));
            int randomPositionZ = (int)math.floor(UnityEngine.Random.Range(0, voxelDimensions.z - 1));
            for (int j = (int)chunk.Value.voxelDimensions.y - 1; j >= 0; j--)
            {
                // if not air, pick j + 1
                checkVoxelPosition = new int3(randomPositionX, j, randomPositionZ);
                voxelIndex = VoxelRaycastSystem.GetVoxelArrayIndex(checkVoxelPosition, voxelDimensions);
                if (chunk.Value.voxels[voxelIndex] != 0)
                {
                    int3 newPosition = new int3(checkVoxelPosition.x, j + 1, checkVoxelPosition.z);
                    //return chunk.GetVoxelPosition() + newPosition;
                    return newPosition;
                }

            }
            Debug.LogError("No solid ground found for character spawning at.");
            return int3.Zero();
        }

        protected override void OnUpdate()
        {
            if (Bootstrap.instance == null || Bootstrap.instance.isMonsters == false)
            {
                return;
            }
            // get player position
            Entities.WithAll<Chunk, LocalToWorld, MonsterSpawnZone>().ForEach((Entity e, ref Chunk chunk, ref Biome biome, ref ChunkTown chunkTown,
                        ref LocalToWorld localToWorld, ref Translation translation, ref MonsterSpawnZone spawnZone) =>
            {
                float time = UnityEngine.Time.time;
                if (time - spawnZone.lastTimeSpawned >= spawnZone.spawnCooldown &&
                         biome.biomes.Length > 0 && World.EntityManager.HasComponent<WorldGenerationChunk>(e) == false)
                {
                    float3 cameraPosition = float3.zero;
                    cameraPosition.y = 0;
                    int previousWorldID = -1;

                    if (spawnZone.clanID == 0)
                    {
                        spawnZone.clanID = Bootstrap.GenerateUniqueID();
                    }

                    spawnZone.lastTimeSpawned = time;
                    //Debug.LogError("Spawning monsters now: " + translation.Value.ToString());
                    // check if all entities are dead
                    bool hasAliveOnes = false;
                    for (int i = 0; i < spawnZone.spawnedIDs.Length; i++)
                    {
                        if (characterSpawnSystem.characters.ContainsKey(spawnZone.spawnedIDs[i]))
                        {
                            hasAliveOnes = true;
                            break;
                        }
                    }
                    
                    if (!hasAliveOnes)
                    {
                        int worldID = chunk.worldID;
                        //cameraPosition = math.transform(math.inverse(localToWorld.Value), cameraPosition);
                        if (previousWorldID != worldID)
                        {
                            previousWorldID = worldID;
                            float4x4 worldTransform = World.EntityManager.GetComponentData<LocalToWorld>(worldSpawnSystem.worlds[worldID]).Value;
                            cameraPosition = math.transform(math.inverse(worldTransform), cameraSystem.GetMainCamera().transform.position); // math.inverse
                            cameraPosition.y = 0;
                        }
                        var voxelDimensions = chunk.Value.voxelDimensions;
                        float3 realChunkPosition = new float3(translation.Value.x + voxelDimensions.x / 2f, 0, translation.Value.z + voxelDimensions.z / 2f);// translation.Value;
                        realChunkPosition.y = 0;
                        float distanceToCamera = math.distance(cameraPosition, realChunkPosition);
                        //Debug.DrawLine(realChunkPosition, realChunkPosition + new float3(0, 64, 0), Color.red, 1);
                        //Debug.DrawLine(cameraPosition, cameraPosition + new float3(0, 64, 0), Color.blue, 1);
                        //Debug.DrawLine(realChunkPosition, realChunkPosition + new float3(0, 64, 0));
                        if (distanceToCamera <= chunk.Value.voxelDimensions.x + chunk.Value.voxelDimensions.z)
                        {
                            /*Debug.LogError("Close to cameraPosition: " + realChunkPosition.ToString() 
                                + " with cameraPosition: " + cameraPosition.ToString() 
                                + ":::" + distanceToCamera);*/

                            // spawn the things
                            List<int> newSpawned = new List<int>();
                            for (int i = 0; i < spawnZone.spawnAmount; i++)
                            {
                                float3 spawnPosition = FindNewPosition(ref chunk).ToFloat3();// realChunkPosition;
                                spawnPosition = math.transform(localToWorld.Value, spawnPosition);
                                if (!chunkTown.IsPointInsideOf(spawnPosition))
                                {
                                    //float3 spawnPosition = newPosition;// + new float3(UnityEngine.Random.Range(-8, 8), 0, UnityEngine.Random.Range(-8, 8));
                                    int positionXZ = (int)(spawnPosition.x * chunk.Value.voxelDimensions.z + spawnPosition.z);
                                    positionXZ = math.min(positionXZ, biome.biomes.Length - 1);
                                    int spawnBiomeType = (int)biome.biomes[positionXZ];
                                    if (spawnZone.spawnDatas.Length == 0)
                                    {
                                        //Debug.LogError("Monster Spawn Datas 0.");
                                        return;
                                    }
                                    if (spawnBiomeType >= spawnZone.spawnDatas.Length)
                                    {
                                        // Debug.LogError("Monster Spawn Datas is wrong: " + spawnBiomeType + ".");
                                        spawnBiomeType = 0;
                                    }
                                    if (spawnBiomeType < 0)
                                    {
                                        //Debug.LogError("Monster Spawn Datas Type is < 0.");
                                        spawnBiomeType = 0;
                                    }
                                    var biomeData = spawnZone.spawnDatas[spawnBiomeType];
                                    if (biomeData.monsterMetaID != 0)
                                    {
                                        newSpawned.AddRange(CharacterSpawnSystem.SpawnNPCs(
                                            World.EntityManager, worldID, biomeData.monsterMetaID, spawnZone.clanID, spawnPosition, 1));
                                    }
                                }
                            }
                            if (spawnZone.spawnedIDs.Length > 0)
                            {
                                spawnZone.spawnedIDs.Dispose();
                            }
                            spawnZone.spawnedIDs = new BlitableArray<int>(newSpawned.Count, Allocator.Persistent);
                            for (int i = 0; i < newSpawned.Count; i++)
                            {
                                spawnZone.spawnedIDs[i] = newSpawned[i];
                            }
                        }
                        //else
                        {
                           /* Debug.LogError("Not Close to cameraPosition: " + realChunkPosition.ToString() 
                                + " with cameraPosition: " + cameraPosition.ToString()
                                + ":::" + distanceToCamera);*/
                        }
                    }
                }
            });
        }
    }
}