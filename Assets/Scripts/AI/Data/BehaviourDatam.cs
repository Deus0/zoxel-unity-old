using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Zoxel
{
    [Serializable]
    public struct BehaviourData
    {
        public int id;
        // AIStateData - aggressive - follow summoned person
        public byte isAggressive;   // 1
        public float idleTime;      // 3
        public WanderData wander;   // 5 5 5 5
        public SeekData seek;       // 4 12 6 3

        public void GenerateID()
        {
            id = Bootstrap.GenerateUniqueID();
        }
    }

    [CreateAssetMenu(fileName = "Behaviour", menuName = "Zoxel/Behaviour")]
    public class BehaviourDatam : ScriptableObject
    {
        public BehaviourData Value;


        [ContextMenu("Generate ID")]
        public void GenerateID()
        {
            Value.GenerateID();
        }
    }
}