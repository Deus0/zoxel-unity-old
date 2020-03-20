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
    public class VoxelCollisionSystem : JobComponentSystem
    {
        [BurstCompile]
        struct VoxelCollisionJob : IJobForEach<WorldBound, Translation>
        {
            public void Execute(ref WorldBound worldBound, ref Translation position)
            {
                if (worldBound.enabled == 0)
                {
                    return;
                }
                float3 newPosition = math.transform(math.inverse(worldBound.worldTransform), position.Value);
                newPosition.y = math.floor(newPosition.y);
                float heightLeft = newPosition.y;
                float heightRight = newPosition.y;
                float heightForward = newPosition.y;
                float heightBack = newPosition.y;
                if (worldBound.voxelTypeLeftBelow == 0) // worldBound.voxelTypeLeft == 0 && 
                {
                    heightLeft--;
                }
                else if (worldBound.voxelTypeLeft != 0)
                {
                    heightLeft++;
                }
                if (worldBound.voxelTypeRight == 0 && worldBound.voxelTypeRightBelow == 0)//
                {
                    heightRight--;
                }
                else if (worldBound.voxelTypeRight != 0)
                {
                    heightRight++;
                }
                if (worldBound.voxelTypeForward == 0 && worldBound.voxelTypeForwardBelow == 0) // 
                {
                    heightForward--;
                }
                else if (worldBound.voxelTypeForward != 0)
                {
                    heightForward++;
                }
                if (worldBound.voxelTypeBack == 0 && worldBound.voxelTypeBackBelow == 0) // 
                {
                    heightBack--;
                }
                else if (worldBound.voxelTypeBack != 0)
                {
                    heightBack++;
                }
                if (worldBound.voxelPosition.y <= 0) //worldBound.voxelPosition.y < worldBound.size.y)
                {
                    heightLeft++;
                    heightRight++;
                    heightForward++;
                    heightBack++;
                }
                float maxHeight = math.max(math.max(heightForward, heightBack), math.max(heightLeft, heightRight));
                maxHeight -= math.floor(worldBound.size.y); //  
                //int amountDown = (int)worldBound.size.y; //(int)math.floor((worldBound.size.y));

                // math.floor
                newPosition.y = maxHeight;

                float3 positionOffset = new float3(0, (worldBound.size.y), 0); // math.floor

                newPosition = math.transform(worldBound.worldTransform, newPosition + positionOffset);

                position.Value = newPosition;
            }
        }
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return new VoxelCollisionJob { }.Schedule(this, inputDeps);
        }
    }
}
//; // + math.floor(worldBound.size.y);
//float3 positionOffset = float3.zero;
// worldBound.size.y + worldBound.size.y / 2f + (amountDown) / 2f;
// newPosition.y = maxHeight + worldBound.size.y / 2f - (amountDown) / 2f; //  worldBound.size.y + 