using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zoxel.Voxels;
using Unity.Collections;
using System;

namespace Zoxel {
    
    // Meta Data for item

    // used for items!
    [Serializable]
    public struct Item
    {
        public int id;
        public int3 offset;
        public EquipSlot maleSlot;
        public BlitableArray<EquipSlot> femaleSlots;
        public BlitableArray<int3> femaleOffsets;
        public BlitableArray<byte> femaleModifiers;

        #region Serialization

        [Serializable]
        public struct SerializeableItem
        {
            public int id;
            public int3 offset;
            public EquipSlot maleSlot;
            public EquipSlot[] femaleSlots;
            public int3[] femaleOffsets;
            public byte[] femaleModifiers;

            public Item GetRealOne()
            {
                Item item = new Item();
                item.id = id;
                item.offset = offset;
                item.maleSlot = maleSlot;
                if (femaleSlots != null)
                {
                    item.femaleSlots = new BlitableArray<EquipSlot>(femaleSlots.Length, Allocator.Persistent);
                    for (int i = 0; i < femaleSlots.Length; i++)
                    {
                        item.femaleSlots[i] = femaleSlots[i];
                    }
                }
                else
                {
                    item.femaleSlots = new BlitableArray<EquipSlot>(0, Allocator.Persistent);
                }
                if (femaleOffsets != null)
                {
                    item.femaleOffsets = new BlitableArray<int3>(femaleOffsets.Length, Allocator.Persistent);
                    for (int i = 0; i < femaleOffsets.Length; i++)
                    {
                        item.femaleOffsets[i] = femaleOffsets[i];
                    }
                }
                else
                {
                    item.femaleOffsets = new BlitableArray<int3>(0, Allocator.Persistent);
                }
                if (femaleModifiers != null)
                {
                    item.femaleModifiers = new BlitableArray<byte>(femaleModifiers.Length, Allocator.Persistent);
                    for (int i = 0; i < femaleModifiers.Length; i++)
                    {
                        item.femaleModifiers[i] = femaleModifiers[i];
                    }
                }
                else
                {
                    item.femaleModifiers = new BlitableArray<byte>(0, Allocator.Persistent);
                }
                return item;
            }
        }

        public SerializeableItem GetSerializeableClone()
        {
            var clone = new SerializeableItem();
            clone.id = id;
            clone.offset = offset;
            clone.maleSlot = maleSlot;
            var femaleSlotsArray = femaleSlots.ToArray();
            clone.femaleSlots = new EquipSlot[femaleSlots.Length];
            for (int i = 0; i < femaleSlotsArray.Length; i++)
            {
                clone.femaleSlots[i] = femaleSlotsArray[i];
            }
            var femaleOffsetsArray = femaleOffsets.ToArray();
            clone.femaleOffsets = new int3[femaleOffsetsArray.Length];
            for (int i = 0; i < femaleOffsetsArray.Length; i++)
            {
                clone.femaleOffsets[i] = femaleOffsetsArray[i];
            }
            var femaleModifiersArray = femaleModifiers.ToArray();
            clone.femaleModifiers = new byte[femaleModifiersArray.Length];
            for (int i = 0; i < femaleModifiersArray.Length; i++)
            {
                clone.femaleModifiers[i] = femaleModifiersArray[i];
            }
            return clone;
        }  
        #endregion

        // public float scale;  // scale is for vox model, dont need item scale as well
        //public string name;
        //public int gameIndex;

        public void GenerateID()
        {
            id = Bootstrap.GenerateUniqueID();
        }

        public void SetSlots(SlotDatam maleSlot_, List<SlotDatam> femaleSlots_, List<int3> femaleOffsets_, List<VoxOperation> femaleModifiers_)
        {
            if (maleSlot_ != null)
            {
                maleSlot = maleSlot_.data.GetRealOne();
                femaleSlots = new BlitableArray<EquipSlot>(femaleSlots_.Count, Allocator.Persistent);
                for (int i = 0; i < femaleSlots_.Count; i++)
                {
                    femaleSlots[i] = femaleSlots_[i].data.GetRealOne();
                }
                femaleOffsets = new BlitableArray<int3>(femaleOffsets_.Count, Allocator.Persistent);
                for (int i = 0; i < femaleOffsets_.Count; i++)
                {
                    femaleOffsets[i] = femaleOffsets_[i];
                }
                femaleModifiers = new BlitableArray<byte>(femaleModifiers_.Count, Allocator.Persistent);
                for (int i = 0; i < femaleModifiers_.Count; i++)
                {
                    femaleModifiers[i] = ((byte)femaleModifiers_[i]);
                }
            }
        }
    }
}