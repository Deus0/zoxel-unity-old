using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Zoxel
{ 
    [Serializable]
    public struct MonsterData
    {
        public int id;

        // Base this on stats rather then internals
        //public float movementSpeed;   // 2
        //public float turnSpeed;       // 1

        public void GenerateID()
        {
            id = Bootstrap.GenerateUniqueID();
        }
    }
}