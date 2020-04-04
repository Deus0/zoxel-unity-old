using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Rendering;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Zoxel.Voxels
{


    public struct WorldChunkMap {
        public Dictionary<int3, Entity> chunks;
    }

    /// <summary>
    /// Spawn a world
    /// Worlds hold collections of chunks
    ///     Rename to VoxSystem - Vox are just models made of voxels
    /// </summary>
    [DisableAutoCreation]
    public class WorldSpawnSystem : ComponentSystem
    {
        // ecs
        private EntityArchetype worldArchtype;
        public ChunkSpawnSystem chunkSpawnSystem;
        public Dictionary<int, Entity> worlds = new Dictionary<int, Entity>();
        public Dictionary<int, WorldChunkMap> worldLookups = new Dictionary<int, WorldChunkMap>();

        public Dictionary<int, MapDatam> maps = new Dictionary<int, MapDatam>();
        public Dictionary<int, VoxData> models = new Dictionary<int, VoxData>();

        public Dictionary<int, MapDatam> mapsMeta = new Dictionary<int, MapDatam>();
        public Dictionary<int, VoxData> modelsMeta = new Dictionary<int, VoxData>();    // when using a metaID - remove this in future
        public Dictionary<int, SkeletonDatam> skeletonsMeta = new Dictionary<int, SkeletonDatam>();

        // should be maps and vox models being spawned by this system
        public Dictionary<int, VoxData> voxData = new Dictionary<int, VoxData>();

        protected override void OnCreate()
        {
            worldArchtype = World.EntityManager.CreateArchetype(
                typeof(World),
                typeof(ZoxID),
                typeof(Translation),
                typeof(NonUniformScale),
                typeof(Rotation),
                typeof(LocalToWorld)
            );
        }

        protected override void OnUpdate()
        {
            Entities.WithAll<SpawnMapCommand>().ForEach((Entity e, ref SpawnMapCommand command) =>
            {
                SpawnWorld(command.spawnID, command.spawnPosition, mapsMeta[command.mapID], command.game);
                World.EntityManager.DestroyEntity(e);
            });
            Entities.WithAll<RemoveMap>().ForEach((Entity e, ref RemoveMap command) =>
            {
                DestroyWorld(command.map);
                World.EntityManager.DestroyEntity(e);
            });
            Entities.WithAll<UpdateModelCommand>().ForEach((Entity e, ref UpdateModelCommand command) =>
            {
                SkeletonDatam skeletonDatam = null;
                if (skeletonsMeta.ContainsKey(command.skeletonID))
                {
                    skeletonDatam = skeletonsMeta[command.skeletonID];
                }
                UpdateModel(command.entity, command.model, skeletonDatam, command.spawnID);
                World.EntityManager.DestroyEntity(e);
            });
        }

        #region Model
        public Entity SpawnModel(float3 spawnPosition, VoxDatam model)
        {
            if (model != null)
            {
                int id = Bootstrap.GenerateUniqueID();
                Entity world = World.EntityManager.CreateEntity(worldArchtype);
                worlds.Add(id, world);
                worldLookups.Add(id,new WorldChunkMap{ chunks = new Dictionary<int3, Entity>() });
                World.EntityManager.SetComponentData(world, new Translation { Value = spawnPosition });
                UpdateModel(world, model.data, null, id);
                return world;
            }
            else 
            {
                return new Entity();
            }
        }

        private void UpdateModel(Entity world, VoxData model, SkeletonDatam skeleton, int id)
        {
            if (worlds.ContainsKey(id) == false)
            {
                //Debug.LogError("Updating Model: " + id);
                worlds.Add(id, world);
                worldLookups.Add(id, new WorldChunkMap{ chunks = new Dictionary<int3, Entity>() });
            } 
            else {
                //Debug.LogError("Removing previous world: " + id);
                DestroyWorldWeakly(id);
                worlds.Add(id, world);
                worldLookups.Add(id, new WorldChunkMap{ chunks = new Dictionary<int3, Entity>() });
            }
            if (models.ContainsKey(id))
            {
                models[id] = model;
            }
            else
            {
                models.Add(id, model);
            }
            float3 scale = new float3(model.scale.x / 16f, model.scale.y / 16f, model.scale.z / 16f);
            World.EntityManager.SetComponentData(world, new ZoxID { id = id });
            World worldComponent = new World
            {
                //chunkIDs = new BlitableArray<int>(0, Unity.Collections.Allocator.Persistent),
                //chunkPositions = new BlitableArray<int3>(0, Unity.Collections.Allocator.Persistent),
                scale = scale,
                voxelDimensions = model.size,
                modelID = model.id
            };
            if (skeleton != null)
            {
                //worldComponent.skeletonID = skeleton.data.id;
            }
            float renderDistance = 0;
            if (World.EntityManager.HasComponent<World>(world) == false)
            {
                World.EntityManager.AddComponentData(world, worldComponent);
            }
            else
            {
                World.EntityManager.SetComponentData(world, worldComponent);
            }
            WorldStreamSystem.StreamChunksIn(
                World.EntityManager, 
                chunkSpawnSystem, 
                true, 
                world,
                ref worldComponent,
                int3.Zero(),
                renderDistance,
                renderDistance,
                false);
            World.EntityManager.SetComponentData(world, worldComponent);
        }

        #endregion

        #region Maps

        /*public int GetFirstWorldID()
        {
            foreach (KeyValuePair<int, Entity> KVP in worlds)
            {
                return KVP.Key;
            }
            return 0;
        }*/

        public int SpawnMap(MapDatam map, Entity game)
        {
            int newID = Bootstrap.GenerateUniqueID();
            SpawnWorld(newID, map.worldPosition, map, game);
            return newID;
        }
        private void UpdateMap(Entity worldEntity, MapDatam map, int id)
        {
            if (maps.ContainsKey(id))
            {
                maps[id] = map;
            }
            else
            {
                maps.Add(id, map);
            }
            // initiate the biomes with tilemap values
            for (int i = 0; i < map.biomes.Count; i++)
            {
                map.biomes[i].InitializeIDs(map.tilemap);
            }
            World.EntityManager.SetComponentData(worldEntity, new Rotation { Value = Quaternion.Euler(map.worldRotation) });
            World.EntityManager.SetComponentData(worldEntity, new NonUniformScale { Value = map.worldScale });
            World world = new World
            {
                chunkIDs = new BlitableArray<int>(0, Unity.Collections.Allocator.Persistent),
                chunkPositions = new BlitableArray<int3>(0, Unity.Collections.Allocator.Persistent),
                scale = map.worldScale,
                voxelDimensions = map.voxelDimensions,
                bounds = 32
            };
            World.EntityManager.SetComponentData(worldEntity, world);
            World.EntityManager.SetComponentData(worldEntity, new ZoxID { id = id });
            if (Application.isPlaying == false)
            {
                WorldStreamSystem.StreamChunksIn(World.EntityManager,chunkSpawnSystem, world.modelID != 0,
                    worldEntity, ref world, int3.Zero(), Bootstrap.GetRenderDistance(), Bootstrap.GetLoadDistance());
            }
        }
        #endregion

        #region Streaming
        public void OnAddedStreamer(Entity player, int worldID)
        {
            if (worlds.ContainsKey(worldID))
            {
                OnAddedStreamer(player, worlds[worldID]);
            }
        }

        public void OnAddedStreamer(Entity player, Entity worldEntity)
        {
            Translation position = World.EntityManager.GetComponentData<Translation>(player);
            World world = World.EntityManager.GetComponentData<World>(worldEntity);
            int3 chunkPosition = VoxelRaycastSystem.GetChunkPosition(
                VoxelRaycastSystem.WorldPositionToVoxelPosition(position.Value),
                world.voxelDimensions);
            chunkPosition.y = 0;
            WorldStreamSystem.StreamChunksIn(World.EntityManager, chunkSpawnSystem, world.modelID != 0, 
                worldEntity, ref world, chunkPosition, Bootstrap.GetRenderDistance(), Bootstrap.GetLoadDistance());
            World.EntityManager.SetComponentData(worldEntity, world);
        }

        public void SetWorldPosition(int streamerID, Entity worldEntity, int3 newCentralPosition)
        {
            if (World.EntityManager.Exists(worldEntity))
            {
                World world = World.EntityManager.GetComponentData<World>(worldEntity);
                WorldStreamSystem.StreamChunksIn(World.EntityManager,chunkSpawnSystem, world.modelID != 0, 
                    worldEntity, ref world, newCentralPosition, Bootstrap.GetRenderDistance(), Bootstrap.GetLoadDistance());
                World.EntityManager.SetComponentData(worldEntity, world);
            }
        }


        /// <summary>
        /// todo:
        ///     Make sure to update chunk types here
        ///     edge chunks are non visible so have no chunk renders
        ///     other chunks are visible
        ///     
        /// later on:
        ///     add subdivision to models of chunks
        ///     chunks on inner edge are subdivided more to render less triangles
        ///     ones in middle are highest definition
        /// </summary>
        #endregion
        //for (int i = 0; i < newIDs.Length; i++)
        //{
        //Chunk chunk = World.EntityManager.GetComponentData<Chunk>(chunkSpawnSystem.chunks[newIDs[i]]);
        //Debug.DrawLine(chunk.GetVoxelPosition(),
        //    chunk.GetVoxelPosition() + new float3(8, 64, 8), Color.cyan, 4);
        //}

        #region Spawning-Removing

        public struct SpawnMapCommand : IComponentData
        {
            public int mapID;
            public int spawnID;
            public Entity game;
            public float3 spawnPosition;
        }

        public struct RemoveMap : IComponentData
        {
            public Entity map;
        }

        public struct UpdateModelCommand : IComponentData
        {
            public int spawnID;
            public Entity entity;
            public VoxData model;
            public int skeletonID;
        }

        public int QueueMap(float3 spawnPosition, MapDatam map, Entity game)
        {
            int newID = Bootstrap.GenerateUniqueID();
            Entity e = World.EntityManager.CreateEntity();
            World.EntityManager.AddComponentData(e, new SpawnMapCommand
            {
                spawnID = newID,
                mapID = map.id,
                spawnPosition = spawnPosition,
                game = game
            });
            return newID;
        }

        public static void QueueUpdateModel(EntityManager EntityManager, Entity entity, int spawnID, VoxData model, int skeletonID = 0)
        {
            Entity e = EntityManager.CreateEntity();
            EntityManager.AddComponentData(e, new UpdateModelCommand
            {
                entity = entity,
                spawnID = spawnID,
                model = model,
                skeletonID = skeletonID
            });
        }

        private void SpawnWorld(int id, float3 spawnPosition, MapDatam map, Entity gameEntity)
        {
            if (map != null)
            {
                Entity world = World.EntityManager.CreateEntity(worldArchtype);
                worlds.Add(id, world);
                worldLookups.Add(id,new WorldChunkMap{ chunks = new Dictionary<int3, Entity>() });
                World.EntityManager.SetComponentData(world, new Translation { Value = spawnPosition });
                UpdateMap(world, map, id);
                Game game = World.EntityManager.GetComponentData<Game>(gameEntity);
                game.map = world;
                World.EntityManager.SetComponentData(gameEntity, game);
            }
            else
            {
                Debug.LogError("Cannot spawn world at: " + spawnPosition.ToString() + " as map is null.");
            }
        }
        public void Clear()
        {
            if (chunkSpawnSystem != null)
            {
                chunkSpawnSystem.Clear();
            }
            //for (int i = worlds.Count - 1; i >= 0; i--)
            foreach (Entity e in worlds.Values)
            {
                if (World.EntityManager.Exists(e))
                {
                    World.EntityManager.DestroyEntity(e);
                }
            }
            worlds.Clear();
            worldLookups.Clear();
        }

        public void DestroyWorld(Entity worldEntity)
        {
            if (World.EntityManager.Exists(worldEntity) == false)
            {
                return;
            }
            int worldID = World.EntityManager.GetComponentData<ZoxID>(worldEntity).id;
            chunkSpawnSystem.DestroyWorld(worldEntity);
            if (World.EntityManager.Exists(worldEntity))
            {
                if (World.EntityManager.HasComponent<World>(worldEntity))
                {
                    World world = World.EntityManager.GetComponentData<World>(worldEntity);
                    world.Dispose();
                    /*for (int i = 0; i < world.chunkIDs.Length; i++)
                    {
                        chunkSpawnSystem.RemoveChunk(world.chunkIDs[i]);
                    }*/
                }
                World.EntityManager.DestroyEntity(worldEntity);
            }
            worlds.Remove(worldID);
            worldLookups.Remove(worldID);
        }

        public void DestroyWorldWeakly(int worldID)
        {
            if (worlds.ContainsKey(worldID))
            {
                if (World.EntityManager.HasComponent<World>(worlds[worldID]))
                {
                    World world = World.EntityManager.GetComponentData<World>(worlds[worldID]);
                    for (int i = 0; i < world.chunks.Length; i++)
                    {
                        chunkSpawnSystem.RemoveChunk(world.chunks[i]);
                    }
                }
                worlds.Remove(worldID);
                worldLookups.Remove(worldID);
            }
        }
        #endregion

    }
}
