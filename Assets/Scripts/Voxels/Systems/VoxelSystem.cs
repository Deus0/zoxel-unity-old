using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Rendering;
using UnityEngine;
using System.Collections.Generic;

namespace Zoxel.Voxels
{
    [DisableAutoCreation]
    public class VoxelSpawnSystem : ComponentSystem
    {
        // data
        public WorldSpawnSystem worldSpawnSystem;
        public ChunkSpawnSystem chunkSpawnSystem;
        // queue
        public static List<float3> commandsPositions = new List<float3>();
        public static List<int> commandsTypes = new List<int>();
        public static List<Entity> commandsWorlds = new List<Entity>();

        public List<int> voxelIDs = new List<int>();
        public Dictionary<int, VoxelDatam> meta = new Dictionary<int, VoxelDatam>();

        public void Clear()
        {
            commandsPositions.Clear();
            commandsTypes.Clear();
        }


        public static void QueueVoxel(float3 spawnPosition, Entity world, int voxelID)
        {
            commandsWorlds.Add(world);
            commandsPositions.Add(new float3(math.floor(spawnPosition.x), math.floor(spawnPosition.y), math.floor(spawnPosition.z)));
            commandsTypes.Add(voxelID);
        }

        protected override void OnUpdate()
        {
            //Debug.LogError("Running VoxelSpawnSystem.");
            if (commandsPositions.Count > 0)
            {
                int commandIndex = 0;// commandsPositions.Count - 1;
                float3 position = commandsPositions[commandIndex];
                int type = commandsTypes[commandIndex];
                var world = commandsWorlds[commandIndex];
                SpawnVoxel(new int3(position), type, world);
                commandsPositions.RemoveAt(commandIndex);
                commandsTypes.RemoveAt(commandIndex);
                commandsWorlds.RemoveAt(commandIndex);
            }
        }

        void SpawnVoxel(int3 spawnPosition, int voxelID, Entity world)
        {
            var voxelDimensions = World.EntityManager.GetComponentData<World>(world).voxelDimensions;
            var chunkPosition = VoxelRaycastSystem.GetChunkPosition(spawnPosition, voxelDimensions);// new float3(spawnPosition.x / 16, spawnPosition.y / 16, spawnPosition.z / 16);
            var localPosition = VoxelRaycastSystem.GetLocalPosition(spawnPosition, chunkPosition, voxelDimensions);
            //Debug.LogError("Spawning voxel of Type: " + spawnType + " M: " + spawnPosition.ToString() + " C: " + chunkPosition.ToString() + " L:" + localPosition);
            // get chunk that position is within
            // get chunk
            // todo: store chunks in UniqueKey<(worldID, chunkPosition)> as keys) - 4 numbers to generate a unique key?
            Entity foundChunk = new Entity();
            Chunk writeToChunk = new Chunk();
            bool didFindChunk = false;
            foreach (Entity e in chunkSpawnSystem.chunks.Values)
            {
                Chunk chunk = World.EntityManager.GetComponentData<Chunk>(e);
                if (chunk.Value.chunkPosition.x == chunkPosition.x && chunk.Value.chunkPosition.y == chunkPosition.y && chunk.Value.chunkPosition.z == chunkPosition.z)
                {
                    foundChunk = e;
                    writeToChunk = chunk;
                    didFindChunk = true;
                    //return chunk.Value.voxels[GetVoxelArrayIndex(localChunkPosition)];
                }
            }
            if (!didFindChunk)
            {
                //Debug.LogError("Could not find chunk: " + chunkPosition.ToString());
                return;
            }
            // get index out of spawnPosition
            int index = VoxelRaycastSystem.GetVoxelArrayIndex(localPosition, voxelDimensions);
            //if (didFindChunk)
            {
                //Debug.LogError("Found Chunk: " + chunkPosition.ToString() + " with voxelIndex: " + index);
            }
            //Debug.LogError("    chunkPosition: " + chunkPosition.ToString() + " index at " + index);
            // get index from voxel list
            int voxelIndex = voxelIDs.IndexOf(voxelID) + 1; // add one for air
            writeToChunk.Value.voxels[index] = (byte) (voxelIndex);
            //writeToChunk.isDirty = 1;
            World.EntityManager.SetComponentData(foundChunk, writeToChunk);


            if (World.EntityManager.HasComponent<ChunkBuilder>(foundChunk))
            {
                World.EntityManager.SetComponentData(foundChunk, new ChunkBuilder { });
            }
            else
            {
                World.EntityManager.AddComponentData(foundChunk, new ChunkBuilder { });
            }
        }
    }
}