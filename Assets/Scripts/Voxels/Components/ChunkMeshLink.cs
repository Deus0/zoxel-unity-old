using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Rendering;
using Unity.Transforms;
using System;

namespace Zoxel.Voxels
{

    public struct ChunkMeshLink : ISharedComponentData, IEquatable<ChunkMeshLink>
    {
        public Mesh mesh;
        public Material material;

        public bool Equals(ChunkMeshLink obj)
        {
            //var volume = (ChunkMesh)obj;
            //return EqualityComparer<Mesh>.Default.Equals(Camera, volume.mesh);
            return mesh == obj.mesh;
        }
        public override int GetHashCode()
        {
            return mesh.GetHashCode(); //1371622046 + EqualityComparer<Mesh>.Default.GetHashCode(mesh);
        }
    }
}