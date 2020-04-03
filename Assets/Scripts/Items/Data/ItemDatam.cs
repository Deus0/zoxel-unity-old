using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zoxel.Voxels;

namespace Zoxel
{
    [CreateAssetMenu(fileName = "Item", menuName = "Zoxel/Item")]
    public class ItemDatam : ScriptableObject, ISerializationCallbackReceiver
    {
        public Item data;
        public TextureDatam texture;
        public VoxDatam model;
        public SoundDatam pickedUp;
        //public string maleSlot;
        //public List<string> femaleSlots;


        // loredatam - instead of basic descriptions i will associate my items with lore objects which can be expanded upon

        [ContextMenu("Generate ID")]
        public void GenerateID()
        {
            data.GenerateID();
        }

        // serializeable item
        
        #region SerializeableComponents

        [HideInInspector]
        public Item.SerializeableItem clone;

        public void OnBeforeSerialize()
        {
            clone = data.GetSerializeableClone();
        }

        public void OnAfterDeserialize()
        {
            data = clone.GetRealOne();
        }
        #endregion
    }
}