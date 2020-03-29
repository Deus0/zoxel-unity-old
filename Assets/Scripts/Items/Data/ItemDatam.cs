using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zoxel.Voxels;

namespace Zoxel
{
    // Meta Data for item

    // used for items!
    [System.Serializable]
    public struct Item
    {
        public int id;
        public float scale;
        //public string name;
        //public int gameIndex;

        public void GenerateID()
        {
            id = Bootstrap.GenerateUniqueID();
        }
    }

    // starting point for a slot to position to for a vox
    [System.Serializable]
    public enum SlotAxis
    {
        Centre,
        Top,
        Bottom,
        Left,
        Right,
        Front,
        Back
    }

    [System.Serializable]
    public struct MaleSlot
    {
        public string name;
        public SlotAxis axis;
        public int3 offset;
    }

    [System.Serializable]
    public struct FemaleSlot
    {
        public string name;
        public SlotAxis axis;
        public int3 offset;
    }

    [CreateAssetMenu(fileName = "Item", menuName = "Zoxel/Item")]
    public class ItemDatam : ScriptableObject
    {
        public Item Value;
        public TextureDatam texture;
        public VoxDatam model;
        public SoundDatam pickedUp;
        public List<MaleSlot> maleSlots;
        public List<FemaleSlot> femaleSlots;

        // loredatam - instead of basic descriptions i will associate my items with lore objects which can be expanded upon

        [ContextMenu("Generate ID")]
        public void GenerateID()
        {
            Value.GenerateID();
        }
    }
}