using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Rendering;
using Unity.Transforms;
using System;

namespace Zoxel.Voxels
{
    public struct ChunkMesh : IComponentData
    {
        public byte verticesDirty;  // reupload to mesh in chunkmeshlink
        public byte trianglesDirty;
        public byte isPushMesh;
		// animation
		public float timePassed;    // animation updates
        public BuildPointer buildPointer;
        public BlitableArray<ZoxelVertex> vertices;
        public BlitableArray<int> triangles;

        public struct BuildPointer
        {
            public int vertIndex;
            public int triangleIndex;
        }

		public void Init(int3 voxelDimensions)
		{
            Dispose();
			int xyzSize = (int)(voxelDimensions.x * voxelDimensions.y * voxelDimensions.z);
			int maxCacheVerts = xyzSize * 4;
			int maxCacheTriangles = maxCacheVerts / 2;
			vertices = new BlitableArray<ZoxelVertex>(maxCacheVerts, Unity.Collections.Allocator.Persistent);
			triangles = new BlitableArray<int>(maxCacheTriangles, Unity.Collections.Allocator.Persistent);
		}

        
		public void Dispose()
		{
			if (vertices.Length > 0)
			{
				vertices.Dispose();
			}
			if (triangles.Length > 0)
			{
				triangles.Dispose();
			}
        }

		public NativeArray<ZoxelVertex> GetVertexNativeArray()
		{
            var vertsArray = vertices.ToArray();
            var verts = new NativeArray<ZoxelVertex>(vertsArray.Length, Allocator.Temp);
            verts.CopyFrom(vertsArray);
            return verts;
		}
		public NativeArray<int> GetTrianglesNativeArray()
		{
            var trisArray = triangles.ToArray();
            var tris = new NativeArray<int>(trisArray.Length, Allocator.Temp);
            tris.CopyFrom(trisArray);
            return tris;
		}
    }
}