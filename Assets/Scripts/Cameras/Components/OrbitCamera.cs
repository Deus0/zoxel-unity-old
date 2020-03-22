using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Zoxel
{

    public struct OrbitCamera : IComponentData
    {
        public float3 position;
        public float3 orbitPosition;
        public float lerpSpeed;
        float sinPosition;
        public float3 cameraPosition;
        public quaternion cameraRotation;
        //float3 oldNewPosition;

        public void SetPosition(float3 cameraPosition, quaternion cameraRotation, ref Translation translation)
        {
            translation.Value = cameraPosition + math.rotate(cameraRotation, position);
        }
        public void SetRotation(float3 cameraPosition, quaternion cameraRotation, ref Rotation rotation)
        {
            quaternion targetRotation = math.mul(quaternion.EulerXYZ(new float3(0, math.PI * 2, 0)).value, (cameraRotation));
            rotation.Value = targetRotation;
        }
        public void SetPosition(float3 cameraPosition, quaternion cameraRotation, ref Translation translation, float delta)
        {
            if (delta > 1)
            {
                delta = 1;
            }
            sinPosition += delta * 0.05f;
            translation.Value = math.lerp(translation.Value,
                GetTranslatedPosition(cameraPosition, cameraRotation),
                delta);
        }

        public float3 GetTranslatedPosition(float3 cameraPosition, quaternion cameraRotation)
        {
            return cameraPosition + math.rotate(cameraRotation, position + new float3(0, math.sin(sinPosition) * 0.004f, 0));
        }

        public void SetRotation(float3 cameraPosition, quaternion cameraRotation, ref Rotation rotation, float delta)
        {
            quaternion targetRotation = math.mul(quaternion.EulerXYZ(new float3(0, math.PI * 2, 0)).value, (cameraRotation));
            if (delta > 1)
            {
                delta = 1;
            }
            rotation.Value = QuaternionHelpers.slerpSafe(rotation.Value, targetRotation, delta);
        }
    }

}
