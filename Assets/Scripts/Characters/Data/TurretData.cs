using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Zoxel
{ 

    [Serializable]
    public struct TurretData
    {
        public float goldCost;
        public float turnSpeed;
        // stats
        public float health;
        public float healthRegen;
        public float attackSpeed;
        public float attackDamage;
        public float attackForce;
        //public float attackRange;

        public SeekData seek;
    }
}