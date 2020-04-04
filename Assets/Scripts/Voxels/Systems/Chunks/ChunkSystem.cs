using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Rendering;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Zoxel.WorldGeneration;

namespace Zoxel.Voxels
{

    /// <summary>
    /// Spawn a world
    /// Worlds hold collections of chunks
    /// </summary>
    [DisableAutoCreation]
    public class ChunkSpawnSystem : ComponentSystem
    {
        // TODO: this number shoould be based on voxels used per chunk - just adding up max and divide by 2 or something
        public static int maxCacheVerts = 2 * 16384 / 1;
        public static int maxCacheTriangles = 2 * 16384;
        public readonly static bool isDebugLog = false;
        private EntityArchetype chunkArchtype;
        private Entity chunkPrefab;
        private Entity modelChunkPrefab;
        public Dictionary<int, Entity> chunks = new Dictionary<int, Entity>();
        // references
        public WorldSpawnSystem worldSpawnSystem;
        public VoxelSpawnSystem voxelSpawnSystem;
        public CharacterDeathSystem characterDeathSystem;
        
        public void Clear()
        {
            foreach (Entity e in chunks.Values)
            {
                Chunk.Destroy(World.EntityManager, e);
            }
            chunks.Clear();
        }

        protected override void OnCreate()
        {
            chunkArchtype = World.EntityManager.CreateArchetype(
                typeof(ZoxID),
                typeof(Chunk),
                typeof(WorldGenerationChunk),
                //typeof(ChunkTerrain),
                //typeof(ChunkTown),
                //typeof(MonsterSpawnZone),
                // transform
                typeof(Translation),
                typeof(Rotation),
                typeof(NonUniformScale),
                typeof(LocalToWorld),
                typeof(Parent),
                typeof(LocalToParent)
                // Generation Step - removed afterwards
            );
            // world chunk prefab
            chunkPrefab = World.EntityManager.CreateEntity(chunkArchtype);
            World.EntityManager.AddComponentData(chunkPrefab, new Prefab { });
            // model chunkp refab
            modelChunkPrefab = World.EntityManager.CreateEntity(chunkArchtype);
            World.EntityManager.RemoveComponent<WorldGenerationChunk>(modelChunkPrefab);
            World.EntityManager.AddComponentData(modelChunkPrefab, new Prefab { });
        }

        // Redo later while cycling through world chunk references rather then all worlds
        public void DestroyWorld(Entity worldEntity)
        {
            if (!World.EntityManager.Exists(worldEntity))
            {
                return;
            }
            List<int> removeIDs = new List<int>();
            World world = World.EntityManager.GetComponentData<World>(worldEntity);
            var worldID = World.EntityManager.GetComponentData<ZoxID>(worldEntity).id;
            //foreach (Entity e in chunks.Values)
            for (int i = 0; i < world.chunks.Length; i++)
            {
                var e = world.chunks[i];
                var chunk = World.EntityManager.GetComponentData<Chunk>(e);
                var chunkID = World.EntityManager.GetComponentData<ZoxID>(e).id;
                //if (chunk.worldID == worldID)
                //{
                    var lookupTable = worldSpawnSystem.worldLookups[worldID];
                    lookupTable.chunks.Remove(chunk.Value.chunkPosition);
                    worldSpawnSystem.worldLookups[worldID] = lookupTable;

                    removeIDs.Add(chunkID);
                    // chunk.renderIDs
                    if (World.EntityManager.HasComponent<MonsterSpawnZone>(e))
                    {
                        var monsterSpawner = World.EntityManager.GetComponentData<MonsterSpawnZone>(e);
                        for (int a = 0; a < monsterSpawner.spawnedIDs.Length; a++)
                        {
                            characterDeathSystem.DestroyCharacter(monsterSpawner.spawnedIDs[a]);
                        }
                    }

                    // for all chunkRenders (should be)
                    for (int a = 0; a < chunk.chunkRenders.Length; a++)
                    {
                        var chunkRender = chunk.chunkRenders[a];
                        ChunkRenderSystem.DestroyChunkRender(World.EntityManager, chunkRender);
                    }
                    if (World.EntityManager.Exists(e))
                    {
                        World.EntityManager.DestroyEntity(e);
                    }
                //}
            }
            foreach (int removeID in removeIDs)
            {
                chunks.Remove(removeID);
            }
        }

        public void RemoveChunks(ref World world,int[] chunkIDs, int3[] positions)
        {
            for (int i = 0; i < chunkIDs.Length; i++)
            {
                RemovePositionIndex(ref world, positions[i], chunkIDs[i], positions,  int3.Forward()); //new float3(0, 0, 1));
                RemovePositionIndex(ref world, positions[i], chunkIDs[i], positions,  int3.Back()); //new float3(0, 0, -1));
                RemovePositionIndex(ref world, positions[i], chunkIDs[i], positions, int3.Right()); // new float3(1, 0, 0));
                RemovePositionIndex(ref world, positions[i], chunkIDs[i], positions, int3.Left()); //new float3(-1, 0, 0));
                RemoveChunk(chunks[chunkIDs[i]], false);
            }
        }

        public void RemoveChunk(Entity chunkEntity, bool isRemoveFromWorld = true)
        {
            //if (chunks.ContainsKey(chunkID))
            {
                //Entity chunkEntity = chunks[chunkID];
                Chunk chunk = World.EntityManager.GetComponentData<Chunk>(chunkEntity);
                var chunkID = World.EntityManager.GetComponentData<ZoxID>(chunkEntity).id;
                var worldID = World.EntityManager.GetComponentData<ZoxID>(chunkEntity).creatorID;
                var lookupTable = worldSpawnSystem.worldLookups[worldID];
                lookupTable.chunks.Remove(chunk.Value.chunkPosition);
                worldSpawnSystem.worldLookups[worldID] = lookupTable;
                if (isRemoveFromWorld)
                {
                    Entity worldEntity = chunk.world;
                    World world = World.EntityManager.GetComponentData<World>(worldEntity);
                    int3[] positions = new int3[0];
                    var chunkPosition = chunk.Value.chunkPosition;
                    RemovePositionIndex(ref world, chunkPosition, chunkID, positions, int3.Forward()); //new float3(0, 0, 1));
                    RemovePositionIndex(ref world, chunkPosition, chunkID, positions, int3.Back()); //new float3(0, 0, -1));
                    RemovePositionIndex(ref world, chunkPosition, chunkID, positions, int3.Right()); // new float3(1, 0, 0));
                    RemovePositionIndex(ref world, chunkPosition, chunkID, positions, int3.Left()); //new float3(-1, 0, 0));
                    World.EntityManager.SetComponentData(worldEntity, world);
                }
                for (int j = 0; j < chunk.chunkRenders.Length; j++)
                {
                    Entity chunkRender = chunk.chunkRenders[j];
                    if (World.EntityManager.Exists(chunkRender))
                    {
                        ChunkRenderSystem.DestroyChunkRender(World.EntityManager, chunkRender);
                    }
                }
                if (World.EntityManager.HasComponent<MonsterSpawnZone>(chunkEntity))
                {
                    MonsterSpawnZone monsterSpawner = World.EntityManager.GetComponentData<MonsterSpawnZone>(chunkEntity);
                    int[] spawnedIDs = monsterSpawner.spawnedIDs.ToArray();
                    for (int j = 0; j < spawnedIDs.Length; j++)
                    {
                        characterDeathSystem.DestroyCharacter(spawnedIDs[j]);
                    }
                }
                chunks.Remove(chunkID);
                Chunk.Destroy(World.EntityManager, chunkEntity);
            }
        }

        #region removingIndexesChunks
        private void RemovePositionIndex(ref World world, int3 chunkPosition, int chunkID, int3[] allRemovePositions, int3 differencePosition)
        {
            // for chunks next to, set them as -1
            // first get position in front
            var positionInFront = chunkPosition + differencePosition;
            for (int i = 0; i < allRemovePositions.Length; i++)
            {
                if (allRemovePositions[i].x == positionInFront.x
                    && allRemovePositions[i].y == positionInFront.y
                    && allRemovePositions[i].z == positionInFront.z)
                {
                    return; // being removed as well dont need to set
                }
            }
            // check index of world
            bool contains = false;
            int frontID = 0;
            for (int i = 0; i < world.chunkPositions.Length; i++)
            {
                if (world.chunkPositions[i].x == positionInFront.x 
                    && world.chunkPositions[i].y == positionInFront.y 
                    && world.chunkPositions[i].z == positionInFront.z)
                {
                    contains = true;
                    frontID = world.chunkIDs[i];
                    break;
                }
            }
            if (contains)
            {
                if (chunks.ContainsKey(frontID))
                {
                    Entity e = chunks[frontID];
                    Chunk c = World.EntityManager.GetComponentData<Chunk>(e);
                    if (differencePosition.x == 0 && differencePosition.y == 0 && differencePosition.z == 1)
                    {
                        c.indexBack = -1;
                    }
                    else if (differencePosition.x == 0 && differencePosition.y == 0 && differencePosition.z == -1)
                    {
                        c.indexForward = -1;
                    }
                    else if (differencePosition.x == 1 && differencePosition.y == 0 && differencePosition.z == 0)
                    {
                        c.indexLeft = -1;
                    }
                    else if (differencePosition.x == -1 && differencePosition.y == 0 && differencePosition.z == 0)
                    {
                        c.indexRight = -1;
                    }
                    else if (differencePosition.x == 0 && differencePosition.y == 1 && differencePosition.z == 0)
                    {
                        c.indexDown = -1;
                    }
                    else if (differencePosition.x == 0 && differencePosition.y == -1 && differencePosition.z == 0)
                    {
                        c.indexUp = -1;
                    }
                    World.EntityManager.SetComponentData(e, c);
                }
                else
                {
                    Debug.LogError("Removing indexes but it doesnt exist: " + frontID);
                }
            }
        }

        #endregion

        public List<Material> GetWorldMaterials(int worldID)
        {
            List<Material> materials = new List<Material>();
            if (worldSpawnSystem.maps.ContainsKey(worldID))
            {
                materials = worldSpawnSystem.maps[worldID].tilemap.materials;
            }
            else if (worldSpawnSystem.models.ContainsKey(worldID))
            {
                materials.Add(Bootstrap.GetVoxelMaterial());//worldSpawnSystem.models[worldID].bakedMaterial);
            }
            return materials;
        }

        public void SpawnChunks(SpawnChunkCommand command)
        {
            var worldEntity = command.world;
            int worldID = World.EntityManager.GetComponentData<ZoxID>(worldEntity).id;
            var world = World.EntityManager.GetComponentData<World>(worldEntity);
            if (ChunkSpawnSystem.isDebugLog)
            {
                Debug.LogError("Spawning World's Chunks [" + command.chunkIDs.Length + "] with dimensions: " + world.voxelDimensions + ", id: " + worldID);
            }
            Translation worldTranslation = World.EntityManager.GetComponentData<Translation>(worldEntity);
            NativeArray<Entity> entities = new NativeArray<Entity>(command.chunkPositions.Length, Allocator.Temp);
            // materials
            int renderEntitiesCount = 0;
            for (int i = 0; i < command.isRender.Length; i++)
            {
                if (command.isRender[i] == 1)
                {
                    renderEntitiesCount++;
                }
            }
            List<Material> materials = GetWorldMaterials(worldID);
            MapDatam map = null;
            VoxData model = new VoxData();
            //NativeArray<Entity> renderEntities = new NativeArray<Entity>(renderEntitiesCount * materials.Count, Allocator.Temp); 
            if (worldSpawnSystem.maps.ContainsKey(worldID))
            {
                map = worldSpawnSystem.maps[worldID];
                World.EntityManager.Instantiate(chunkPrefab, entities);
                //World.EntityManager.Instantiate(worldChunkRenderPrefab, renderEntities);
            }
            else if (worldSpawnSystem.models.ContainsKey(worldID))
            {
                model = worldSpawnSystem.models[worldID];
                World.EntityManager.Instantiate(modelChunkPrefab, entities);
                //World.EntityManager.Instantiate(modelChunkRenderPrefab, renderEntities);
            }
            // for all bullets, set custom data using indexes entity
            int renderEntityCount = 0;
            var chunkIDs = command.chunkIDs.ToArray();
            var chunkPositions = command.chunkPositions.ToArray();
            for (int i = 0; i < entities.Length; i++)
            {
                Entity chunkEntity = entities[i];
                if (chunks.ContainsKey(chunkIDs[i]))
                {
                    World.EntityManager.DestroyEntity(chunkEntity);
                    continue;
                }
                chunks.Add(chunkIDs[i], entities[i]);
                var lookupTable = worldSpawnSystem.worldLookups[worldID];
                if (lookupTable.chunks.ContainsKey(chunkPositions[i])) {
                    lookupTable.chunks[chunkPositions[i]] = chunkEntity;
                } else {
                    lookupTable.chunks.Add(chunkPositions[i], chunkEntity);
                }
                worldSpawnSystem.worldLookups[worldID] = lookupTable;
                World.EntityManager.SetComponentData(entities[i], new ZoxID {
                    id = chunkIDs[i],
                    creatorID = worldID
                });
                Chunk chunk = World.EntityManager.GetComponentData<Chunk>(chunkEntity);
                chunk.world = worldEntity;
                chunk.Value.chunkPosition = chunkPositions[i];
                chunk.Value.worldScale = world.scale;
                chunk.Value.voxelDimensions = world.voxelDimensions;
                chunk.Init(world.voxelDimensions);
                if (model.id != 0)
                {
                    UpdateChunkWithModel(chunkEntity, ref chunk, model);
                }
                SetChunkSurroundingIndexes(chunkEntity, ref chunk);
                float3 spawnPosition = new float3( // worldOffset + 
                    chunk.Value.chunkPosition.x * chunk.Value.worldScale.x * chunk.Value.voxelDimensions.x,
                    chunk.Value.chunkPosition.y * chunk.Value.worldScale.y * chunk.Value.voxelDimensions.y,
                    chunk.Value.chunkPosition.z * chunk.Value.worldScale.z * chunk.Value.voxelDimensions.z);
                World.EntityManager.SetComponentData(chunkEntity, new Translation { Value = spawnPosition });
                World.EntityManager.SetComponentData(chunkEntity, new NonUniformScale { Value = new float3(1, 1, 1) });
                World.EntityManager.SetComponentData(chunkEntity, new Rotation { Value = quaternion.identity });
                World.EntityManager.SetComponentData(chunkEntity, new Parent { Value = chunk.world });
                // set depending on biome type - biomeDatam has CharacterDatam linked
                //World.EntityManager.SetComponentData(entities[i], chunk);
                World.EntityManager.SetComponentData(entities[i], chunk);
            }
            var entitiesArray = entities.ToArray();;
            world.chunks = new BlitableArray<Entity>(entitiesArray.Length, Allocator.Persistent);
            for (int i = 0; i < world.chunks.Length; i++)
            {
                world.chunks[i] = entitiesArray[i];
            }
            World.EntityManager.SetComponentData(worldEntity, world);
            ChunkRenderSystem.SpawnChunkRenders(World.EntityManager, worldEntity, renderEntitiesCount, materials.Count, command.isRender.ToArray());
            entities.Dispose();
        }

        private void UpdateChunkWithModel(Entity chunkEntity, ref Chunk chunk, VoxData model)
        {
            chunk.isWeights = 1;
            // set chunk data depending on voxModel data
            //if (model.Value.size.x == 16 && model.Value.size.y == 16 && model.Value.size.z == 16)
            {
                //chunk.isDirty = 1;
                if ( model.data.Length == 0)
                {
                    Debug.LogError("Model Data is 0.");
                    return;
                }
                World.EntityManager.AddComponentData(chunkEntity, new ChunkBuilder { });
                int3 localPosition; // = float3.zero;
                for (localPosition.x = 0; localPosition.x < chunk.Value.voxelDimensions.x; localPosition.x++)
                {
                    for (localPosition.y = 0; localPosition.y < chunk.Value.voxelDimensions.y; localPosition.y++)
                    {
                        for (localPosition.z = 0; localPosition.z < chunk.Value.voxelDimensions.z; localPosition.z++)
                        {
                            int voxelIndex = VoxelRaycastSystem.GetVoxelArrayIndex(localPosition, chunk.Value.voxelDimensions);
                            int modelVoxelIndex = VoxelRaycastSystem.GetVoxelArrayIndex(localPosition + chunk.GetVoxelPosition(), model.size);
                            int newVoxelType = model.data[modelVoxelIndex];
                            if (newVoxelType == 0)
                            {
                                chunk.Value.voxels[voxelIndex] = (byte)(newVoxelType);
                            }
                            else
                            {
                                // mutations
                                if (Bootstrap.instance.isMutateVoxes)
                                {
                                    if (UnityEngine.Random.Range(0, 100) >= 90)
                                    {
                                        chunk.Value.voxels[voxelIndex] = (byte)(newVoxelType + 1);
                                    }
                                    else
                                    {
                                        if (UnityEngine.Random.Range(0, 100) >= 93)
                                        {
                                            chunk.Value.voxels[voxelIndex] = (byte)(0);
                                        }
                                        else
                                        {
                                            chunk.Value.voxels[voxelIndex] = (byte)(newVoxelType);
                                        }
                                    }
                                }
                                else {
                                    chunk.Value.voxels[voxelIndex] = (byte)(newVoxelType);
                                }
                            }
                        }
                    }
                }
            }
        }
       
        public void SetChunkSurroundingIndexes(Entity chunkEntity, ref Chunk chunk)
        {
            // defaults
            chunk.indexUp = -1;
            chunk.indexDown = -1;
            chunk.indexLeft = -1;
            chunk.indexRight = -1;
            chunk.indexBack = -1;
            chunk.indexForward = -1;
            //int3 chunkPositionUp = chunk.Value.chunkPosition + new int3(0,1,0);
            //int3 chunkPositionDown = chunk.Value.chunkPosition + new int3(0,-1,0);
            int3 chunkPositionLeft = chunk.Value.chunkPosition + int3.Left(); //new int3(-1,0,0);
            int3 chunkPositionRight = chunk.Value.chunkPosition + int3.Right(); //new int3(1,0,0);
            int3 chunkPositionForward = chunk.Value.chunkPosition + int3.Forward(); //new int3(0,0,1);
            int3 chunkPositionBack = chunk.Value.chunkPosition + int3.Back(); //new int3(0,0,-1);
            int worldID = World.EntityManager.GetComponentData<ZoxID>(chunkEntity).creatorID;
            int chunkID = World.EntityManager.GetComponentData<ZoxID>(chunkEntity).id;
            var map = worldSpawnSystem.worldLookups[worldID];

            if (map.chunks.ContainsKey(chunkPositionLeft))
            {
                Entity entity = map.chunks[chunkPositionLeft];
                Chunk otherChunk = World.EntityManager.GetComponentData<Chunk>(entity);
                int otherChunkID = World.EntityManager.GetComponentData<ZoxID>(entity).id;
                if (otherChunk.Value.chunkPosition == chunkPositionLeft)
                {
                    //Debug.LogError("Chunk Found: " + otherChunk.Value.chunkPosition);
                    chunk.indexLeft = otherChunkID;
                    otherChunk.indexRight = chunkID;
                    World.EntityManager.SetComponentData(entity, otherChunk);
                }
            }
            if (map.chunks.ContainsKey(chunkPositionRight))
            {
                Entity entity = map.chunks[chunkPositionRight];
                Chunk otherChunk = World.EntityManager.GetComponentData<Chunk>(entity);
                int otherChunkID = World.EntityManager.GetComponentData<ZoxID>(entity).id;
                if (otherChunk.Value.chunkPosition == chunkPositionRight)
                {
                    chunk.indexRight = otherChunkID;
                    otherChunk.indexLeft = chunkID;
                    World.EntityManager.SetComponentData(entity, otherChunk);
                }
            }

            if (map.chunks.ContainsKey(chunkPositionBack))
            {
                Entity entity = map.chunks[chunkPositionBack];
                Chunk otherChunk = World.EntityManager.GetComponentData<Chunk>(entity);
                int otherChunkID = World.EntityManager.GetComponentData<ZoxID>(entity).id;
                if (otherChunk.Value.chunkPosition == chunkPositionBack)
                {
                    chunk.indexBack = otherChunkID;
                    otherChunk.indexForward = chunkID;
                    World.EntityManager.SetComponentData(entity, otherChunk);
                }
            }
            
            if (map.chunks.ContainsKey(chunkPositionForward))
            {
                Entity entity = map.chunks[chunkPositionForward];
                Chunk otherChunk = World.EntityManager.GetComponentData<Chunk>(entity);
                int otherChunkID = World.EntityManager.GetComponentData<ZoxID>(entity).id;
                if (otherChunk.Value.chunkPosition == chunkPositionForward)
                {
                    chunk.indexForward = otherChunkID;
                    otherChunk.indexBack = chunkID;
                    World.EntityManager.SetComponentData(entity, otherChunk);
                }
            }
        }

        #region Spawning-Despawning

        public int[] SpawnChunks(Entity world, int3[] positions, bool[] isRender)
        {
            SpawnChunkCommand newCommand = new SpawnChunkCommand();
            newCommand.world = world;
            newCommand.SetChunkPositions(positions);
            newCommand.SetChunkRenders(isRender);
            // generate new ids
            var chunkIDs = new int[positions.Length];
            for (int i = 0; i < chunkIDs.Length; i++)
            {
                chunkIDs[i] = Bootstrap.GenerateUniqueID();
            }
            newCommand.SetChunkIDs(chunkIDs);

            Entity e = World.EntityManager.CreateEntity();
            World.EntityManager.AddComponentData(e, newCommand);
            //SpawnChunks(newCommand);
            return newCommand.chunkIDs;
        }

        public struct SpawnChunkCommand : IComponentData
        {
            public Entity world;
            public BlitableArray<int3> chunkPositions;
            public BlitableArray<int> chunkIDs;
            public BlitableArray<byte> isRender;

            public void SetChunkPositions(int3[] newPositions)
            {
                chunkPositions = new BlitableArray<int3>(newPositions.Length, Allocator.Persistent);
                for (int i = 0; i < newPositions.Length; i++)
                {
                    chunkPositions[i] = newPositions[i];
                }
            }
            public void SetChunkIDs(int[] newIDs)
            {
                chunkIDs = new BlitableArray<int>(newIDs.Length, Allocator.Persistent);
                for (int i = 0; i < newIDs.Length; i++)
                {
                    chunkIDs[i] = newIDs[i];
                }
            }
            public void SetChunkRenders(bool[] newRenders)
            {
                isRender = new BlitableArray<byte>(newRenders.Length, Allocator.Persistent);
                for (int i = 0; i < newRenders.Length; i++)
                {
                    if (newRenders[i])
                    {
                        isRender[i] = 1;
                    }
                    else
                    {
                        isRender[i] = 0;
                    }
                }
            }

            public void Dispose()
            {
                chunkPositions.Dispose();
                isRender.Dispose();
                chunkIDs.Dispose();
            }
        }

        public struct RemoveChunkCommand : IComponentData
        {
            public Entity chunk;
        }

        protected override void OnUpdate()
        {
            Entities.WithAll<SpawnChunkCommand>().ForEach((Entity e, ref SpawnChunkCommand command) =>
            {
                SpawnChunks(command);
                command.Dispose();
                //SpawnWorld(command.spawnID, command.spawnPosition, mapsMeta[command.mapID]);
                World.EntityManager.DestroyEntity(e);
            });
            Entities.WithAll<RemoveChunkCommand>().ForEach((Entity e, ref RemoveChunkCommand command) =>
            {
                RemoveChunk(command.chunk);
                World.EntityManager.DestroyEntity(e);
            });
        }
        #endregion
        public static readonly Vector2[,] cubeUvs = new Vector2[,]
        {
            // Top
            {
                new Vector3(0, 0),
                new Vector3(0, 1),
                new Vector3(1, 1),
                new Vector3(1, 0)
            },
            // Bottom
			{
                new Vector3(1, 0),
                new Vector3(0, 0),
                new Vector3(0, 1),
                new Vector3(1, 1),
            },
            // Left
            {
                new Vector3(0, 0),
                new Vector3(0, 1),
                new Vector3(1, 1),
                new Vector3(1, 0)
            },
            // Right
            {
                new Vector3(0, 0),
                new Vector3(0, 1),
                new Vector3(1, 1),
                new Vector3(1, 0)
            },
            // Front
			{
                new Vector3(0, 0),
                new Vector3(1, 0),
                new Vector3(1, 1),
                new Vector3(0, 1),
            },
            // Back
			{
                new Vector3(1, 0),
                new Vector3(0, 0),
                new Vector3(0, 1),
                new Vector3(1, 1),
            }
        };


    }
}