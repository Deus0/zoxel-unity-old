using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

namespace Zoxel
{
    // remember to put proxies on camera object

    [System.Serializable]
    public struct CharacterLink : IComponentData
    {
        public Entity character;
        public float3 position;
        public quaternion rotation;
    }
}