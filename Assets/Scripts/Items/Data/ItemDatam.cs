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

    [CreateAssetMenu(fileName = "Item", menuName = "Zoxel/Item")]
    public class ItemDatam : ScriptableObject
    {
        public Item Value;
        public TextureDatam texture;
        public VoxDatam model;
        public SoundDatam pickedUp;

        //public Mesh mesh;
        //public Material material;

        [ContextMenu("Generate ID")]
        public void GenerateID()
        {
            Value.GenerateID();
        }
    }
}