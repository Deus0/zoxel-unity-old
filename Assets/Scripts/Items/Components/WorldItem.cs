using Unity.Collections;
using Unity.Entities;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;


namespace Zoxel
{

    [System.Serializable]
    public struct WorldItem : IComponentData
    {
        public int id;
        public int quantity;
        [ReadOnly]
        public Item data;
    }
}