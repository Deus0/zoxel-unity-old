using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Zoxel
{
    [Serializable]
    public struct MeshData
    {
        public BlitableArray<float3> vertices;
        public BlitableArray<float2> uvs;
        public BlitableArray<int> triangles;

    }

    /// <summary>
    /// Used for voxels
    /// </summary>
    [CreateAssetMenu(fileName = "Mesh", menuName = "ZoxelArt/Mesh")]
    public class MeshDatam : ScriptableObject//or monobehaviour
    {
        // primary data stored here
        public MeshData Value;
        public Mesh mesh;

        /// <summary>
        /// Generates a basic cube mesh for the voxel
        /// </summary>
        public void GenerateCube()
        {

        }
    }
}