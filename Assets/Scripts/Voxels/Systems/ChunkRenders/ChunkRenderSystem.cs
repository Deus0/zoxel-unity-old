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
    
    [DisableAutoCreation]
    public class ChunkRenderSystem : ComponentSystem
    {
        public WorldSpawnSystem worldSpawnSystem;
        public VoxelSpawnSystem voxelSpawnSystem;
        //public Dictionary<int, Entity> chunkRenders = new Dictionary<int, Entity>();
        private EntityArchetype worldChunkRenderArchtype;
        private Entity worldChunkRenderPrefab;
        private EntityArchetype modelChunkRenderArchtype;
        private Entity modelChunkRenderPrefab;

        protected override void OnCreate()
        {
            // renderers
            modelChunkRenderArchtype = World.EntityManager.CreateArchetype(
                // important
                typeof(ChunkMesh),
                typeof(ChunkMeshAnimation),
                typeof(ChunkRenderer),
                typeof(ChunkSides),
                typeof(ZoxID),
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
                typeof(Static)
            );
            // render prefab
            modelChunkRenderPrefab = World.EntityManager.CreateEntity(modelChunkRenderArchtype);
            World.EntityManager.AddComponentData(modelChunkRenderPrefab, new Prefab { });
            RenderMesh renderer = World.EntityManager.GetSharedComponentData<RenderMesh>(modelChunkRenderPrefab);
            //renderer.mesh = new Mesh();
            renderer.castShadows = UnityEngine.Rendering.ShadowCastingMode.On;
            renderer.receiveShadows = true;
            World.EntityManager.SetSharedComponentData(modelChunkRenderPrefab, renderer);
            // world chunk prefab
            worldChunkRenderArchtype = World.EntityManager.CreateArchetype(
                // important
                typeof(ZoxID),
                typeof(ChunkMeshLink),
                typeof(ChunkMesh),
                typeof(ChunkRenderer),
                typeof(ChunkSides),
                // other stuff
                typeof(Parent),
                typeof(Translation)
            );
            worldChunkRenderPrefab = World.EntityManager.CreateEntity(worldChunkRenderArchtype);
            World.EntityManager.AddComponentData(worldChunkRenderPrefab, new Prefab { });

            if (!Bootstrap.instance.isCustomRenderSystem)
            {
                worldChunkRenderPrefab = modelChunkRenderPrefab;
            }
        }

        public struct SpawnChunkRendersCommand : IComponentData
        {
            public Entity world;
            public int renderEntitiesCount;
            public int materialsCount;
            public BlitableArray<byte> isRender;

            public void Dispose()
            {
                if (isRender.Length > 0)
                {
                    isRender.Dispose();
                }
            }
        }

        // when a chunk needs to add renderers
        protected override void OnUpdate()
        {
            Entities.WithAll<SpawnChunkRendersCommand>().ForEach((Entity e, ref SpawnChunkRendersCommand command) =>
            {
                SpawnChunkRenders(command);
                command.Dispose();
                World.EntityManager.DestroyEntity(e);
            });
        }

		public static void DestroyChunkRender(EntityManager EntityManager, Entity e)
		{
			if (EntityManager.Exists(e))
			{
				if (EntityManager.HasComponent<ChunkRenderer>(e))
				{
					ChunkRenderer chunkRenderer = EntityManager.GetComponentData<ChunkRenderer>(e);
					chunkRenderer.Dispose();
				}
				if (EntityManager.HasComponent<ChunkSides>(e))
				{
					ChunkSides chunkSides = EntityManager.GetComponentData<ChunkSides>(e);
					chunkSides.Dispose();
				}
                if (EntityManager.HasComponent<RenderMesh>(e))
                {
                    RenderMesh renderer = EntityManager.GetSharedComponentData<RenderMesh>(e);
                    GameObject.Destroy(renderer.mesh);
                }
                else if (EntityManager.HasComponent<ChunkMesh>(e))
                {
                    var renderer = EntityManager.GetSharedComponentData<ChunkMeshLink>(e);
                    GameObject.Destroy(renderer.mesh);
                }
				EntityManager.DestroyEntity(e);
			}
		}


        public static void SpawnChunkRenders(EntityManager EntityManager, Entity world, int renderEntitiesCount, int materialsCount, byte[] isRender)
        {
            SpawnChunkRendersCommand newCommand = new SpawnChunkRendersCommand
            {
                world = world,
                renderEntitiesCount = renderEntitiesCount,
                materialsCount = materialsCount,
                isRender = new BlitableArray<byte>(isRender.Length, Allocator.Persistent)
            };
            for (int i = 0; i < isRender.Length; i++)
            {
                newCommand.isRender[i] = isRender[i];
            }
            Entity e = EntityManager.CreateEntity();
            EntityManager.AddComponentData(e, newCommand);
        }

        public List<Material> GetWorldMaterials(int worldID)
        {
            List<Material> materials = new List<Material>();
            if (worldSpawnSystem.maps.ContainsKey(worldID))
            {
                materials = worldSpawnSystem.maps[worldID].tilemap.materials;
            }
            else if (worldSpawnSystem.models.ContainsKey(worldID))
            {
                materials.Add(Bootstrap.GetVoxelMaterial());
            }
            return materials;
        }

        public void SpawnChunkRenders(SpawnChunkRendersCommand command)
        {
            World world = World.EntityManager.GetComponentData<World>(command.world);
            if (Bootstrap.instance.isUseModels == false) {
                if (world.modelID != 0) {
                    return;
                }
            }
            if (World.EntityManager.HasComponent<ZoxID>(command.world) == false)
            {
                Debug.LogError("World does not have ZOXID but has modelID: " + world.modelID);
                return;
            }
            int worldID = World.EntityManager.GetComponentData<ZoxID>(command.world).id;
            var materials = GetWorldMaterials(worldID);
            //int chunksCount = world.chunks.Length;
            //MapDatam map = null;
            VoxData model = new VoxData();
            NativeArray<Entity> renderEntities = new NativeArray<Entity>(command.renderEntitiesCount * command.materialsCount, Allocator.Temp); 
            if (world.modelID == 0)
            {
                //map = worldSpawnSystem.maps[worldID];
                World.EntityManager.Instantiate(worldChunkRenderPrefab, renderEntities);
            }
            else //if (worldSpawnSystem.models.ContainsKey(worldID))
            {
                model = worldSpawnSystem.models[worldID];
                World.EntityManager.Instantiate(modelChunkRenderPrefab, renderEntities);
            }
            var chunkEntities = world.chunks.ToArray();
            int renderEntityCount = 0;
            for (int i = 0; i < chunkEntities.Length; i++)
            {
                // need to bulk these too
                if (command.isRender[i] == 1)
                {
                    Chunk chunk = World.EntityManager.GetComponentData<Chunk>(chunkEntities[i]);
                    Entity[] renderEntitiesSmall = new Entity[command.materialsCount];
                    for (int j = 0; j < renderEntitiesSmall.Length; j++)
                    {
                        renderEntitiesSmall[j] = renderEntities[renderEntityCount * command.materialsCount + j];
                    }
                    AddRenderEntitiesToChunk(chunkEntities[i], ref chunk, world, materials, chunk.GetVoxelPosition().ToFloat3(), renderEntitiesSmall, model);
                    World.EntityManager.SetComponentData(chunkEntities[i], chunk);
                    renderEntityCount++;
                }
            }
            renderEntities.Dispose();
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
                    var chunkRenderEntity = chunk.chunkRenders[i];
                    ChunkRenderSystem.DestroyChunkRender(World.EntityManager, chunkRenderEntity);
                }
                chunk.chunkRenders = new BlitableArray<Entity>(0, Allocator.Persistent);
            }
        }

        public void AddRenderEntitiesToChunk(World world, Entity chunkEntity, ref Chunk chunk)
        {
            int worldID = World.EntityManager.GetComponentData<ZoxID>(chunk.world).id;
            List<Material> materials = GetWorldMaterials(worldID);
            float3 spawnPosition = World.EntityManager.GetComponentData<Translation>(chunkEntity).Value;
            VoxData model = new VoxData();
            var prefab = worldChunkRenderPrefab;
            if (worldSpawnSystem.models.ContainsKey(worldID))
            {
                model = worldSpawnSystem.models[worldID];
                prefab = modelChunkRenderPrefab;
            }
            Entity[] entities = new Entity[materials.Count];
            for (int i = 0; i < entities.Length; i++)
            {
                entities[i] = World.EntityManager.Instantiate(prefab);
            }
            AddRenderEntitiesToChunk(chunkEntity, ref chunk, world, materials, spawnPosition, entities, model);
        }

        public void AddRenderEntitiesToChunk(Entity chunkEntity, ref Chunk chunk, World world, List<Material> materials, float3 spawnPosition, Entity[] renderEntities, VoxData model)
        {
            if (ChunkSpawnSystem.isDebugLog)
            {
                Debug.LogError("Adding " + materials.Count + " chunk renders to chunk: " + chunk.Value.chunkPosition + "::" + chunk.Value.voxelDimensions);
            }
            chunk.chunkRenders = new BlitableArray<Entity>(materials.Count, Allocator.Persistent);
            for (int j = 0; j < materials.Count; j++)
            {
                //Entity renderEntity = renderEntities[i * materials.Count + j];
                Entity renderEntity = renderEntities[j];
                SetChunkRender(
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
                    chunkRender.isCenter = 1;
                    List<Color> colors = model.GetColors(); //  new List<Color>(); // 
                    chunkRender.voxelColors = new BlitableArray<float3>(colors.Count, Allocator.Persistent);
                    for (int a = 0; a < colors.Count; a++)
                    {
                        chunkRender.voxelColors[a] = new float3(colors[a].r, colors[a].b, colors[a].g);
                    }
                    World.EntityManager.SetComponentData(renderEntity, chunkRender);
                }
                chunk.chunkRenders[j] = renderEntity;
            }
        }
        private void SetChunkRender(Entity chunkRendererEntity, Entity chunkEntity, ref Chunk chunk,
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
            int chunkID = World.EntityManager.GetComponentData<ZoxID>(chunkEntity).id;
            World.EntityManager.SetComponentData(chunkRendererEntity, new ZoxID { id = id, creatorID = chunkID });
            World.EntityManager.SetComponentData(chunkRendererEntity, new Translation { Value = spawnPosition });
            if ( World.EntityManager.HasComponent<Parent>(chunkRendererEntity))
            {
                World.EntityManager.SetComponentData(chunkRendererEntity, new Parent { Value = chunk.world });
            }
            // Render material data
            if (World.EntityManager.HasComponent<RenderMesh>(chunkRendererEntity))
            {
                World.EntityManager.SetComponentData(chunkRendererEntity, new NonUniformScale { Value = new float3(1,1,1) });
                World.EntityManager.SetComponentData(chunkRendererEntity, new Rotation { Value = quaternion.identity });
                var renderer = World.EntityManager.GetSharedComponentData<RenderMesh>(chunkRendererEntity);
                renderer.material = material;
                renderer.subMesh = materialID;
                renderer.mesh = new Mesh();
                renderer.mesh.MarkDynamic();
                World.EntityManager.SetSharedComponentData(chunkRendererEntity, renderer);
            }
            else if (World.EntityManager.HasComponent<ChunkMeshLink>(chunkRendererEntity))
            {
                var renderer = World.EntityManager.GetSharedComponentData<ChunkMeshLink>(chunkRendererEntity);
                renderer.material = material;
                renderer.mesh = new Mesh();
                renderer.mesh.MarkDynamic();
                World.EntityManager.SetSharedComponentData(chunkRendererEntity, renderer);
            }
            AddChunkRenderComponent(chunkRendererEntity, chunkEntity, ref chunk, materialID, skeletonID);
        }
        public void AddChunkRenderComponent(Entity chunkRenderEntity, Entity chunkEntity, ref Chunk chunk, int materialID, int skeletonID)// byte buildState = 0)
        {
            ChunkRenderer chunkRender = new ChunkRenderer { };
            chunkRender.chunk = chunkEntity;
            chunkRender.materialID = (byte)materialID;
            chunkRender.SetMetaData(voxelSpawnSystem.meta, voxelSpawnSystem.voxelIDs);
             //, ChunkSpawnSystem.maxCacheVerts, ChunkSpawnSystem.maxCacheTriangles);
            chunkRender.Value = chunk.Value;
            /*if (skeletonID != 0)
            {
                chunkRender.hasWeights = 1;
                chunkRender.InitializeBoneWeights(ChunkSpawnSystem.maxCacheVerts,
                    worldSpawnSystem.skeletonsMeta[chunk.worldID].data.datas);
            }*/
            if (World.EntityManager.HasComponent<ChunkRenderer>(chunkRenderEntity))
            {
                World.EntityManager.SetComponentData(chunkRenderEntity, chunkRender);
            }
            else
            {
                World.EntityManager.AddComponentData(chunkRenderEntity, chunkRender);
            }
            ChunkSides sides = new ChunkSides();
            sides.Init(chunk.Value.voxelDimensions);
            World.EntityManager.SetComponentData(chunkRenderEntity, sides);

            ChunkMesh chunkMesh = new ChunkMesh();
            chunkMesh.Init(chunk.Value.voxelDimensions);
            World.EntityManager.SetComponentData(chunkRenderEntity, chunkMesh);
        }

    }
}