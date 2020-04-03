using System;
using System.Collections;
using System.Collections.Generic;

namespace Zoxel
{
    [Serializable]
    public enum StatType
    {
        Base,
        State,
        Regen,
        Attribute,
        Level
    }

    [Serializable]
    public struct StatData
    {
        public int id;
        public void GenerateID()
        {
            id = Bootstrap.GenerateUniqueID();
        }
    }
}