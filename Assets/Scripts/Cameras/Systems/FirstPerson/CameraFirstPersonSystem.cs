using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Transforms;

namespace Zoxel
{
    /// <summary>
    /// Move player inputs to controlling the body
    /// 
    /// Run after ForceSystem
    /// </summary>
    [DisableAutoCreation, UpdateBefore(typeof(CameraSynchSystem))]
    public class CameraFirstPersonSystem : JobComponentSystem
    {
        [BurstCompile]
        struct ControllerJob : IJobForEach<FirstPersonCamera, Rotation>
        {
            [ReadOnly]
            public float delta;

            public void Execute(ref FirstPersonCamera camera, ref Rotation rotation)
            {
                if (camera.enabled == 1)
                {
                    const float multiplyAccel = 6;
                    float2 cameraInput = new float2(-camera.rightStick.y, camera.rightStick.x);
                    camera.rotationAcceleration.x = cameraInput.x * camera.Value.sensitivity.y * delta * multiplyAccel;
                    camera.rotationAcceleration.y = cameraInput.y * camera.Value.sensitivity.x * delta * multiplyAccel;
                    camera.rotationVelocity += camera.rotationAcceleration;
                    camera.rotationAcceleration = float3.zero;
                    camera.rotation += camera.rotationVelocity;
                    if (camera.rotation.x < camera.Value.rotationBoundsX.x)
                    {
                        camera.rotation.x = camera.Value.rotationBoundsX.x;
                    }
                    if (camera.rotation.x > camera.Value.rotationBoundsX.y)
                    {
                        camera.rotation.x = camera.Value.rotationBoundsX.y;
                    }
                    // finally set it
                    rotation.Value = UnityEngine.Quaternion.Euler(camera.rotation);
                    if (cameraInput.x >= -0.2f && cameraInput.x < 0.2f)
                    {
                        camera.rotationVelocity.x *= 0.1f;
                    }
                    else
                    {
                        camera.rotationVelocity.x *= 0.6f;
                    }
                    if (cameraInput.y >= -0.2f && cameraInput.y < 0.2f)
                    {
                        camera.rotationVelocity.y *= 0.1f;
                    }
                    else
                    {
                        camera.rotationVelocity.y *= 0.6f;
                    }
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return new ControllerJob { delta = UnityEngine.Time.deltaTime }.Schedule(this, inputDeps);
        }
    }
}