using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Transforms;
using Unity.Burst;
using UnityEngine;

/// <summary>
/// Flagged for rewrite - Due to using Referencing to lists of character entities
/// </summary>

namespace Zoxel
{
    [DisableAutoCreation]
    public class CameraFollowSystem : JobComponentSystem
    {
        [BurstCompile]
        struct CameraFollowJob : IJobForEach<FollowerCamera, Rotation, CharacterLink> 
        {

            public void Execute(ref FollowerCamera camera, ref Rotation rotation, ref CharacterLink characterToCamera)
            {
                float3 newPosition = characterToCamera.position + new float3(0, camera.Value.cameraAddition.y, 0);
                if (camera.Value.cameraAddition.z != 0)
                {
                    newPosition += math.rotate(characterToCamera.rotation, new float3(0, 0, camera.Value.cameraAddition.z));
                }
                if (!System.Single.IsNaN(newPosition.x) && !System.Single.IsNaN(newPosition.y) && !System.Single.IsNaN(newPosition.z))
                {
                    camera.Value.targetPosition = newPosition;
                }
                else
                {
                    camera.Value.targetPosition = characterToCamera.position + new float3(0, camera.Value.cameraAddition.y, 
                        camera.Value.cameraAddition.z);
                }
                camera.Value.targetRotation = Quaternion.Euler(camera.Value.cameraRotation);
            }
        }

        // need a list of cameras (from CameraSystem), then grab the monster IDs from all follow camera datas and add their positions here
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return new CameraFollowJob { }.Schedule(this, inputDeps);
        }
    }
}