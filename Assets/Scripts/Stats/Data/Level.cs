using System;

namespace Zoxel
{

    /// <summary>
    /// You can get different levels for different things: 
    ///     Combat
    ///     Planting Flowers
    ///     Digging Dirt
    /// </summary>
    [System.Serializable]
    public struct Level
    {
        public int id;
        public int value;
        public float experienceGained;
        public float experienceRequired;
    }
}