using Unity.Entities;
using Unity.Mathematics;

namespace Zoxel
{
    public struct PanelUI : IComponentData
    {
        public byte id;
        public int characterID;
        public byte updated;
        public byte navigationDirty;
        public float2 size;
        public float orbitDepth;
        public float2 positionOffset;
        public byte anchor;
    }
}