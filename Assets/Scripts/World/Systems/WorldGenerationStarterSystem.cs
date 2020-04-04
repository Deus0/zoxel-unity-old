using Unity.Entities;
using Zoxel.WorldGeneration;
using UnityEngine;
using Zoxel.Voxels;
using Unity.Mathematics;
using Unity.Collections;
using System.Collections.Generic;

namespace Zoxel.WorldGeneration
{
    public struct WorldGenerationChunk : IComponentData
    {
        public byte state;
    }

    [DisableAutoCreation]
    public class WorldGenerationStarterSystem : ComponentSystem
    {
        public WorldSpawnSystem worldSpawnSystem;

        protected override void OnUpdate()
        {
            Entities.WithAll<WorldGenerationChunk>().ForEach((Entity e, ref Chunk chunk, ref WorldGenerationChunk baby) =>
            {
                if (baby.state == 0)
                {
                    baby.state = 1;
                    chunk.isGenerating = 1;
                }
                else
                {
                    return;
                }
                int worldID = World.EntityManager.GetComponentData<ZoxID>(e).creatorID;
                MapDatam map = worldSpawnSystem.maps[worldID];

                Biome chunkBiome = new Biome { }; // World.EntityManager.GetComponentData<ChunkTerrain>(e);
                chunkBiome.biomes = new BlitableArray<byte>((int)(chunk.Value.voxelDimensions.x * chunk.Value.voxelDimensions.z), Allocator.Persistent);
                chunkBiome.blends = new BlitableArray<float>((int)(chunk.Value.voxelDimensions.x * chunk.Value.voxelDimensions.z), Allocator.Persistent);
                World.EntityManager.AddComponentData(e, chunkBiome);

                ChunkTerrain chunkTerrain = new ChunkTerrain { }; // World.EntityManager.GetComponentData<ChunkTerrain>(e);
                chunkTerrain.biomes = new BlitableArray<BiomeData>(map.biomes.Count, Allocator.Persistent);
                for (int i = 0; i < map.biomes.Count; i++)
                {
                    chunkTerrain.biomes[i] = map.biomes[i].Value;
                }
                chunkTerrain.chunkPosition = chunk.Value.chunkPosition;
                chunkTerrain.heights = new BlitableArray<int>((int)(chunk.Value.voxelDimensions.x * chunk.Value.voxelDimensions.z), Allocator.Persistent);
                World.EntityManager.AddComponentData(e, chunkTerrain);

                var chunkPosition = chunk.GetVoxelPosition();
                var dimensions = chunk.Value.voxelDimensions;
                float leftSideChunk = chunkPosition.x;
                float rightSideChunk = chunkPosition.x + dimensions.x;
                float leftSideChunkZ = chunkPosition.z;
                float rightSideChunkZ = chunkPosition.z + dimensions.z;
                // Use something like WorldTowns, and pass down relevant chunk information into the chunks
                List<Town> towns = new List<Town>();
                List<Building> buildings = new List<Building>();

                for (int i = 0; i < map.towns.Count; i++)
                {
                    float leftSideBuilding = map.towns[i].position.x - map.towns[i].dimensions.x;
                    float rightSideBuilding = map.towns[i].position.x + map.towns[i].dimensions.x;
                    float leftSideBuildingZ = map.towns[i].position.z - map.towns[i].dimensions.z;
                    float rightSideBuildingZ = map.towns[i].position.z + map.towns[i].dimensions.z;
                    if (!(leftSideBuilding > rightSideChunk || rightSideBuilding < leftSideChunk
                    || leftSideBuildingZ > rightSideChunkZ || rightSideBuildingZ < leftSideChunkZ))
                    {
                        towns.Add(map.towns[i]);
                        //for (int j = 0; j < map.buildings.Count; j++)
                        {
                            ///buildings.Add(map.buildings[j]);
                        }
                    }
                }
                // if building intersects chunk
                for (int i = 0; i < map.buildings.Count; i++)
                {
                    float leftSideBuilding = map.buildings[i].position.x - map.buildings[i].dimensions.x;
                    float rightSideBuilding = map.buildings[i].position.x + map.buildings[i].dimensions.x;
                    float leftSideBuildingZ = map.buildings[i].position.z - map.buildings[i].dimensions.z;
                    float rightSideBuildingZ = map.buildings[i].position.z + map.buildings[i].dimensions.z;
                    //Debug.DrawLine(new float3(leftSideBuilding))
                    if (!(leftSideBuilding > rightSideChunk || rightSideBuilding < leftSideChunk
                    || leftSideBuildingZ > rightSideChunkZ || rightSideBuildingZ < leftSideChunkZ))
                    {
                        buildings.Add(map.buildings[i]);
                    }
                }
                // for towns
                ChunkTown chunkTown = new ChunkTown { }; // World.EntityManager.GetComponentData<ChunkTown>(e);
                if (towns.Count > 0)
                {
                    chunkTown.towns = new BlitableArray<Town>(towns.Count, Allocator.Persistent);
                    for (int i = 0; i < towns.Count; i++)
                    {
                        chunkTown.towns[i] = towns[i];
                        //Debug.DrawLine(chunk.GetVoxelPosition() + new float3(8, 0, 8), chunk.GetVoxelPosition() + new float3(8, 64, 8),
                        //    Color.green, 30);
                        //Debug.LogError("Town added to chunk: " + chunk.Value.chunkPosition + " : " + towns[i].position);
                    }
                }
                if (buildings.Count > 0)
                {
                    chunkTown.buildings = new BlitableArray<Building>(buildings.Count, Allocator.Persistent);
                    for (int i = 0; i < buildings.Count; i++)
                    {
                        chunkTown.buildings[i] = buildings[i];
                        //Debug.DrawLine(chunk.GetVoxelPosition() + new float3(8, 0, 8), chunk.GetVoxelPosition() + new float3(8, 64, 8),
                        //    Color.red, 30);
                        //Debug.LogError("BUildings added to chunk: " + chunk.Value.chunkPosition + " : " + buildings[i].position);
                    }
                }
                World.EntityManager.AddComponentData(e, chunkTown);

                /* chunkTown.centrePosition = float3.zero;
                 chunkTown.wallSize = new float3(60, 8, 60);
                 chunkTown.wallThickness = 4f;*/
                /*chunkTown.buildings[0] = new Building
                {
                    position = new float3(-8, 0, -8),
                    dimensions = new float3(4, 4, 4)
                };
                chunkTown.buildings[1] = new Building
                {
                    position = new float3(8, 0, -8),
                    dimensions = new float3(4, 4, 4)
                };*/
                //if (map.towns.buildings)
                // monsters
                //if (biome.monsters.Count > 0)
                {
                    MonsterSpawnZone monsterSpawner = new MonsterSpawnZone { }; // World.EntityManager.GetComponentData<MonsterSpawnZone>(e);
                    monsterSpawner.lastTimeSpawned = UnityEngine.Time.time - 9;
                    monsterSpawner.spawnDatas = new BlitableArray<MonsterBiome>(map.biomes.Count, Allocator.Persistent);
                    for (int i = 0; i < monsterSpawner.spawnDatas.Length; i++)
                    {
                        int monsterMetaID = 0;
                        if (map.biomes[i].monsters.Count != 0)
                        {
                            monsterMetaID = map.biomes[i].monsters[UnityEngine.Random.Range(0, map.biomes[i].monsters.Count - 1)].Value.id;
                        }
                        monsterSpawner.spawnDatas[i] = new MonsterBiome
                        {
                            spawnAmount = map.biomes[i].monsterSpawnAmount,
                            spawnCooldown = map.biomes[i].monsterSpawnCooldown,
                            monsterMetaID = monsterMetaID
                        };
                    }
                    //monsterSpawner.monsterMetaID = biome.monsters[UnityEngine.Random.Range(0, biome.monsters.Count)].Value.id;
                    //monsterSpawner.spawnAmount = biome.monsterSpawnAmount;
                    //monsterSpawner.spawnCooldown = biome.monsterSpawnCooldown;
                    monsterSpawner.CalculateValues();
                    World.EntityManager.AddComponentData(e, monsterSpawner);
                }
            });
        }

    }
}
