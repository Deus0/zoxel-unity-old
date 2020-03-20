using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Zoxel
{
    /// <summary>
    /// used for bullets
    /// Vox Model, or normal mesh?
    /// Needs particles (ongoing, on release, on destroyed)
    /// Needs animation (like oscillating the scale, etc)
    /// </summary>
    [CreateAssetMenu(fileName = "Skill", menuName = "Zoxel/Skill")]
    public class SkillDatam : ScriptableObject
    {
        public SkillData Value = new SkillData
        {
            /*cooldown = 1,
            casttime = 0,
            statCostName = "Energy",
            statCostValue = 1*/
        };
        public TextureDatam texture;
        public SoundDatam audio;

        // for spawning turrets!
        public TurretDatam turret;
        // for spawning monsters!
        public CharacterDatam monster;
        // for spawning bullets!
        public BulletDatam bullet;
        // for spawning bullets!
        public VoxelDatam voxel;

        [ContextMenu("Generate ID")]
        public void GenerateID()
        {
            Value.GenerateID();
        }
    }
}