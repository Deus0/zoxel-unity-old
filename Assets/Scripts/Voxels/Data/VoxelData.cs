using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Zoxel
{ 

    [Serializable]
    public struct VoxelData 
    {
        public int id;
        public int meshIndex;   // 0 for cube
        ///public float2 uv;
        //public float2 textureDimensions;

        public void GenerateID()
        {
            id = Bootstrap.GenerateUniqueID();
        }
    }
}