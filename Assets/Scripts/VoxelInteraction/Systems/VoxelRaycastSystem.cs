using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.InputSystem;
using Unity.Entities;

namespace Zoxel.Voxels
{
    public struct VoxelRaycaster : IComponentData
    {
        public byte hasHitVoxels;       // 1 if hit, set to 0 after hit, it will then check again
        public float3 voxelPosition;

    }

    [DisableAutoCreation]
    public class VoxelRaycastSystem : ComponentSystem
    {
        public CameraSystem cameraSystem;
        public WorldSpawnSystem worldSpawnSystem;
        public ChunkSpawnSystem chunkSpawnSystem;
        private float raycastLength = 8;
        public static float raycastInterval = 0.05f;
        public static float3 failedPosition = new float3(0, -10000, 0);
        //private static int randRange = 2;
        //private static int randRangeAdd = 2;
        private static List<Entity> commandWorlds = new List<Entity>();
        private static List<Entity> cameraIDs = new List<Entity>();
        private static List<float3> commandScreenPositions = new List<float3>();
        private static List<int3> commandsVoxelDimensions = new List<int3>();
        public static Dictionary<int, float3> commandOutputPositions = new Dictionary<int, float3>();
        //Dictionary<float3, Chunk> chunksLoaded = new Dictionary<float3, Chunk>();

        public static float3 PullPosition(int commandID)
        {
            float3 voxelPosition = commandOutputPositions[commandID];
            commandOutputPositions.Remove(commandID);
            return voxelPosition;
        }

        public int QueueRaycast(float2 screenPosition, Entity world, Entity camera)
        {
            int id = Bootstrap.GenerateUniqueID();
            /*commandWorlds.Add(id);
            commandScreenPositions.Add(new float3(screenPosition.x, screenPosition.y, 0));
            cameraIDs.Add(camera);
            commandsVoxelDimensions.Add(World.EntityManager.GetComponentData<World>(worldSpawnSystem.worlds[worldID]).voxelDimensions);*/
            return id;
        }

        protected override void OnUpdate()
        {
            //chunksLoaded.Clear();
            /*while (commandIDs.Count > 0)
            {
                int index = commandIDs.Count - 1;
                int commandID = commandIDs[index];
                float3 screenPosition = commandScreenPositions[index];
                float3 voxelDimensions = commandsVoxelDimensions[index];
                var cameraID = cameraIDs[index];
                commandIDs.RemoveAt(index);
                commandScreenPositions.RemoveAt(index);
                commandsVoxelDimensions.RemoveAt(index);
                cameraIDs.RemoveAt(index);
                if (World.EntityManager.Exists(cameraID))//CameraSystem.cameras.ContainsKey(cameraID))
                {
                    ZoxID zoxID = World.EntityManager.GetComponentData<ZoxID>(cameraID);
                    Camera cam = cameraSystem.cameraObjects[zoxID.id].GetComponent<Camera>();
                    if (cam == null)
                    {
                        continue;   // camera removed from object..
                    }
                    Ray ray = cam.ScreenPointToRay(screenPosition);
                    // now convert ray to empty voxel
                    float3 rayHit;
                    float3 voxelPosition = GetRayHitVoxel(ray, out rayHit, voxelDimensions);
                    if (!(voxelPosition.x == -1 && voxelPosition.y == -1 && voxelPosition.z == -1))
                    {
                        voxelPosition += new float3(0.5f, 0.5f, 0.5f);//0.5f
                        commandOutputPositions.Add(commandID, voxelPosition);
                    }
                    else
                    {
                        commandOutputPositions.Add(commandID, failedPosition);
                    }
                }
            }*/
        }


        public int3 GetRayHitVoxel(Ray ray, out float3 rayHit, int3 voxelDimensions)
        {
            //float3 previousVoxelTranslation;// = ray.origin;
            rayHit = float3.zero;
            for (float i = 0; i < raycastLength; i += raycastInterval)
            {
                // get voxel position
                rayHit = ray.origin + ray.direction * i;
                var voxelPosition = WorldPositionToVoxelPosition(rayHit);
                // voxel
                byte vox = GetVoxel(voxelPosition, voxelDimensions);
                if (vox != 0)
                {
                    return voxelPosition + int3.Up();
                }
               // previousVoxelTranslation = voxelPosition;
            }
            //Debug.LogError("Could not find a non 0 voxel.");
            return new int3(-1, -1, -1);
        }

        private byte GetVoxel(int3 voxelWorldPosition, int3 voxelDimensions)
        {
            var chunkPosition = GetChunkPosition(voxelWorldPosition, voxelDimensions);
            var localChunkPosition = GetLocalPosition(voxelWorldPosition, chunkPosition, voxelDimensions);
            /*if (chunksLoaded.ContainsKey(chunkPosition))
            {
                var cdawgs = chunksLoaded[chunkPosition];
                int voxelIndex = GetVoxelArrayIndex(localChunkPosition, voxelDimensions);
                if (voxelIndex < cdawgs.Value.voxels.Length && voxelIndex >= 0)
                {
                    return cdawgs.Value.voxels[voxelIndex];
                }
                else
                {
                    Debug.LogError("Voxel index out of range");
                    chunksLoaded.Remove(chunkPosition);
                }
            }*/
           // if (chunksLoaded.ContainsKey(chunkPosition))
            {
                foreach (Entity e in chunkSpawnSystem.chunks.Values)
                {
                    Chunk chunk = World.EntityManager.GetComponentData<Chunk>(e);
                    if (chunk.Value.chunkPosition.x == chunkPosition.x && chunk.Value.chunkPosition.y == chunkPosition.y && chunk.Value.chunkPosition.z == chunkPosition.z)
                    {
                        //chunksLoaded.Add(chunkPosition, chunk);
                        int voxelArrayIndex = GetVoxelArrayIndex(localChunkPosition, voxelDimensions);
                        if (voxelArrayIndex >= 0 && voxelArrayIndex < chunk.Value.voxels.Length)
                        {
                            return chunk.Value.voxels[voxelArrayIndex];
                        }
                        else
                        {
                            return 0;
                        }
                    }
                }
            }
            return 0;
        }

        public static int GetVoxelArrayIndex(int3 position, int3 size)
        {
            return (position.z + size.z * (position.y + size.y * position.x));//(voxelDimensions.z * voxelDimensions.y * (localChunkPosition.x) + voxelDimensions.z * (localChunkPosition.y) + (localChunkPosition.z));
        }

        /*public static int GetVoxelArrayIndexWeird(int3 localChunkPosition, int3 voxelDimensions)
        {
            return (voxelDimensions.x * voxelDimensions.z * (localChunkPosition.x) + voxelDimensions.y * (localChunkPosition.y) + (localChunkPosition.z));
        }*/

        // should use voxel dimensions..
        public static int3 GetLocalPosition(int3 voxelWorldPosition, int3 chunkPosition, int3 voxelDimensions)
        {
            return voxelWorldPosition - chunkPosition * 16;
        }
        public static int3 GetChunkPosition(int3 voxelWorldPosition, int3 voxelDimensions)
        {
            return new int3(
                (int)math.floor(voxelWorldPosition.x / ((float)voxelDimensions.x)),
                (int)math.floor(voxelWorldPosition.y / ((float)voxelDimensions.y)),
                (int)math.floor(voxelWorldPosition.z / ((float)voxelDimensions.z)));
        }
        
        public static int3 WorldPositionToVoxelPosition(float3 worldPosition)
        {
            return new int3(
                (int)math.floor(worldPosition.x),
                (int)math.floor(worldPosition.y),
                (int)math.floor(worldPosition.z));
        }
    }
}