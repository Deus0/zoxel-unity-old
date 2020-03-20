using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Zoxel
{
    //[UpdateAfter(typeof(PositionalForceSystem))]

    /// <summary>
    /// Parents the camera position to the characters
    ///     This should run after movement systems
    /// </summary>
    [DisableAutoCreation] // why is this chopping
    public class CameraSynchSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities.WithAll<CameraSynch, Translation, FirstPersonCamera>().ForEach((Entity e, 
                ref CameraSynch cameraSynch,
                ref Translation translation,
                ref FirstPersonCamera camera) =>
            {
                if (World.EntityManager.Exists(cameraSynch.Value) && World.EntityManager.HasComponent<Translation>(cameraSynch.Value))
                {
                    Translation parentTranslation = World.EntityManager.GetComponentData<Translation>(cameraSynch.Value);
                    Rotation parentRotation = World.EntityManager.GetComponentData<Rotation>(cameraSynch.Value);
                    //float3 newPosition = parentTranslation.Value + parent.localPosition;
                    float3 newPosition = parentTranslation.Value + new float3(cameraSynch.localPosition.x, cameraSynch.localPosition.y, 0);
                    if (camera.Value.cameraAddition.z != 0)
                    {
                        newPosition += math.rotate(parentRotation.Value, new float3(0, 0, camera.Value.cameraAddition.z));
                    }
                    translation.Value = newPosition;
                }
            });
        }
    }
}