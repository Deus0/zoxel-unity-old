using Unity.Entities;
using Unity.Mathematics;

namespace Zoxel
{
    /// <summary>
    /// Should also make it follow a target character - locked
    /// Or just move to it, over time, then unlock controls
    /// </summary>
    [System.Serializable]
    public struct SpawnPoint : IComponentData
    {
        public int spawnType;
        public float3 position;
        public int monsterID;
        public int clanID;
    }

    //[RequireComponent(typeof(TranslationComponent), typeof(RotationComponent))]
    public class SpawnPointComponent : ComponentDataProxy<SpawnPoint> { }
}