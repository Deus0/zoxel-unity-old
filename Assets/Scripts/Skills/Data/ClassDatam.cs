using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Zoxel
{
    [Serializable]
    public struct ClassData
    {
        public int id;

        public void GenerateID()
        {
            id = Bootstrap.GenerateUniqueID();
        }
    }
    /// <summary>
    /// used for bullets
    /// Vox Model, or normal mesh?
    /// Needs particles (ongoing, on release, on destroyed)
    /// Needs animation (like oscillating the scale, etc)
    /// </summary>
    [CreateAssetMenu(fileName = "Class", menuName = "Zoxel/Class")]
    public class ClassDatam : ScriptableObject
    {
        public ClassData Value;
        public List<SkillDatam> startingSkills;
        public string description;

        [ContextMenu("Generate ID")]
        public void GenerateID()
        {
            Value.GenerateID();
        }
    }
}