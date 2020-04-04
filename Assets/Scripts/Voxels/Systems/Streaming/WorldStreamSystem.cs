using Unity.Entities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace Zoxel.Voxels
{
    [DisableAutoCreation]
    public class WorldStreamSystem : ComponentSystem
    {
        public ChunkSpawnSystem chunkSpawnSystem;
        public WorldSpawnSystem worldSpawnSystem;
        public ChunkRenderSystem chunkRenderSystem;

        public static void CreateWorldUpdate(EntityManager EntityManager, Entity world, 
            List<int> worldsChunkIDs, List<int> oldChunkIDs, Dictionary<int, bool> allRenders)
        {
            Entity e = EntityManager.CreateEntity();
            WorldStreamCommand worldStreamCommand = new WorldStreamCommand { };
            worldStreamCommand.world = world;
            worldStreamCommand.SetIDs(worldsChunkIDs, oldChunkIDs);
            worldStreamCommand.SetRenders(allRenders);
            EntityManager.AddComponentData(e, worldStreamCommand);
        }

        protected override void OnUpdate()
        {
            Entities.WithAll<WorldStreamCommand>().ForEach((Entity e, ref WorldStreamCommand command) =>
            {
                UpdateWorld(command);
                command.Dispose();
                World.EntityManager.DestroyEntity(e);
            });
        }

        void UpdateWorld(WorldStreamCommand command)
        {
            // , Dictionary<int, bool> allRenders
            Entity worldEntity = command.world;
            World world = World.EntityManager.GetComponentData<World>(worldEntity);
            var ids = command.newIDs.ToArray();
            if (command.isModel == 1)
            {
                //Debug.LogError("Adding Chunk Renders for World: " + world.id);
                for (int i = 0; i < ids.Length; i++)
                {
                    var chunkEntity = chunkSpawnSystem.chunks[ids[i]];
                    Chunk chunk = World.EntityManager.GetComponentData<Chunk>(chunkEntity);
                    chunkRenderSystem.AddRenderEntitiesToChunk(world, chunkEntity, ref chunk);
                    World.EntityManager.SetComponentData(chunkSpawnSystem.chunks[ids[i]], chunk);
                }
            }
            // for all old worlds that stayed
            else
            {
                var oldIDs = command.oldIDs.ToArray();
                var renders = command.newRenders.ToArray();
                for (int i = 0; i < ids.Length; i++)
                {
                    // if new and old id!
                    bool doesContain = false;
                    for (int j = 0; j < oldIDs.Length; j++)
                    {
                        if (ids[i] == oldIDs[j])
                        {
                            doesContain = true;
                            break;
                        }
                    }
                    if (doesContain)
                    {
                        // now get old render and see if changed
                        Entity chunkEntity = chunkSpawnSystem.chunks[ids[i]];
                        Chunk chunk = World.EntityManager.GetComponentData<Chunk>(chunkEntity);
                        if (chunk.chunkRenders.Length == 0 && renders[i] == 1)
                        {

                            if (World.EntityManager.HasComponent<ChunkBuilder>(chunkEntity))
                            {
                                World.EntityManager.SetComponentData(chunkEntity, new ChunkBuilder { });
                            }
                            else
                            {
                                World.EntityManager.AddComponentData(chunkEntity, new ChunkBuilder { });
                            }
                            if (chunkRenderSystem != null)
                            {
                                chunkRenderSystem.AddRenderEntitiesToChunk(world, chunkEntity, ref chunk);
                            }
                        }
                        else if (chunk.chunkRenders.Length != 0 && renders[i] == 0)
                        {
                            if (chunkRenderSystem != null)
                            {
                                chunkRenderSystem.RemoveRenderEntitiesFromChunk(ref chunk);
                            }
                        }
                        World.EntityManager.SetComponentData(chunkEntity, chunk);
                    }
                }
            }
            World.EntityManager.SetComponentData(worldEntity, world);
        }

        public static void StreamChunksIn(EntityManager EntityManager, ChunkSpawnSystem chunkSpawnSystem,
            bool isModel, Entity worldEntity, ref World world, int3 centerWorldPosition, float renderSize, float loadSize, bool isCentredWorld = true)
        {
            world.centralPosition = centerWorldPosition;
            int boundsSize =  (int)math.max(renderSize, loadSize);
            // Calculate Bounds
            int3 lowerBounds = new int3(0, 0, 0);
            if (isCentredWorld)
            {
                lowerBounds = new int3(-boundsSize, 0, -boundsSize);
            }
            int3 upperBounds = new int3(boundsSize, 0, boundsSize);
            lowerBounds += centerWorldPosition;
            upperBounds += centerWorldPosition;
            int3 spawnChunkPosition;
            HashSet<int3> oldChunkPositions = new HashSet<int3>(world.chunkPositions.ToArray());
            HashSet<int3> onlyPositions = new HashSet<int3>();
            HashSet<int3> worldChunkPositions = new HashSet<int3>(world.chunkPositions.ToArray());    // total positions!
            Dictionary<int, bool> allRenders = new Dictionary<int, bool>();
            Dictionary<int3, int> chunkPositions = new Dictionary<int3, int>();
            List<int> chunkDistances = new List<int>();

            for (int i = 0; i < world.chunkIDs.Length; i++)
            {
                Entity chunkEntity = chunkSpawnSystem.chunks[world.chunkIDs[i]];
                chunkPositions.Add(EntityManager.GetComponentData<Chunk>(chunkEntity).Value.chunkPosition, world.chunkIDs[i]);
            }
            lowerBounds.x = (int)lowerBounds.x;
            lowerBounds.y = (int)lowerBounds.y;
            lowerBounds.z = (int)lowerBounds.z;

            for (spawnChunkPosition.x = lowerBounds.x; spawnChunkPosition.x <= upperBounds.x; spawnChunkPosition.x++)
            {
                for (spawnChunkPosition.y = lowerBounds.y; spawnChunkPosition.y <= upperBounds.y; spawnChunkPosition.y++)
                {
                    for (spawnChunkPosition.z = lowerBounds.z; spawnChunkPosition.z <= upperBounds.z; spawnChunkPosition.z++)
                    {
                        if (world.bounds == 0 ||
                            (spawnChunkPosition.x >= -world.bounds && spawnChunkPosition.x <= world.bounds
                            && spawnChunkPosition.z >= -world.bounds && spawnChunkPosition.z <= world.bounds))
                        {
                            // if doesnt have chunk, add it to list
                            if (!worldChunkPositions.Contains(spawnChunkPosition))
                            {
                                worldChunkPositions.Add(spawnChunkPosition);    // new position added here
                            }
                            else
                            {
                                int3 distance = centerWorldPosition - spawnChunkPosition;
                                distance.x = math.abs(distance.x);
                                distance.z = math.abs(distance.z);
                                int maxDistance = (int)math.max(distance.x, distance.z);
                                chunkDistances.Add(maxDistance);
                                bool isRender = (spawnChunkPosition.x - centerWorldPosition.x) >= -renderSize && (spawnChunkPosition.x - centerWorldPosition.x) <= renderSize
                                    && (spawnChunkPosition.z - centerWorldPosition.z) >= -renderSize && (spawnChunkPosition.z - centerWorldPosition.z) <= renderSize;
                                allRenders.Add(chunkPositions[spawnChunkPosition],
                                    (isModel || isRender)); //maxDistance != renderSize));
                            }
                            // in another list add all chunk positions needed - this should be done per stream position
                            onlyPositions.Add(spawnChunkPosition);
                        }
                    }
                }
            }
            // in another system - spawn in chunks
            // Adding New Chunks!
            List<int> worldsChunkIDs = new List<int>(); // total ids // world.chunkIDs.ToArray()
            for (int i = 0; i < world.chunkIDs.Length; i++)
            {
                worldsChunkIDs.Add(world.chunkIDs[i]);
            }
            List<int3> newPositions = new List<int3>();
            List<bool> newRenders = new List<bool>();
            // only for updates, so not for first chunks - basically not for models
            //for (int i = 0; i < worldChunkPositions.Count; i++)
            foreach (var worldChunkPosition in worldChunkPositions)
            {
                // if doesn't have chunk position already existing
                if (!oldChunkPositions.Contains(worldChunkPosition))
                {
                    newPositions.Add(worldChunkPosition);
                    bool isRender = (worldChunkPosition.x - centerWorldPosition.x) >= -renderSize && (worldChunkPosition.x - centerWorldPosition.x) <= renderSize
                        && (worldChunkPosition.z - centerWorldPosition.z) >= -renderSize && (worldChunkPosition.z - centerWorldPosition.z) <= renderSize;
                    newRenders.Add(isModel || isRender);
                }
            }
            // finally, spawn the chunks
            int[] newIDs = chunkSpawnSystem.SpawnChunks(worldEntity, newPositions.ToArray(), newRenders.ToArray());
            worldsChunkIDs.AddRange(newIDs); // add new ids here for added chunk positions
            // Remove chunks that arn't in onlyPositions list
            if (worldsChunkIDs.Count != worldChunkPositions.Count)
            {
                Debug.LogError("worldsChunkIDs count: " + worldsChunkIDs.Count + " not equal to worldCHunkPositions Count: " + worldChunkPositions.Count);
                return;
            }
            List<int> removingChunkIDs = new List<int>();
            List<int3> removingPositions = new List<int3>();
            //for (int i = 0; i < worldChunkPositions.Count; i++)
            int h = 0;
            foreach (var worldChunkPosition in worldChunkPositions)
            {
                // if not in main location, remove the chunk
                if (!onlyPositions.Contains(worldChunkPosition))
                {
                    //Debug.LogError("Removing Chunk: " + worldChunkPositions[i].ToString() + ":::" + worldsChunkIDs[i]);
                    removingChunkIDs.Add(worldsChunkIDs[h]);
                    removingPositions.Add(worldChunkPosition);
                }
                h++;
            }
            chunkSpawnSystem.RemoveChunks(ref world, removingChunkIDs.ToArray(), removingPositions.ToArray());
            // now remove the ids from list
            for (int i = 0; i < removingChunkIDs.Count; i++)
            {
                if (worldsChunkIDs.Contains(removingChunkIDs[i]))
                {
                    int index = worldsChunkIDs.IndexOf(removingChunkIDs[i]);
                    worldsChunkIDs.RemoveAt(index);
                    int j = 0;
                    foreach (var positoin in worldChunkPositions) {
                        if (index == j) {
                            worldChunkPositions.Remove(positoin);
                            break;
                        }
                        j++;
                    }
                    //worldChunkPositions.RemoveAt(index);
                }
            }
            // create new arrays for world
            List<int> oldChunkIDs = new List<int>(world.chunkIDs.ToArray());
            world.chunkIDs = new BlitableArray<int>(worldsChunkIDs.Count, Unity.Collections.Allocator.Persistent);
            for (int i = 0; i < world.chunkIDs.Length; i++)
            {
                world.chunkIDs[i] = worldsChunkIDs[i];
            }
            world.chunkPositions = new BlitableArray<int3>(worldChunkPositions.Count, Unity.Collections.Allocator.Persistent);
            //for (int i = 0; i < world.chunkPositions.Length; i++)
            int a = 0;
            foreach (var position in worldChunkPositions)
            {
                world.chunkPositions[a] = position; //worldChunkPositions[i];
                a++;
            }
            WorldStreamSystem.CreateWorldUpdate(EntityManager, worldEntity, worldsChunkIDs, oldChunkIDs, allRenders);
        }


    }
}

/*Debug.DrawLine(chunk.GetVoxelPosition(),
    chunk.GetVoxelPosition() + new float3(8, 64, 8),
    Color.green, 4);*/
/*Debug.DrawLine(chunk.GetVoxelPosition(), 
    chunk.GetVoxelPosition() + new float3(8, 64, 8),
    Color.red, 4);*/
