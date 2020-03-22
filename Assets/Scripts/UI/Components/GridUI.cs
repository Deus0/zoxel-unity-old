using Unity.Entities;
using Unity.Mathematics;

namespace Zoxel
{
    /// <summary>
    /// Puts all children into a grid
    /// </summary>
    public struct GridUI : IComponentData
    {
        public byte dirty;

        // grid
        public float2 gridSize; // 3 x 3
        public float2 iconSize;
        public float2 margins;
        public float2 padding;

        public float2 GetSize()
        {
            return new float2(gridSize.x * iconSize.x + (gridSize.x - 1) * padding.x + margins.x * 2f,
                gridSize.y * iconSize.y + (gridSize.y - 1) * padding.y + margins.y * 2f);

        }
        // temp
        //public int characterID;
    }
}