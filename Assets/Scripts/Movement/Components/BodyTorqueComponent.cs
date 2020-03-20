using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;

namespace Zoxel
{
    public class BodyTorqueComponent : ComponentDataProxy<BodyTorque> { }

    [System.Serializable]
    public struct BodyTorque : IComponentData
    {
        // Rotation
        public float3 angle;
        public float3 velocity;
        public float3 torque;
    }
}
