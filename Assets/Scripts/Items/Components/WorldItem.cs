using Unity.Entities;
using Unity.Mathematics;

namespace Zoxel
{

    [System.Serializable]
    public struct WorldItem : IComponentData
    {
        public int id;
        public int metaID;  // ID for item meta data
        public int quantity;
    }
}