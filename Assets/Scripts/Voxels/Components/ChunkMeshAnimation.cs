using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Rendering;
using Unity.Transforms;
using System;

namespace Zoxel.Voxels
{
    public struct ChunkMeshAnimation : IComponentData
    {
        public BlitableArray<float3> bonePositions;
        public BlitableArray<quaternion> boneRotations;

        [ReadOnly]
        public BlitableArray<byte> boneIndexes;
        public BlitableArray<ZoxelVertex> vertices;
        public byte dirty;

        public void SetBones(float3[] bonePositions_, quaternion[] boneRotations_)
        {
            if (bonePositions.Length != bonePositions_.Length) {
                bonePositions = new BlitableArray<float3>(bonePositions_.Length, Allocator.Persistent);
            }
            for (int i = 0; i < bonePositions_.Length; i++)
            {
                bonePositions[i] = bonePositions_[i];
            }
            if (boneRotations.Length != boneRotations_.Length) {
                boneRotations = new BlitableArray<quaternion>(boneRotations_.Length, Allocator.Persistent);
            }
            for (int i = 0; i < boneRotations_.Length; i++)
            {
                boneRotations[i] = boneRotations_[i];
            }
        }

        public void DisposeVertices()
        {
            if (vertices.Length > 0)
            {
                vertices.Dispose();
            }
        }
		public NativeArray<ZoxelVertex> GetTempArray()
		{
            var vertsArray = vertices.ToArray();
            var verts = new NativeArray<ZoxelVertex>(vertsArray.Length, Allocator.Temp);
            verts.CopyFrom(vertsArray);
            return verts;
		}

		public NativeArray<ZoxelVertex> GetVertexNativeArray()
		{
            //var vertsArray = vertices.ToArray();
            var vertices2 = new NativeArray<ZoxelVertex>(vertices.Length, Allocator.Persistent);
            //verts.CopyFrom(vertsArray);
            for (int i = 0; i < vertices.Length; i++) 
            {
                vertices2[i] = vertices[i];
            }
            return vertices2;
		}

        public void CopyTo(ref VertCache vertCache)
        {
            /*if (array.verts.Length != vertsArray.Length)
            {
                Debug.LogError("Initializing VertCache Array: " + vertsArray.Length);
                array.Init(vertsArray.Length);
            }*/
            //var vertsArray = vertices.ToArray();
            //array.verts.CopyFrom(vertsArray);
            for (int i = 0; i < vertices.Length; i++) 
            {
                vertCache.vertices[i] = vertices[i];
            }

        }
    }


}