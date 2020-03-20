using System;
using Unity.Mathematics;

namespace Zoxel
{
    [Serializable]
    public struct ItemDrop
    {
        public ItemDatam item;
        public float2 quantity;

        public ItemDatam GetItem()
        {
            return item;    // later make it random based on chance
        }

        public int GetQuantity()
        {
            return (int)math.ceil(UnityEngine.Random.Range(quantity.x, quantity.y));
        }
    }

}