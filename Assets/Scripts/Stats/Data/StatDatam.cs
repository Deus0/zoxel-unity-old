using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zoxel
{
    public enum StatType
    {
        Base,
        State,
        Regen,
        Attribute,
        Level
    }

    [System.Serializable]
    public struct StatData
    {
        public int id;
        public void GenerateID()
        {
            id = Bootstrap.GenerateUniqueID();
        }
    }

    [CreateAssetMenu(fileName = "Stat", menuName = "Zoxel/Stat")]
    public class StatDatam : ScriptableObject
    {
        public StatData Value;
        public int targetStatID;
        public StatType type;
        public TextureDatam texture;
        public string description;

        [ContextMenu("Generate ID")]
        public void GenerateID()
        {
            Value.GenerateID();
        }
    }
}