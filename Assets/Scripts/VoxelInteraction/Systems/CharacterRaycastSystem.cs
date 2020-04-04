using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Zoxel.Voxels;

namespace Zoxel
{
    // When switching to placing voxel or whatever
    //      Add this component onto character
    //      else remove it


    [DisableAutoCreation]
    public class CharacterRaycastSystem : ComponentSystem
    {
        public VoxelPreviewSystem voxelPreviewSystem;
        public VoxelRaycastSystem voxelRaycastSystem;
        public CameraSystem cameraSystem;

        protected override void OnUpdate()
        {
            Entities.WithAll<CharacterRaycaster>().ForEach((Entity e, ref CharacterRaycaster raycaster) =>
            {
                if (raycaster.commandID == 0)
                {
                    if (UnityEngine.Time.time - raycaster.lastCasted >= 0.1f)
                    {
                        raycaster.lastCasted = UnityEngine.Time.time;
                        if (voxelRaycastSystem != null)
                        {
                            Entity camera = raycaster.camera;
                            var zoxID = World.EntityManager.GetComponentData<ZoxID>(camera).id;
                            if (World.EntityManager.HasComponent<Controller>(e))
                            {
                                Controller controller = World.EntityManager.GetComponentData<Controller>(e);
                                WorldBound worldBound = World.EntityManager.GetComponentData<WorldBound>(e);
                                if (controller.inputType == (byte)(DeviceType.KeyboardMouse))
                                {
                                    float2 screenPosition = cameraSystem.GetScreenPosition(zoxID);
                                    raycaster.commandID = voxelRaycastSystem.QueueRaycast(screenPosition, worldBound.world, camera);
                                }
                                else
                                {
                                    float2 screenPosition = cameraSystem.GetScreenPosition(zoxID, new float2(Screen.width / 2f, Screen.height / 2f));
                                    raycaster.commandID = voxelRaycastSystem.QueueRaycast(screenPosition, worldBound.world, camera);
                                }
                            }
                            else
                            {
                                Debug.LogError("Raycasting not supported by npcs yet.");
                                //raycaster.commandID = voxelRaycastSystem.QueueRaycast(screenPosition, raycaster.cameraID);
                            }
                        }
                    }
                }
                else
                {
                    if (VoxelRaycastSystem.commandOutputPositions.ContainsKey(raycaster.commandID))
                    {
                        raycaster.voxelPosition = VoxelRaycastSystem.PullPosition(raycaster.commandID);
                        raycaster.commandID = 0;
                    }
                }
                if (raycaster.DidCast() == 1)
                {
                    voxelPreviewSystem.SetPosition(raycaster.voxelPosition - new float3(0, 0.49f, 0));
                   // Bootstrap.instance.selectedVoxel.transform.position = ;
                }
            });
        }
    }
}