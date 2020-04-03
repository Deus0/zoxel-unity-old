using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zoxel.Voxels;
using System;

namespace Zoxel
{
    [Serializable]
    public enum EquipLayer
    {
        Body,
        Gear
    }

    [Serializable]
    public struct EquipSlotEditor
    {
        [SerializeField]
        private int id;
        public SlotAxis axis;
        public EquipLayer layer;  // 0 for body, 1 for gear

        public void GenerateID()
        {
            if (id == 0)
            {
                id = Bootstrap.GenerateUniqueID();
            }
        }

        public EquipSlot GetRealOne()
        {
            if (id == 0)
            {
                Debug.LogError("Equip Slot has 0 id");
            }
            return new EquipSlot
            {
                id = id,
                axis = (byte)axis,
                layer = (byte)layer
            };
        }

    }
    // used for items!
    [Serializable]
    public struct EquipSlot
    {
        public int id;
        public byte axis;
        public byte layer;  // 0 for body, 1 for gear

        public void GenerateID()
        {
            id = Bootstrap.GenerateUniqueID();
        }
    }
}