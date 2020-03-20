using Unity.Entities;

namespace Zoxel
{
    public class AIStateComponent : ComponentDataProxy<AIState> { }

    /// <summary>
    /// A state for the AI
    /// </summary>
    [System.Serializable]
    public struct AIState : IComponentData
    {
        public byte state;
        public float lastIdled;
        public float idleTime;
        public byte isAggressive;
    }
}