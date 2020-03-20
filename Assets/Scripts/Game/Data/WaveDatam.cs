using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Zoxel
{
    [CreateAssetMenu(fileName = "Waves", menuName = "Zoxel/Waves")]//, order = 1)]
    public class WaveDatam : ScriptableObject
    {
        public List<WaveData> Values;
    }

}