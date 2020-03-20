using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;

namespace Zoxel
{
    // Anything used by the character to aim at things!
    // Basically an AI thing now
    public struct Aimer : IComponentData
    {
        public int id;
        //public byte triggered;
        //public byte hasInitialized;
        public Random random;
        public int uniqueness;
        public float3 originalPosition;
        public float turnSpeed;
        public quaternion targetRotation;
        public float3 shootPosition;
        public float offsetZ;   // used in aim system
        //public quaternion shootRotation;
        //public float shootCooldown;
        //public float shootSpeed;
        //public float shootTime;
        //public float attackForce;
        //public BulletData bullet;
    }
}