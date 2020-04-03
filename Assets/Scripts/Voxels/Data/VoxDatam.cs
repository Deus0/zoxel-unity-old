using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Mathematics;

namespace Zoxel.Voxels
{

    /// <summary>
    /// This is the data for Vox Models (magica voxel)
    /// 
    /// </summary>
    [CreateAssetMenu(fileName = "Vox", menuName = "ZoxelArt/Vox")]//, order = 4)]
    public class VoxDatam : ScriptableObject, ISerializationCallbackReceiver
    {
        [Header("Data")]
        public VoxData data;

        [ContextMenu("GenerateID")]
        public void GenerateID()
        {
            data.id = Bootstrap.GenerateUniqueID();
        }

        /// <summary>
        /// Generate a vox sphere
        /// </summary>
        [ContextMenu("Generate Sphere")]
        public void GenerateSphere()
        {

        }

        /// <summary>
        /// Generate a heightmap
        /// </summary>
        [ContextMenu("Generate Height Noise")]
        public void GenerateHeightNoise()
        {

        }

        /// <summary>
        /// Uses the voxel data, and bakes the mesh, using Zoxel Systems
        /// </summary>
        [ContextMenu("Bake Mesh")]
        public void BakeMesh()
        {

        }

        [HideInInspector]
        public VoxData.SerializeableVoxData clone;

        public void OnBeforeSerialize()
        {
            clone = data.GetSerializeableClone();
        }

        public void OnAfterDeserialize()
        {
            data = clone.GetRealOne();
        }
    }
}