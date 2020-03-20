using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

namespace Zoxel
{
    /// <summary>
    /// each body spawns inside a 
    /// </summary>
    public struct CharacterSpawner : IComponentData
    {
        public byte disabled;
    }
}