using Unity.Entities;
using Unity.Mathematics;
using Zoxel.Voxels;
using Unity.Transforms; 

namespace Zoxel
{
    [DisableAutoCreation, UpdateBefore(typeof(VoxelCollisionSystem))]
    public class CharacterVoxelPositionSystem : ComponentSystem
    {
        public WorldSpawnSystem worldSpawnSystem;
        public ChunkSpawnSystem chunkSpawnSystem;

        protected override void OnUpdate()
        {
            Entities.WithAll<WorldBound, Translation>().ForEach((Entity e, ref WorldBound worldBound, ref Translation translation) =>
            {
                //worldBound.enabled = 1;
                float3 positionOfMe = translation.Value;//- worldBound.lastNoise;
                if (worldSpawnSystem.worlds.ContainsKey(worldBound.worldID) == false)
                {
                    return;
                }
                Entity worldEntity = worldSpawnSystem.worlds[worldBound.worldID];
                if (worldBound.enabled == 0)
                {
                    worldBound.worldTransform = World.EntityManager.GetComponentData<LocalToWorld>(worldEntity).Value;
                    float3 newPosition2 = math.transform(math.inverse(worldBound.worldTransform), positionOfMe);
                    var voxelPositionIn = VoxelRaycastSystem.WorldPositionToVoxelPosition(newPosition2);
                    Chunk chunk;
                    var chunkPosition = VoxelRaycastSystem.GetChunkPosition(voxelPositionIn, worldBound.voxelDimensions);
                    Voxels.World thisworld = World.EntityManager.GetComponentData<Voxels.World>(worldEntity);
                    if (GetChunk(thisworld, chunkPosition, out chunk))
                    {
                        if (chunk.isGenerating == 0)
                        {
                            worldBound.enabled = 1;
                        }
                        else 
                        { 
                            return;
                        }
                    }
                    else
                    {
                        return;
                    }
                }
                float3 positionOffset = - new float3(0, math.floor(worldBound.size.y), 0); // math.floor
                float3 newPosition = math.transform(math.inverse(worldBound.worldTransform), positionOfMe) + positionOffset;
                var voxelPosition = VoxelRaycastSystem.WorldPositionToVoxelPosition(newPosition);
                worldBound.voxelPosition = voxelPosition;
                var voxelPositionLeft = VoxelRaycastSystem.WorldPositionToVoxelPosition(newPosition + new float3(-worldBound.size.x, 0, 0));
                var voxelPositionRight = VoxelRaycastSystem.WorldPositionToVoxelPosition(newPosition + new float3(worldBound.size.x, 0, 0));
                var voxelPositionForward = VoxelRaycastSystem.WorldPositionToVoxelPosition(newPosition + new float3(0, 0, worldBound.size.z));
                var voxelPositionBack = VoxelRaycastSystem.WorldPositionToVoxelPosition(newPosition + new float3(0, 0, -worldBound.size.z));
                if (worldBound.voxelPositionLeft == voxelPositionLeft && 
                    worldBound.voxelPositionRight == voxelPositionRight && 
                    worldBound.voxelPositionForward == voxelPositionForward && 
                    worldBound.voxelPositionBack == voxelPositionBack)
                {
                    return;
                }
                worldBound.voxelPositionLeft = voxelPositionLeft;
                worldBound.voxelPositionRight = voxelPositionRight;
                worldBound.voxelPositionForward = voxelPositionForward;
                worldBound.voxelPositionBack = voxelPositionBack;
                Voxels.World world = World.EntityManager.GetComponentData<Voxels.World>(worldEntity);
                worldBound.voxelTypeLeft = GetVoxelType(world, worldBound, voxelPositionLeft);
                worldBound.voxelTypeRight = GetVoxelType(world, worldBound, voxelPositionRight);
                worldBound.voxelTypeForward = GetVoxelType(world, worldBound, voxelPositionForward);
                worldBound.voxelTypeBack = GetVoxelType(world, worldBound, voxelPositionBack);
                worldBound.voxelTypeLeftBelow = GetVoxelType(world, worldBound, voxelPositionLeft + int3.Down());// new float3(0, -1, 0));
                worldBound.voxelTypeRightBelow = GetVoxelType(world, worldBound, voxelPositionRight + int3.Down());// new float3(0, -1, 0));
                worldBound.voxelTypeForwardBelow = GetVoxelType(world, worldBound, voxelPositionForward + int3.Down());// new float3(0, -1, 0));
                worldBound.voxelTypeBackBelow = GetVoxelType(world, worldBound, voxelPositionBack +int3.Down());//  new float3(0, -1, 0));
            });
        }

        private bool GetChunk(Voxels.World world, int3 chunkPosition, out Chunk chunk)
        {
            for (int i = 0; i < world.chunkIDs.Length; i++)
            {
                if (world.chunkPositions[i] == chunkPosition)
                {
                    if (chunkSpawnSystem.chunks.ContainsKey(world.chunkIDs[i]))
                    {
                        chunk = World.EntityManager.GetComponentData<Chunk>(chunkSpawnSystem.chunks[world.chunkIDs[i]]);
                        return true;
                    }
                    else
                    {
                        chunk = new Chunk();
                        return false;
                    }
                }
            }
            chunk = new Chunk();
            return false;
        }

        private byte GetVoxelType(Voxels.World world, WorldBound worldBound, int3 voxelPosition)
        {
            var chunkPosition = VoxelRaycastSystem.GetChunkPosition(voxelPosition, worldBound.voxelDimensions);
            Chunk chunk;
            if (GetChunk(world, chunkPosition, out chunk) && chunk.Value.voxels.Length > 0)
            {
                if (chunk.isGenerating == 1)
                {
                    return 1;
                }
                int voxelIndex = VoxelRaycastSystem.GetVoxelArrayIndex(voxelPosition - chunk.GetVoxelPosition(), worldBound.voxelDimensions);
                if (voxelIndex >= chunk.Value.voxels.Length)
                {
                    return 1;
                }
                return chunk.Value.voxels[voxelIndex];
            }
            return 1;
        }

       /* private bool IsSame(float3 positionA, float3 positionB)
        {
            return positionA.x == positionB.x &&
                positionA.y == positionB.y &&
                positionA.z == positionB.z;
        }*/

    }
}
