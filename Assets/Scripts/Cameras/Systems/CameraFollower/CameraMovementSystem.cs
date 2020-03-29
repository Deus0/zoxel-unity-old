using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Transforms;

/// <summary>
/// Flagged for rewrite - Due to using Referencing to lists of character entities
/// </summary>

namespace Zoxel
{

    /// <summary>
    /// Check if bullet hits a character body
    /// This causes lag!
    /// </summary>
    [DisableAutoCreation]
    public class CameraMovementSystem : JobComponentSystem
    {
        public static float panSpeed = 20;

        [BurstCompile]
        struct CameraJob : IJobForEach<FollowerCamera, Translation, Rotation>
        {
            [ReadOnly]
            public float delta;

            public void Execute(ref FollowerCamera camera, ref Translation position, ref Rotation rotation)
            {
                /*if (camera.isMovement == 1)
                {
                    camera.targetPosition.x += camera.movement.x * delta * 4f;
                    camera.targetPosition.z += camera.movement.z * delta * 4f;
                    camera.targetPosition.y -= camera.movement.y * delta * 3f;
                }*/
                float3 newPosition = camera.Value.targetPosition;
                newPosition.x = math.max(newPosition.x, -999);
                newPosition.x = math.min(newPosition.x, 999);
                newPosition.y = math.max(newPosition.y, -999);
                newPosition.y = math.min(newPosition.y, 999);
                newPosition.z = math.max(newPosition.z, -999);
                newPosition.z = math.min(newPosition.z, 999);
                 position.Value = math.lerp(position.Value, newPosition, delta * camera.Value.lerpSpeed.x);
                //position.Value = newPosition;
                rotation.Value = QuaternionHelpers.slerpSafe(rotation.Value.value, camera.Value.targetRotation.value, delta * camera.Value.lerpSpeed.y);//TurretAimSystem.le math.lerp(rotation.Value.value, slimeCamera.targetRotation.value, deltaTime * slimeCamera.lerpRotationSpeed);
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            CameraJob job = new CameraJob { delta = UnityEngine.Time.deltaTime };
            JobHandle handle = job.Schedule(this, inputDeps);
            return handle;
        }
    }
}
