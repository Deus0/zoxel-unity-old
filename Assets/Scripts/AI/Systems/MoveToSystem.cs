using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Transforms;
using Unity.Burst;

namespace Zoxel
{
    /// <summary>
    /// what to do after reaches target?
    /// </summary>
    [DisableAutoCreation]
    public class MoveToSystem : JobComponentSystem
	{
		[BurstCompile]
		struct MoveToJob : IJobForEach<Mover, BodyForce, Translation, Rotation>
        {
			public void Execute(ref Mover mover, ref BodyForce body, ref Translation position, ref Rotation rotation)
            {
                if (mover.disabled == 0)
                {
                    // Move to point
                    float distanceTo = math.distance(mover.target, position.Value);// math.max(0.5f, math.distance(moveto.target, position.Value));
                    if (distanceTo >= mover.stopDistance + 2f)  // if above 2 units away go at full speed
                    {
                        body.velocity = new float3(0, 0, mover.moveSpeed);
                    }
                    // if within range 2 of targetslow down
                    else if (distanceTo >= mover.stopDistance)
                    {
                        //body.velocity = float3.zero;//
                        body.velocity = new float3(0, 0, (distanceTo - mover.stopDistance) * mover.moveSpeed * 0.9f);   // 
                    }
                    // else distanceTo must be between 0 and stop distance, then go backwards
                    else if (distanceTo < mover.stopDistance)
                    {
                        // at 0 distance of 0, go minus 2 speed away from target
                        body.velocity = new float3(0, 0, (distanceTo - mover.stopDistance) * mover.moveSpeed); // new float3(0, 0, -(stopDistance - distanceTo) * body.movementSpeed);  // if 1, go -1 minus
                    }
                }
            }
        }

		protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return new MoveToJob { }.Schedule(this, inputDeps);
		}
	}
}