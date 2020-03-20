using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;

namespace Zoxel
{
    public class MoverComponent : ComponentDataProxy<Mover> { }

    /// <summary>
    /// To Move to a point
    /// </summary>
    [System.Serializable]
    public struct Mover : IComponentData
    {
        public byte disabled;
        public float3 target;       // targetPosition
        public float stopDistance;  // how far away from target should i stop

        // seperate from this if i need to reuse
        public float moveSpeed;
        public float turnSpeed;
    }

}