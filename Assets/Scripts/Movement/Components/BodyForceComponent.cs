using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;

namespace Zoxel
{
    public class BodyForceComponent : ComponentDataProxy<BodyForce> { }

    [System.Serializable]
    public struct BodyForce : IComponentData
    {
        // Movement
        public float3 acceleration; // local velocity
        public float3 worldVelocity;

        public float3 localAcceleration;
        public float3 velocity; // local velocity
    }
}
