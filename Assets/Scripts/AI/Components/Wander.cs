using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;

namespace Zoxel
{
    public class WanderComponent : ComponentDataProxy<Wander> { }
    /// <summary>
    /// each body spawns inside a 
    /// </summary>
    [System.Serializable]
    public struct Wander : IComponentData
	{
        // Instanced data
        //public byte disabled;
        public byte thinking;
        public Random random;
		public int uniqueness;
        public float lastWandered;
        public float wanderCooldown;
        public float waitCooldown;
        // Core data
        public WanderData Value;

        public float3 targetAngle;
    }
    [System.Serializable]
    public struct WanderData
    {
        public float wanderCooldownMin;
        public float wanderCooldownMax;
        public float waitCooldownMin;
        public float waitCooldownMax;
    }
}
