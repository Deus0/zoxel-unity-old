using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Transforms;
using Unity.Burst;
using Zoxel;
using Unity.Rendering;
using System;

namespace Zoxel.Animations
{

    // Make a system that sets the animators state depending on the body speed

    [DisableAutoCreation]
    public class AnimatorSystem : JobComponentSystem
    {
        [BurstCompile]
        struct AnimatorJob : IJobForEach<Animator, BodyForce>
        {
            public void Execute(ref Animator animator, ref BodyForce body)
            {
                if (body.velocity.z >= -0.05f && body.velocity.z <= 0.05f
                    && body.velocity.x >= -0.05f && body.velocity.x <= 0.05f)
                {
                    if (animator.isWalking != 0)
                    {
                        animator.didUpdate = 1;
                        animator.isWalking = 0;
                    }
                }
                else
                {
                    if (animator.isWalking != 1)
                    {
                        animator.didUpdate = 1;
                        animator.isWalking = 1;
                    }
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return new AnimatorJob {  }.Schedule(this, inputDeps);
        }
    }
}