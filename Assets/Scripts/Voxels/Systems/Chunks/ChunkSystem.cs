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
        // references
        public WorldSpawnSystem worldSpawnSystem;
        public VoxelSpawnSystem voxelSpawnSystem;
        public CharacterDeathSystem characterDeathSystem;
        private EntityArchetype chunkArchtype;
        private EntityArchetype chunkRenderArchtype;
        private Entity chunkPrefab;
        private Entity chunkRenderPrefab;
        private Entity modelChunkPrefab;
        public Dictionary<int, Entity> chunks = new Dictionary<int, Entity>();
        public Dictionary<int, Entity> chunkRenders = new Dictionary<int, Entity>();
        //private List<ChunkSpawnCommand> commands = new List<ChunkSpawnCommand>();    // used for moving between chunks
        
        public void Clear()
        {
            foreach (Entity e in chunks.Values)
            {
                Chunk.Destroy(World.EntityManager, e);
            }
            chunks.Clear();
            foreach (Entity e in chunkRenders.Values)
            {
                ChunkRenderer.Destroy(World.EntityManager, e);
            }
            chunkRenders.Clear();
        }

        protected override void OnCreate()
        {
            /*RenderBounds b = new RenderBounds
            {
                Value = new AABB
                {
                    Extents = new float3(iconSize.x, iconSize.y, 0.5f)
                }
            };*/
            chunkArchtype = World.EntityManager.CreateArchetype(
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
            chunkRenderArchtype = World.EntityManager.CreateArchetype(
                // transform
                typeof(Translation),
                typeof(Rotation),
                typeof(NonUniformScale),
                typeof(LocalToWorld),
                typeof(Parent),
                typeof(LocalToParent),
                typeof(RenderBounds),
                // renderer
                //typeof(ChunkRenderer),
                typeof(RenderMesh),
                typeof(Static),
                typeof(ZoxID)
            );
            chunkPrefab = World.EntityManager.CreateEntity(chunkArchtype);
            World.EntityManager.AddComponentData(chunkPrefab, new Prefab { });
            // render prefab
            chunkRenderPrefab = World.EntityManager.CreateEntity(chunkRenderArchtype);
            World.EntityManager.AddComponentData(chunkRenderPrefab, new Prefab { });
            RenderMesh renderer = World.EntityManager.GetSharedComponentData<RenderMesh>(chunkRenderPrefab);
            //renderer.mesh = new Mesh();
            renderer.castShadows = UnityEngine.Rendering.ShadowCastingMode.On;
            renderer.receiveShadows = true;
            World.EntityManager.SetSharedComponentData(chunkRenderPrefab, renderer);

            // prefab

            modelChunkPrefab = World.EntityManager.CreateEntity(chunkArchtype);
            World.EntityManager.RemoveComponent<WorldGenerationChunk>(modelChunkPrefab);
            World.EntityManager.AddComponentData(modelChunkPrefab, new Prefab { });
        }

        // Redo later while cycling through world chunk references rather then all worlds
        public void DestroyWorld(int worldID)
        {
            List<int> removeIDs = new List<int>();
            foreach (Entity e in chunks.Values)
            {
                Chunk chunk = World.EntityManager.GetComponentData<Chunk>(e);
                if (chunk.worldID == worldID)
                {
                    var lookupTable = worldSpawnSystem.worldLookups[worldID];
                    lookupTable.chunks.Remove(chunk.Value.chunkPosition);
                    worldSpawnSystem.worldLookups[worldID] = lookupTable;

                    removeIDs.Add(chunk.id);
                    // chunk.renderIDs

                    if (World.EntityManager.HasComponent<MonsterSpawnZone>(e))
                    {
                        MonsterSpawnZone monsterSpawner = World.EntityManager.GetComponentData<MonsterSpawnZone>(e);
                        for (int i = 0; i < monsterSpawner.spawnedIDs.Length; i++)
                        {
                            characterDeathSystem.DestroyCharacter(monsterSpawner.spawnedIDs[i]);
                        }
                    }

                    // for all chunkRenders (should be)
                    for (int i = 0; i < chunk.chunkRenders.Length; i++)
                    {
                        Entity chunkRender = chunkRenders[chunk.chunkRenders[i]];
                        ChunkRenderer.Destroy(World.EntityManager, chunkRender);
                        chunkRenders.Remove(chunk.chunkRenders[i]);
                    }
                    if (World.EntityManager.Exists(e))
                    {
                        World.EntityManager.DestroyEntity(e);
                    }
                }
            }
            foreach (int removeID in removeIDs)
            {
                chunks.Remove(removeID);
            }
        }

        public void RemoveChunks(ref World world, int[] chunkIDs, int3[] positions)
        {
            for (int i = 0; i < chunkIDs.Length; i++)
            {
                RemovePositionIndex(ref world, positions[i], chunkIDs[i], positions,  int3.Forward()); //new float3(0, 0, 1));
                RemovePositionIndex(ref world, positions[i], chunkIDs[i], positions,  int3.Back()); //new float3(0, 0, -1));
                RemovePositionIndex(ref world, positions[i], chunkIDs[i], positions, int3.Right()); // new float3(1, 0, 0));
                RemovePositionIndex(ref world, positions[i], chunkIDs[i], positions, int3.Left()); //new float3(-1, 0, 0));
                RemoveChunk(chunkIDs[i], false);
            }
        }

        public void RemoveChunk(int chunkID, bool isRemoveFromWorld = true)
        {
            if (chunks.ContainsKey(chunkID))
            {
                Entity chunkEntity = chunks[chunkID];
                Chunk chunk = World.EntityManager.GetComponentData<Chunk>(chunkEntity);

                var lookupTable = worldSpawnSystem.worldLookups[chunk.worldID];
                lookupTable.chunks.Remove(chunk.Value.chunkPosition);
                worldSpawnSystem.worldLookups[chunk.worldID] = lookupTable;

                if (isRemoveFromWorld)
                {
                    if (worldSpawnSystem.worlds.ContainsKey(chunk.worldID))
                    {
                        Entity worldEntity = worldSpawnSystem.worlds[chunk.worldID];
                        World world = World.EntityManager.GetComponentData<World>(worldEntity);
                        int3[] positions = new int3[0];
                        var chunkPosition = chunk.Value.chunkPosition;
                        RemovePositionIndex(ref world, chunkPosition, chunkID, positions, int3.Forward()); //new float3(0, 0, 1));
                        RemovePositionIndex(ref world, chunkPosition, chunkID, positions, int3.Back()); //new float3(0, 0, -1));
                        RemovePositionIndex(ref world, chunkPosition, chunkID, positions, int3.Right()); // new float3(1, 0, 0));
                        RemovePositionIndex(ref world, chunkPosition, chunkID, positions, int3.Left()); //new float3(-1, 0, 0));
                        World.EntityManager.SetComponentData(worldEntity, world);
                    }
                }
                for (int j = 0; j < chunk.chunkRenders.Length; j++)
                {
                    int chunkRenderID = chunk.chunkRenders[j];
                    Entity chunkRender = chunkRenders[chunkRenderID];
                    if (chunkRenders.ContainsKey(chunkRenderID))
                    {
                        ChunkRenderer.Destroy(World.EntityManager, chunkRender);
                        chunkRenders.Remove(chunkRenderID);
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
                Chunk.Destroy(World.EntityManager, chunkEntity);
                chunks.Remove(chunkID);
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
            int worldID = command.worldID;
            var worldEntity = worldSpawnSystem.worlds[worldID];
            World world = World.EntityManager.GetComponentData<World>(worldEntity);
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
            if (worldSpawnSystem.maps.ContainsKey(worldID))
            {
                map = worldSpawnSystem.maps[worldID];
                World.EntityManager.Instantiate(chunkPrefab, entities);
            }
            else if (worldSpawnSystem.models.ContainsKey(worldID))
            {
                model = worldSpawnSystem.models[worldID];
                World.EntityManager.Instantiate(modelChunkPrefab, entities);
            }
            NativeArray<Entity> renderEntities = new NativeArray<Entity>(renderEntitiesCount *
               materials.Count, Allocator.Temp);   // * materials.count
            World.EntityManager.Instantiate(chunkRenderPrefab, renderEntities);
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

                Chunk chunk = World.EntityManager.GetComponentData<Chunk>(chunkEntity);
                chunk.world = worldEntity;
                chunk.id = chunkIDs[i];
                chunk.worldID = command.worldID;
                chunk.Value.chunkPosition = chunkPositions[i];
                chunk.Value.worldScale = world.scale;
                chunk.Value.voxelDimensions = world.voxelDimensions;
                chunk.InitializeData(world.voxelDimensions);
                if (model.id != 0)
                {
                    UpdateChunkWithModel(chunkEntity, ref chunk, model);
                }

                SetChunkSurroundingIndexes(ref chunk);
                float3 spawnPosition = new float3( // worldOffset + 
                    chunk.Value.chunkPosition.x * chunk.Value.worldScale.x * chunk.Value.voxelDimensions.x,
                    chunk.Value.chunkPosition.y * chunk.Value.worldScale.y * chunk.Value.voxelDimensions.y,
                    chunk.Value.chunkPosition.z * chunk.Value.worldScale.z * chunk.Value.voxelDimensions.z);
                World.EntityManager.SetComponentData(chunkEntity, new Translation { Value = spawnPosition });
                World.EntityManager.SetComponentData(chunkEntity, new NonUniformScale { Value = new float3(1, 1, 1) });
                World.EntityManager.SetComponentData(chunkEntity, new Rotation { Value = quaternion.identity });
                World.EntityManager.SetComponentData(chunkEntity, new Parent { Value = worldSpawnSystem.worlds[chunk.worldID] });
                // set depending on biome type - biomeDatam has CharacterDatam linked
                //World.EntityManager.SetComponentData(entities[i], chunk);

                // need to bulk these too
                if (command.isRender[i] == 1)
                {
                    Entity[] renderEntitiesSmall = new Entity[materials.Count];
                    for (int j = 0; j < renderEntitiesSmall.Length; j++)
                    {
                        renderEntitiesSmall[j] = renderEntities[renderEntityCount * materials.Count + j];
                    }
                    AddRenderEntitiesToChunk(chunkEntity, ref chunk, world, materials, spawnPosition, renderEntitiesSmall, model);
                    renderEntityCount++;
                }
                else
                {
                    chunk.chunkRenders = new BlitableArray<int>(0, Allocator.Persistent);
                }
                World.EntityManager.SetComponentData(entities[i], chunk);
            }
            renderEntities.Dispose();
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
                                chunk.Value.voxels[voxelIndex] = (byte)(newVoxelType);
                                // mutations
                                /*if (UnityEngine.Random.Range(0, 100) >= 90)
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
                                }*/
                            }
                        }
                    }
                }
            }
        }

        public void RemoveRenderEntitiesFromChunk(ref Chunk chunk)
        {
            if (ChunkSpawnSystem.isDebugLog)
            {
                Debug.LogError("Removing chunk renders from chunk: " + chunk.Value.chunkPosition);
            }
            if (chunk.chunkRenders.Length != 0)
            {
                for (int i = 0; i < chunk.chunkRenders.Length; i++)
                {
                    int renderID = chunk.chunkRenders[i];
                    Entity chunkRenderEntity = chunkRenders[renderID];
                    RenderMesh renderer = World.EntityManager.GetSharedComponentData<RenderMesh>(chunkRenderEntity);
                    GameObject.Destroy(renderer.mesh);
                    World.EntityManager.DestroyEntity(chunkRenderEntity);
                    chunkRenders.Remove(renderID);
                }
                chunk.chunkRenders = new BlitableArray<int>(0, Allocator.Persistent);
            }
        }


        public void AddRenderEntitiesToChunk(World world, Entity chunkEntity, ref Chunk chunk)
        {
            List<Material> materials = GetWorldMaterials(world.id);
            float3 spawnPosition = World.EntityManager.GetComponentData<Translation>(chunks[chunk.id]).Value;
            VoxData model = new VoxData();
            if (worldSpawnSystem.models.ContainsKey(world.id))
            {
                model = worldSpawnSystem.models[world.id];
            }
            Entity[] entities = new Entity[materials.Count];
            for (int i = 0; i < entities.Length; i++)
            {
                entities[i] = World.EntityManager.Instantiate(chunkRenderPrefab);
            }
            AddRenderEntitiesToChunk(chunkEntity, ref chunk, world, materials, spawnPosition, entities, model);

        }

        public void AddRenderEntitiesToChunk(Entity chunkEntity, ref Chunk chunk, World world, List<Material> materials, float3 spawnPosition, Entity[] renderEntities, VoxData model)
        {
            if (ChunkSpawnSystem.isDebugLog)
            {
                Debug.LogError("Adding " + materials.Count + " chunk renders to chunk: " + chunk.Value.chunkPosition + "::" + chunk.Value.voxelDimensions);
            }
            chunk.chunkRenders = new BlitableArray<int>(materials.Count, Allocator.Persistent);
            for (int j = 0; j < materials.Count; j++)
            {
                //Entity renderEntity = renderEntities[i * materials.Count + j];
                Entity renderEntity = renderEntities[j];
                int renderID = SetChunkRenderEntity(
                    renderEntity,
                    chunkEntity,
                    ref chunk,
                    materials[j],
                    j,
                    spawnPosition,
                    model.id != 0,
                    world.skeletonID);
                if (model.id != 0)
                {
                    ChunkRenderer chunkRender = World.EntityManager.GetComponentData<ChunkRenderer>(renderEntity);
                    List<Color> colors = model.GetColors(); //  new List<Color>(); // 
                    chunkRender.voxelColors = new BlitableArray<float3>(colors.Count, Allocator.Persistent);
                    for (int a = 0; a < colors.Count; a++)
                    {
                        chunkRender.voxelColors[a] = new float3(colors[a].r, colors[a].b, colors[a].g);
                    }
                    World.EntityManager.SetComponentData(renderEntity, chunkRender);
                }
                chunk.chunkRenders[j] = renderID;
            }
        }

        private int SetChunkRenderEntity(Entity chunkRendererEntity, Entity chunkEntity, ref Chunk chunk,
            Material material, int materialID, float3 spawnPosition, 
            bool isModel, int skeletonID)
        {
            int id = Bootstrap.GenerateUniqueID();
            if (World.EntityManager.HasComponent<ChunkRenderer>(chunkRendererEntity))
            {
                var chunkRenderer = World.EntityManager.GetComponentData<ChunkRenderer>(chunkRendererEntity);
                chunkRenderer.chunk = chunkEntity;
                World.EntityManager.SetComponentData(chunkRendererEntity, chunkRenderer);
            }
            World.EntityManager.SetComponentData(chunkRendererEntity, new ZoxID { id = id, creatorID = chunk.id });
            World.EntityManager.SetComponentData(chunkRendererEntity, new Translation { Value = spawnPosition });
            World.EntityManager.SetComponentData(chunkRendererEntity, new NonUniformScale { Value = new float3(1,1,1) });
            World.EntityManager.SetComponentData(chunkRendererEntity, new Rotation { Value = quaternion.identity });
            World.EntityManager.SetComponentData(chunkRendererEntity, new Parent { Value = worldSpawnSystem.worlds[chunk.worldID] });
            // Render material data
            RenderMesh renderer = World.EntityManager.GetSharedComponentData<RenderMesh>(chunkRendererEntity);
            renderer.material = material;
            renderer.subMesh = materialID;
            renderer.mesh = new Mesh();
            World.EntityManager.SetSharedComponentData(chunkRendererEntity, renderer);
            if (isModel)
            {
                AddChunkRenderComponent(chunkRendererEntity, chunkEntity, ref chunk, materialID, skeletonID);
            }
            chunkRenders.Add(id, chunkRendererEntity);
            return id;
        }

        public void AddChunkRenderComponent(Entity chunkRenderEntity, Entity chunkEntity, ref Chunk chunk, int materialID, int skeletonID)// byte buildState = 0)
        {
            ChunkRenderer chunkRender = new ChunkRenderer { };
            chunkRender.chunk = chunkEntity;
            chunkRender.materialID = (byte)materialID;
            chunkRender.SetMetaData(voxelSpawnSystem.meta, voxelSpawnSystem.voxelIDs);
            chunkRender.InitializeData(chunk.Value.voxelDimensions); //, ChunkSpawnSystem.maxCacheVerts, ChunkSpawnSystem.maxCacheTriangles);
            chunkRender.Value = chunk.Value;
            if (skeletonID != 0)
            {
                chunkRender.hasWeights = 1;
                chunkRender.InitializeBoneWeights(ChunkSpawnSystem.maxCacheVerts,
                    worldSpawnSystem.skeletonsMeta[chunk.worldID].data.datas);
            }
            World.EntityManager.AddComponentData(chunkRenderEntity, chunkRender);
        }

        public void SetChunkSurroundingIndexes(ref Chunk chunk)
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
            var map = worldSpawnSystem.worldLookups[chunk.worldID];

            if (map.chunks.ContainsKey(chunkPositionLeft)) {
                Entity entity = map.chunks[chunkPositionLeft];
                Chunk otherChunk = World.EntityManager.GetComponentData<Chunk>(entity);
                if (otherChunk.Value.chunkPosition == chunkPositionLeft)
                {
                    //Debug.LogError("Chunk Found: " + otherChunk.Value.chunkPosition);
                    chunk.indexLeft = otherChunk.id;
                    otherChunk.indexRight = chunk.id;
                    World.EntityManager.SetComponentData(entity, otherChunk);
                }
                /*else {
                    Debug.LogError("Position isn't right: " + chunkPositionLeft + " :: " + otherChunk.Value.chunkPosition);
                }*/
            }
            if (map.chunks.ContainsKey(chunkPositionRight)) {
                Entity entity = map.chunks[chunkPositionRight];
                Chunk otherChunk = World.EntityManager.GetComponentData<Chunk>(entity);
                if (otherChunk.Value.chunkPosition == chunkPositionRight)
                {
                    chunk.indexRight = otherChunk.id;
                    otherChunk.indexLeft = chunk.id;
                    World.EntityManager.SetComponentData(entity, otherChunk);
                }
                /*else {
                    Debug.LogError("Position isn't right: " + chunkPositionRight + " :: " + otherChunk.Value.chunkPosition);
                }*/
            }

            if (map.chunks.ContainsKey(chunkPositionBack)) {
                Entity entity = map.chunks[chunkPositionBack];
                Chunk otherChunk = World.EntityManager.GetComponentData<Chunk>(entity);
                if (otherChunk.Value.chunkPosition == chunkPositionBack)
                {
                    chunk.indexBack = otherChunk.id;
                    otherChunk.indexForward = chunk.id;
                    World.EntityManager.SetComponentData(entity, otherChunk);
                }
                /*else {
                    Debug.LogError("Position isn't right: " + chunkPositionBack + " :: " + otherChunk.Value.chunkPosition);
                }*/
            }
            
            if (map.chunks.ContainsKey(chunkPositionForward)) {
                Entity entity = map.chunks[chunkPositionForward];
                Chunk otherChunk = World.EntityManager.GetComponentData<Chunk>(entity);
                if (otherChunk.Value.chunkPosition == chunkPositionForward)
                {
                    chunk.indexForward = otherChunk.id;
                    otherChunk.indexBack = chunk.id;
                    World.EntityManager.SetComponentData(entity, otherChunk);
                }
                /*else {
                    Debug.LogError("Position isn't right: " + chunkPositionForward + " :: " + otherChunk.Value.chunkPosition);
                }*/
            }

            /*if (map.chunks.ContainsKey(chunkPositionLeft)) {
                Entity entity = map.chunks[chunkPositionLeft];
                Chunk otherChunk = World.EntityManager.GetComponentData<Chunk>(entity);
                otherChunk.indexRight = chunk.id;
                World.EntityManager.SetComponentData(entity, otherChunk);
                chunk.indexLeft = otherChunk.id;
                //UnityEngine.Debug.LogError("ChunkPositionLeft: " + chunkPositionLeft + " does exists.");
            } 
            //else {
                //UnityEngine.Debug.LogError("chunkPositionLeft: " + chunkPositionLeft + " does not exist.");
            //}
            if (map.chunks.ContainsKey(chunkPositionRight)) {
                Entity entity = map.chunks[chunkPositionRight];
                Chunk otherChunk = World.EntityManager.GetComponentData<Chunk>(entity);
                otherChunk.indexLeft = chunk.id;
                World.EntityManager.SetComponentData(entity, otherChunk);
                chunk.indexRight = otherChunk.id;
            }
            
            if (map.chunks.ContainsKey(chunkPositionForward)) {
                Entity entity = map.chunks[chunkPositionForward];
                Chunk otherChunk = World.EntityManager.GetComponentData<Chunk>(entity);
                otherChunk.indexBack = chunk.id;
                World.EntityManager.SetComponentData(entity, otherChunk);
                chunk.indexForward = otherChunk.id;
            }
            if (map.chunks.ContainsKey(chunkPositionBack)) {
                Entity entity = map.chunks[chunkPositionBack];
                Chunk otherChunk = World.EntityManager.GetComponentData<Chunk>(entity);
                otherChunk.indexForward = chunk.id;
                World.EntityManager.SetComponentData(entity, otherChunk);
                chunk.indexBack = otherChunk.id;
            }*/
            /*if (otherChunkPosition == chunkPositionLeft)
            {
                chunk.indexLeft = otherChunkIndex;
                otherChunk.indexRight = chunk.id;
                World.EntityManager.SetComponentData(entity, otherChunk);
            }*/

            //foreach (Entity entity in chunks)

           // float3 worldMiddlePosition = float3.zero;
            /*var chunkPosition = chunk.Value.chunkPosition;
            foreach (Entity entity in chunks.Values)
            {
                // if initialized
                Chunk otherChunk = World.EntityManager.GetComponentData<Chunk>(entity);
                if (otherChunk.Value.chunkPosition == chunkPositionLeft)
                {
                    chunk.indexLeft = otherChunk.id;
                    otherChunk.indexRight = chunk.id;
                    World.EntityManager.SetComponentData(entity, otherChunk);
                }
                if (otherChunk.Value.chunkPosition == chunkPositionRight)
                {
                    chunk.indexRight = otherChunk.id;
                    otherChunk.indexLeft = chunk.id;
                    World.EntityManager.SetComponentData(entity, otherChunk);
                }

                if (otherChunk.Value.chunkPosition == chunkPositionBack)
                {
                    chunk.indexBack = otherChunk.id;
                    otherChunk.indexForward = chunk.id;
                    World.EntityManager.SetComponentData(entity, otherChunk);
                }
                if (otherChunk.Value.chunkPosition == chunkPositionForward)
                {
                    chunk.indexForward = otherChunk.id;
                    otherChunk.indexBack = chunk.id;
                    World.EntityManager.SetComponentData(entity, otherChunk);
                }
            }*/
        }


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


        #region Spawning-Despawning

        public int[] SpawnChunks(int worldID, int3[] positions, bool[] isRender)
        {
            SpawnChunkCommand newCommand = new SpawnChunkCommand();
            newCommand.worldID = worldID;
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

        /*public int[] QueueChunks(int worldID, float3[] positions, bool[] isRender)
        {
            //Debug.LogError("Queuing for " + positions.Length + " chunks!");
            ChunkSpawnCommand newCommand = new ChunkSpawnCommand();
            newCommand.worldID = worldID;
            newCommand.chunkPositions = positions;
            newCommand.isRender = isRender;
            // generate new ids
            newCommand.chunkIDs = new int[positions.Length];
            for (int i = 0; i < newCommand.chunkIDs.Length; i++)
            {
                newCommand.chunkIDs[i] = Bootstrap.GenerateUniqueID();
            }
            commands.Add(newCommand);
            return newCommand.chunkIDs;
        }*/

        public struct SpawnChunkCommand : IComponentData
        {
            public int worldID;
            public BlitableArray<int3> chunkPositions;
            public BlitableArray<int> chunkIDs;
            public BlitableArray<byte> isRender;
           /* public float3[] chunkPositions;
            public int[] chunkIDs;
            public bool[] isRender;*/

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

        /*public struct SpawnMapCommand : IComponentData
        {
            public int mapID;
            public int spawnID;
            public float3 spawnPosition;
        }*/

        public struct RemoveChunkCommand : IComponentData
        {
            public int chunkID;
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
                RemoveChunk(command.chunkID);
                World.EntityManager.DestroyEntity(e);
            });
            /*if (commands.Count > 0)
            {
                int index = commands.Count - 1;
                ChunkSpawnCommand command = commands[commands.Count - 1];
                commands.RemoveAt(index);
            }*/
        }
        #endregion
    }
}