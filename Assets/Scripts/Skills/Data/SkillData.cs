using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Zoxel
{
    [Serializable]
    public struct SkillData
    {
        public int id;  // skill ids
        public int attackType;  // 1 for melee..?
        //public byte isRaycast;
        // melee data
        public float attackDamage;
        public float attackSpeed;
        public float attackForce;

        public byte isSpawnHostile;
        //public float cooldown;
        //public float casttime;
        //public string statCostName;
        //public float statCostValue;
        //public float castedTime;

        public void GenerateID()
        {
            id = Bootstrap.GenerateUniqueID();
        }
    }
}