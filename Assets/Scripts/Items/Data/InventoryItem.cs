using Unity.Collections;
using Unity.Entities;
using System.Collections.Generic;
using System;

namespace Zoxel
{
    [System.Serializable]
    public struct InventoryItem
    {
        // state
        public int quantity;
        public byte dirty;
        public byte dirtyQuantity;
        // meta data
        [ReadOnly]
        public Item data;

        [Serializable]
        public struct SerializableInventoryItem
        {
            public int quantity;
            public byte dirty;
            public byte dirtyQuantity;
            public Item.SerializeableItem data;

            public InventoryItem GetRealOne()
            {
                var real = new InventoryItem();
                real.quantity = quantity;
                real.dirty = dirty;
                real.dirtyQuantity = dirtyQuantity;
                real.data = data.GetRealOne();
                return real;
            }
        }

        public SerializableInventoryItem GetSerializeableClone()
        {
            var clone = new SerializableInventoryItem();
            clone.quantity = quantity;
            clone.dirty = dirty;
            clone.dirtyQuantity = dirtyQuantity;
            clone.data = data.GetSerializeableClone();
            return clone;
        }
    }

}