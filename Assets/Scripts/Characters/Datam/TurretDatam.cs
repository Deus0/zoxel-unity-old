using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Entities;
using Unity.Mathematics;
using Zoxel.Voxels;

namespace Zoxel
{
    /// <summary>
    /// Turrets!
    /// </summary>
    [CreateAssetMenu(fileName = "Turret", menuName = "Zoxel/Turret")]//, order = 0)]
    public class TurretDatam : ScriptableObject
    {
        public TurretData Value;
        public VoxDatam headVox;
        public VoxDatam baseVox;
        public BulletDatam bullet;

        public Mesh headMesh;
        public Mesh baseMesh;
        public Material material;

        public SoundDatam spawnSound;
    }
}