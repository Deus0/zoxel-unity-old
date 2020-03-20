using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Transforms;

namespace Zoxel
{
    public struct TopDownCamera :IComponentData
    {
        public float3 movement;
    }

    /// <summary>
    /// Move player inputs to controlling the body
    /// </summary>
    [DisableAutoCreation]
    public class CameraTopDownSystem : JobComponentSystem
    {
        [BurstCompile]
        struct ControllerJob : IJobForEach<TopDownCamera, Controller>
        {
            [ReadOnly]
            public float delta;

            public void Execute(ref TopDownCamera camera, ref Controller controller)
            {
                camera.movement.x = controller.Value.leftStick.x;
                camera.movement.z = controller.Value.leftStick.y;
                camera.movement.y = controller.Value.rightStick.y;
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            ControllerJob job = new ControllerJob
            {
                delta = UnityEngine.Time.deltaTime
            };
            JobHandle handle = job.Schedule(this, inputDeps);
            return handle;
        }
    }
}