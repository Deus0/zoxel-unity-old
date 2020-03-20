using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Transforms;
using Unity.Burst;

namespace Zoxel
{
    // Remove thinking from this
    // Add State changes to idle (including idle system which involves looking at nearby units) to AIStateSystem
    // 

    /// How can I link the body up to pathing
    /// Have a byte array for masking - using chunks
    /// Link the body to the world that its in - world being the ChunkGroup - but for terrain
    /// Input into the job - the world array?
    /// 
    /// Another system before body system, that processes pathing - only has to do it when chunks update
    ///		- creates a bitmask of the chunks that can walk on (just 1 voxel units above for ground units) - ChunkPathing
    ///		- These component datas will be used by the bodys
    ///		- body needs bound values as well - can check if the bounds intersects with any of the voxels - a simple AAB check
    ///		- BodyAI (used for wandering) use floodfill to find out if it can get to new target - with maximum checks
    ///		- Body uses this to keep height for now
    ///		
    /// Finally spawn 2000 agents to check how well they wander
    ///  - make a small youtube video
    ///  - add text between cuts
    ///  - 'Near Instanteous ECS Rendering of voxels'
    ///  - 'ECS AI and movement, up to 5000 agents'
    ///  
    /// Maybe, changing state will add/remove components?
    ///     Flee, FleeSystem
    ///     Attack, AttackSystem
    ///     Wander, WanderSystem
    ///     Idle, thats all folks
    ///     Patrol, PatrolSystem



    /// <summary>
    /// Wander System
    ///     Make use moveToSystem and just set new points around character every x seconds
    /// </summary>
    [DisableAutoCreation]
    public class WanderSystem : JobComponentSystem
	{

		[BurstCompile]
		struct WanderJob : IJobForEach<AIState, Wander, BodyForce, BodyTorque, BodyInnerForce, Rotation>
        {
            [ReadOnly]
			public float time;
			[ReadOnly]
			public float deltaTime;

            public void Execute(ref AIState brain, ref Wander wander, ref BodyForce body, ref BodyTorque torque, ref BodyInnerForce innerBody, ref Rotation rotation)
            {
				if (brain.state == 1)
				{
                    if (wander.thinking == 0)
                    {
                        body.velocity = new float3(0, 0, innerBody.movementForce);
                        wander.targetAngle = (new float3(0, wander.random.NextFloat(0, 360), 0));
                        if (time - wander.lastWandered >= wander.wanderCooldown)
                        {
                            wander.lastWandered = time;
                            if (wander.Value.waitCooldownMin != 0 && wander.Value.waitCooldownMax != 0)
                            {
                                wander.thinking = 1;
                                wander.waitCooldown = wander.random.NextFloat(wander.Value.waitCooldownMin, wander.Value.waitCooldownMax);
                            }
                        }
                    }
                    else
                    {
                        body.velocity = new float3();
                        if (time - wander.lastWandered >= wander.waitCooldown)
                        {
                            wander.lastWandered = time;
                            wander.wanderCooldown = wander.random.NextFloat(wander.Value.wanderCooldownMin, wander.Value.wanderCooldownMax);
                            wander.thinking = 0;
                        }
                    }
                    quaternion newAngle = QuaternionHelpers.slerp(
                       rotation.Value,
                       quaternion.Euler(wander.targetAngle), 
                       deltaTime * innerBody.movementTorque);
                    Quaternion newAngle2 = new Quaternion(newAngle.value.x, newAngle.value.y, newAngle.value.z, newAngle.value.w);
                    rotation.Value = newAngle2;
                }
            }
		}

		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
            return new WanderJob
			{
				time = UnityEngine.Time.time,
				deltaTime = UnityEngine.Time.deltaTime
			}.Schedule(this, inputDeps);
		}

    }
}