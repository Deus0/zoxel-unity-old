using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Transforms;
using Unity.Burst;

namespace Zoxel
{

    [DisableAutoCreation]
    public class CameraFacerSystem : JobComponentSystem
	{
        public CameraSystem cameraSystem;

        [BurstCompile]
		struct FaceCameraJob : IJobForEach<FaceCameraComponent, Translation, Rotation>
		{
			[ReadOnly]
			public float3 cameraPosition;
            [ReadOnly]
            public quaternion cameraRotation;
            [ReadOnly]
            public float delta;

            public void Execute(ref FaceCameraComponent face, ref Translation position, ref Rotation rotation)
            {
                quaternion targetRotation = math.mul(quaternion.EulerXYZ(new float3(0, math.PI * 2, 0)).value, (cameraRotation));
                rotation.Value = QuaternionHelpers.slerpSafe(rotation.Value, targetRotation, delta * 3);
                //rotation.Value = quaternion.LookRotation(face.position - cameraPosition, new float3(0, 1, 0));

            }
		}

		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
            if (cameraSystem.cameraObjects.Values.Count == 0)
            {
                return new JobHandle();
            }
            float3 cameraPosition = cameraSystem.GetMainCameraPosition();
            quaternion cameraRotation = cameraSystem.GetMainCamera().transform.rotation;
            FaceCameraJob job = new FaceCameraJob
            {
                cameraPosition = cameraPosition,
                cameraRotation = cameraRotation,
                delta = UnityEngine.Time.deltaTime
            };
			JobHandle handle = job.Schedule(this, inputDeps);
			return handle;
		}
	}
}