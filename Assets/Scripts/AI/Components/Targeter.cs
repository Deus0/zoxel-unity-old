using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;

namespace Zoxel
{
    public class TargeterComponent : ComponentDataProxy<Targeter> { }

    /// <summary>
    /// Keeps data about targeting other characters
    ///     TargetSystem uses it to allocate targets
    /// </summary>
    [System.Serializable]
    public struct Targeter : IComponentData
    {
        // meta
        [ReadOnly]
        public SeekData Value;
        // Instanced Data
        public NearbyCharacter nearbyCharacter;
        //public float3 targetPosition;
        //public float distance;
        public byte hasTarget;
        //public Entity target;
        //public int targetID;
        //public int targetClanID;
        //public float targetDistance;
        public float lastSeeked;
        // other stuff?
        public quaternion currentAngle;
        public quaternion targetAngle;
    }
}