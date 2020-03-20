
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Transforms;
using Unity.Burst;

namespace Zoxel
{
    [DisableAutoCreation, UpdateBefore(typeof(MoveToSystem))]
    public class FollowTargetSystem : JobComponentSystem
    {

        [BurstCompile]
        struct FollowTargetJob : IJobForEach<ZoxID, Mover, Targeter, Body, Translation>
        {
            public void Execute(ref ZoxID zoxID, ref Mover mover, ref Targeter targeter, ref Body body, ref Translation position)
            {
                if (targeter.hasTarget == 1)
                {
                    mover.target = targeter.nearbyCharacter.position;
                    //if (body.grounded == 1)
                    {
                        mover.target = new float3(mover.target.x, position.Value.y, mover.target.z);
                    }
                    if (targeter.nearbyCharacter.clan == zoxID.clanID)
                    {
                        mover.stopDistance = 3 * (body.size.x + body.size.z); // + half radius!
                    }
                    else
                    {
                        mover.stopDistance = targeter.Value.attackRange - (body.size.x + body.size.z) / 2f; // + half radius!}
                    }
                }
                else
                {
                    mover.stopDistance = 0.5f + (body.size.x + body.size.z) / 2f;
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return new FollowTargetJob { }.Schedule(this, inputDeps);
        }
    }
}