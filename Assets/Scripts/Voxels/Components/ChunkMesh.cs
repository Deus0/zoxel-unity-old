using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Rendering;
using Unity.Transforms;
using System;

namespace Zoxel
{

    public struct ChunkMesh : ISharedComponentData, IEquatable<ChunkMesh>
    {
        public Mesh mesh;
        public Material material;
        public bool Equals(ChunkMesh obj)
        {
            //var volume = (ChunkMesh)obj;
            return mesh == obj.mesh;
            //return EqualityComparer<Mesh>.Default.Equals(Camera, volume.mesh);
        }

        public override int GetHashCode()
        {
            return mesh.GetHashCode(); //1371622046 + EqualityComparer<Mesh>.Default.GetHashCode(mesh);
        }
    }
}