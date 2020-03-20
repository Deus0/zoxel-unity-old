using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Transforms;
using Unity.Burst;
using Zoxel.Voxels;

namespace Zoxel
{
    // should be character interactions with voxels

    [DisableAutoCreation]
    public class WorldBoundSystem : JobComponentSystem
    {
        [BurstCompile]
        struct WorldBoundJob : IJobForEach<WorldBound, Translation>
        {

            public void Execute(ref WorldBound worldBound, ref Translation position)
            {
                if (worldBound.worldID == 0)
                {
                    return;
                }
                var voxelPosition = worldBound.voxelPosition;
                var chunkPosition = VoxelRaycastSystem.GetChunkPosition(voxelPosition, worldBound.voxelDimensions);
                float3 voxelPositionMax = new float3(
                    worldBound.voxelDimensions.x * 32,  // world size
                    worldBound.voxelDimensions.y * 32,
                    worldBound.voxelDimensions.z * 32);

                if (position.Value.x > voxelPositionMax.x + worldBound.voxelDimensions.x / 2f)
                {
                    position.Value = new float3(voxelPositionMax.x + worldBound.voxelDimensions.x / 2f, position.Value.y, position.Value.z);
                }
                if (position.Value.x < -voxelPositionMax.x + worldBound.voxelDimensions.x / 2f)
                {
                    position.Value = new float3(-voxelPositionMax.x + worldBound.voxelDimensions.x / 2f, position.Value.y, position.Value.z);
                }

                if (position.Value.z > voxelPositionMax.z + worldBound.voxelDimensions.z / 2f)
                {
                    position.Value = new float3(position.Value.x, position.Value.y, voxelPositionMax.z + worldBound.voxelDimensions.z / 2f);
                }
                if (position.Value.z < -voxelPositionMax.z + worldBound.voxelDimensions.z / 2f)
                {
                    position.Value = new float3(position.Value.x, position.Value.y, -voxelPositionMax.z + worldBound.voxelDimensions.z / 2f);
                }

                if (position.Value.y < 0)
                {
                    position.Value = new float3(position.Value.x, 0, position.Value.z);
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return new WorldBoundJob { }.Schedule(this, inputDeps);
        }
    }
}
