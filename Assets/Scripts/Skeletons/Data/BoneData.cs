using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Unity.Mathematics;

namespace Zoxel
{

    // a list of animation and data
    [Serializable]
    public struct BoneData
    {
        public string name;
        public string parentName;
        public int id;
        public int parentID;
        public float influence; // on the weights!
        public float3 position;
        public quaternion rotation;
    }
}