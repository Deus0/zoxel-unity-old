using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Transforms;
using Unity.Burst;

using Unity.Rendering;

namespace Zoxel
{
    // if attacking, must trigger skills if target is within range
    // must cooperate with allies


    [DisableAutoCreation]
    public class BrainSystem : JobComponentSystem
    {
        [BurstCompile]
        struct MoveToJob : IJobForEach<Brain, Targeter>
        {

            /// <summary>
            /// Builds a basic mesh on the chunk!
            /// </summary>
            /// <param name="chunk"></param>
            public void Execute(ref Brain brain, ref Targeter moveto)
            {

            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            MoveToJob job = new MoveToJob { };
            JobHandle handle = job.Schedule(this, inputDeps);
            return handle;
        }
    }
}