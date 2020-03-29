using System;

namespace Zoxel
{
    [Serializable]
    public struct StateStaz
    {
        public int id;
        public float value;
        public float maxValue;

        public void DebugStat()
        {
            UnityEngine.Debug.LogError("value: " + value + " maxValue " + maxValue);
        }

        public bool IsMaxValue()
        {
            //return (int)(1 * value) == (int)(1 * maxValue);
            return value == maxValue;
        }
    }
}