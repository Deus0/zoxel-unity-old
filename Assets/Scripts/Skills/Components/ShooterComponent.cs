using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;

namespace Zoxel
{

    // basically a skill that spawns bullets and adds force to them!
    public struct Shooter : IComponentData
    {
        //public BulletData bullet;
        public float lastShotTime;  // last shot time
        public float3 shootPosition;
        public quaternion shootRotation;
        //public float shootDamage;
        public byte triggered;
        public byte isShoot;
        //public int cameraID;
        public float attackDamage;   // should this be somewhere else?
        public float attackForce;   // should this be somewhere else?
        public int bulletMetaID;

        public bool CanTrigger(float time)
        {
            return time - lastShotTime >= 1f;   // should have cooldown here
        }
    }
}