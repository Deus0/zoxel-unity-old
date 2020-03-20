using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Entities;
using Unity.Mathematics;
using Zoxel.Voxels;

namespace Zoxel
{
    [Serializable]
    public struct BulletData
    {
        public int id;
        public float lifetime;
        public float speed;
        public float2 damage;
        public float betweenSpread;
        public float2 spread;
        public float scale;
        public string particlesName;    // "BulletsShot"
        public string deathParticlesName;   //  "BulletsExplode"
        public float deathParticleLife; // 4

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
    [CreateAssetMenu(fileName = "Bullet", menuName = "Zoxel/Bullet")]
    public class BulletDatam : ScriptableObject
    {
        public BulletData Value = new BulletData
        {
            scale = 0.5f,
           // mass = 1
        };
        public VoxDatam model;
        public SoundDatam hitSound;
        public SoundDatam spawnSound;

        [ContextMenu("Generate ID")]
        public void GenerateID()
        {
            Value.GenerateID();
        }
    }
}