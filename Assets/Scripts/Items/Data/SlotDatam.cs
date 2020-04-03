using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zoxel.Voxels;

namespace Zoxel
{

    [CreateAssetMenu(fileName = "Slot", menuName = "Zoxel/Slot")]
    public class SlotDatam : ScriptableObject
    {
        public EquipSlotEditor data;
        

        [ContextMenu("Generate ID")]
        public void GenerateID()
        {
            data.GenerateID();
        }
    }
}