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
    public class FallSystem : JobComponentSystem
    {
        [BurstCompile]
        struct WorldBoundJob : IJobForEach<WorldBound, Translation, BodyForce>
        {
            public const float fallSpeed = 1.2f;
            public void Execute(ref WorldBound worldBound, ref Translation position, ref BodyForce forcer)
            {
                /* if (worldBound.worldID == 0)
                {
                    return;
                }
                if (worldBound.IsInsideSolids())
                {
                    //Debug.LogError("Not Falling");
                    forcer.acceleration = new float3(0, fallSpeed, 0);
                }
                else if (worldBound.IsNoSolidsUnderneath())
                 {
                     //Debug.LogError("Falling");
                     forcer.acceleration = new float3(0, -fallSpeed, 0);
                 }*/
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return new WorldBoundJob { }.Schedule(this, inputDeps);
        }
    }
}
