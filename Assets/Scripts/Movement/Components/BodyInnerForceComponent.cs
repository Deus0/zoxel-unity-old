using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;

namespace Zoxel
{
    public class BodyInnerForceComponent : ComponentDataProxy<BodyInnerForce> { }


    [System.Serializable]
    public struct BodyInnerForce : IComponentData
    {
        public float maxVelocity;
        public float movementForce;
        public float movementTorque;
    }
}
