using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;

namespace Zoxel
{
    /// <summary>
    /// Moves the player's character Body around using the Controller component
    /// 
    /// Notes:
    ///     Move player inputs to controlling the body
    ///     XY controller goes to XZ body movement
    /// </summary>
    [DisableAutoCreation]
    public class PlayerInputSystem : JobComponentSystem
	{
		[BurstCompile]
		struct PlayerControllerJob : IJobForEach<BodyForce, BodyInnerForce, Controller, Targeter>
		{
			public void Execute(ref BodyForce body, ref BodyInnerForce innerBody, ref Controller controller, ref Targeter targeter)
            {
                if (controller.mappingType == 0) 
                {
                    //body.velocity = new float3(controller.Value.leftStick.x, 0, controller.Value.leftStick.y) * 1f * innerBody.movementForce;
                    if (math.abs(body.velocity.x) < innerBody.maxVelocity)
                    {
                        body.localAcceleration.x = controller.Value.leftStick.x * 0.7f * 1f * innerBody.movementForce / 5f;
                    }
                    if (math.abs(body.velocity.z) < innerBody.maxVelocity)
                    {
                        body.localAcceleration.z = controller.Value.leftStick.y * 1f * innerBody.movementForce / 5f;
                    }
                    // friction
                    body.velocity.x *= 0.78f; // slow down force
                    body.velocity.z *= 0.78f; // slow down force
                }
            }
		}

		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
            return new PlayerControllerJob { }.Schedule(this, inputDeps);
		}
    }
}