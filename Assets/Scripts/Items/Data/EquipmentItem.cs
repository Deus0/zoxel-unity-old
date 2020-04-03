using Unity.Collections;
using Unity.Entities;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using System;

namespace Zoxel
{

    // rewrite this later and call them equipment parts - for body and gear, just using layers

    // need a way to store positions of voxes here, offset from their parents - only recalculate if they are dirty (changed) or if their parents changed

    [System.Serializable]
    public struct EquipmentItem
    {
        // state data
        public float durability;
       // public byte dirty;
        public int bodyIndex; // 0 if is core
        public int slotIndex; // 0, to 4, 0 is head, 1,2 shoulder, 3 is hips etc
        
        // meta data
        [ReadOnly]
        public Item data;

        [Serializable]
        public struct SerializableEquipmentItem
        {
            public float durability;
            //public byte dirty;
            public int bodyIndex;
            public int slotIndex;
            public Item.SerializeableItem data;

            public EquipmentItem GetRealOne()
            {
                var real = new EquipmentItem();
                real.durability = durability;
                //real.dirty = dirty;
                real.bodyIndex = bodyIndex;
                real.slotIndex = slotIndex;
                real.data = data.GetRealOne();
                return real;
            }
        }

        public SerializableEquipmentItem GetSerializeableClone()
        {
            var clone = new SerializableEquipmentItem();
            clone.durability = durability;
            //clone.dirty = dirty;
            clone.bodyIndex = bodyIndex;
            clone.slotIndex = slotIndex;
            clone.data = data.GetSerializeableClone();
            return clone;
        }
    }
}