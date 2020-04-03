using System;

namespace Zoxel
{
    [Serializable]
    public struct RegenStaz
    {
        // meta
        public int id;
        public int targetID;
        public float value;
        public float rate;
        public float lastUpdatedTime;
    }
}