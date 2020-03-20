using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Zoxel
{

    [Serializable]
    public struct WaveData
    {
        public CharacterDatam spawnedOne;
        public int spawnAmount;
        public float spawnCooldown; // how long does it take for wave to appear
        public float3 spawnPosition;
        //public List<int> spawnTypes;
    }
}