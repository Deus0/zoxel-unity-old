using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Transforms;
using Unity.Burst;

namespace Zoxel
{

    // make two groups. One OrbitStarterSystem - just updates camera transform into orbitor.
    // before:  1.3 milliseconds
    // after: 0.05 seconds
    [DisableAutoCreation, UpdateAfter(typeof(CameraSynchSystem))]  // updateAfter
    public class CameraOrbitStartSystem : ComponentSystem
    {

        protected override void OnUpdate()
        {
            // if selected or not, lerp button colour
            Entities.WithAll<CameraLink, OrbitCamera>().ForEach((Entity e, ref CameraLink cameraLink, ref OrbitCamera orbitCamera) =>
            {
                if (World.EntityManager.Exists(cameraLink.camera))
                {
                    orbitCamera.cameraPosition = World.EntityManager.GetComponentData<Translation>(cameraLink.camera).Value;
                    orbitCamera.cameraRotation = World.EntityManager.GetComponentData<Rotation>(cameraLink.camera).Value;
                }
            });
        }
    }

    [DisableAutoCreation, UpdateAfter(typeof(CameraOrbitStartSystem))]
    public class CameraOrbitSystem : JobComponentSystem
    {
        [BurstCompile]
		struct CameraOrbitJob : IJobForEach<CameraLink, OrbitCamera, Translation, Rotation>
		{
            [ReadOnly]
            public float delta;

            public void Execute(ref CameraLink cameraLink, ref OrbitCamera orbitCamera, ref Translation position, ref Rotation rotation)
            {
                float3 cameraPosition = orbitCamera.cameraPosition;
                quaternion cameraRotation = orbitCamera.cameraRotation;
                orbitCamera.SetPosition(cameraPosition, cameraRotation, ref position, delta * orbitCamera.lerpSpeed);
                orbitCamera.SetRotation(cameraPosition, cameraRotation, ref rotation, delta * orbitCamera.lerpSpeed);
            }
		}

        protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
            return new CameraOrbitJob { delta = UnityEngine.Time.deltaTime }.Schedule(this, inputDeps);
		}
	}
}