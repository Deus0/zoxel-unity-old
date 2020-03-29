using System;

namespace Zoxel
{

    /// <summary>
    /// Attributes can increase any other type of stat, by multiplying and adding the value
    ///     Skills - passive buffs - can increase the multipliers
    /// </summary>
    [Serializable]
    public struct AttributeStaz
    {
        // meta
        public int id;              // metaID - link to other stats
        public int targetID;        // target another stat to increase it
        public float value;         // the increase of the other value
        public float multiplier;      // the multiplier that it increases it by

        public float previousAdded; // previous stat value of effected stat
    }
}