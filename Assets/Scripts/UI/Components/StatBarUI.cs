using Unity.Entities;
using Unity.Mathematics;

namespace Zoxel
{

    public struct StatBarUI : IComponentData
    {
        public Entity character;
        public float width;
        // states todo: remove states and just keep fading state (byte, 0 for nothing, 1 for fade out, 2 for fade in.
        public byte isTakingDamage;
        public byte isDead;
        public byte isDying;
        // animation states
        public float timeStateChanged;
        public byte visible;
        public byte isFading;

        // values : todo: just have percentage kept - use 0.4 (40 hp out of 100 max) - set when setting states
        public float targetPercentage;
        public float percentage;
        //public float max;
        // public float3 position;
    }
}