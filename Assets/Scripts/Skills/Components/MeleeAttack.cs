using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Transforms;

namespace Zoxel
{
    public struct MeleeAttack : IComponentData
    {
        public byte triggered;
        public float lastAttacked;
        public float attackCooldown;
        public float attackDamage;
        public byte didHit;
    }

}