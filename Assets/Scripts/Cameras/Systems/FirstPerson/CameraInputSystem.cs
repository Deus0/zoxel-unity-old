using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Transforms;

namespace Zoxel
{
    // sets camera input to characters one
    // sets characters angle to camera one

    // Move all input data from controller in character to camera
    [DisableAutoCreation, UpdateBefore(typeof(CameraFirstPersonSystem))]
    public class CameraInputSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities.WithAll<CameraLink, Controller, Rotation>().ForEach((Entity e, ref CameraLink cameraLink, ref Controller controller, ref Rotation rotation) =>
            {
                if (controller.deviceID != 0)
                {
                    if (World.EntityManager.Exists(cameraLink.camera))//CameraSystem.cameras.ContainsKey(cameraLink.cameraID))
                    {
                        //Entity cameraEntity = CameraSystem.cameras[cameraLink.cameraID];
                        if (World.EntityManager.HasComponent<FirstPersonCamera>(cameraLink.camera))
                        {
                            FirstPersonCamera camera = World.EntityManager.GetComponentData<FirstPersonCamera>(cameraLink.camera);
                            if (camera.enabled == 1)
                            {
                                camera.rightStick = controller.Value.rightStick;
                                World.EntityManager.SetComponentData(cameraLink.camera, camera);
                                Rotation cameraRotation = World.EntityManager.GetComponentData<Rotation>(cameraLink.camera);
                                float3 currentCameraEuler = new UnityEngine.Quaternion(cameraRotation.Value.value.x, cameraRotation.Value.value.y, cameraRotation.Value.value.z, cameraRotation.Value.value.w).eulerAngles;
                                float3 currentAngle = new UnityEngine.Quaternion(rotation.Value.value.x, rotation.Value.value.y, rotation.Value.value.z, rotation.Value.value.w).eulerAngles;
                                currentAngle.y = currentCameraEuler.y;
                                rotation.Value = UnityEngine.Quaternion.Euler(currentAngle.x, currentAngle.y, currentAngle.z);
                            }
                        }
                    }
                }
            });
        }
    }

}