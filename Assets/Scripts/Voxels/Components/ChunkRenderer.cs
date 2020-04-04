using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using System.Collections.Generic;
using Unity.Rendering;
using UnityEngine.Rendering;

// IComponentData//  ISharedComponentData

namespace Zoxel.Voxels
{
	public struct BuildPointer
	{
		public int vertIndex;
		public int triangleIndex;
	}
		
	public struct ChunkRenderer : IComponentData
	{
		public Entity chunk;
		[ReadOnly]
		public ChunkData Value;
		// 0 for diffuse, 1 for water, 2 for glass, etc
		public byte materialID;
		// if has weights, does a weight baker system pass after mesh baking
		public byte hasWeights;
		// animation
		public float timePassed;

		// mesh build data
		public BuildPointer buildPointer;
		public BlitableArray<ZoxelVertex> vertices;
		public BlitableArray<int> triangles;

		// Build Data
		// UVs per voxel - 0 for cube - 1 for the 0th index of uvs - uvMaps[i - 1]
		public BlitableArray<int> modelIndexes;
		public BlitableArray<float3> voxelColors;
		public BlitableArray<VoxelUVMap> uvMaps;

		public void Init(int3 voxelDimensions)
		{
			//Debug.LogError("Initiating ChunkRenderer: " + voxelDimensions);
			int xyzSize = (int)(voxelDimensions.x * voxelDimensions.y * voxelDimensions.z);
			int maxCacheVerts = xyzSize * 4;
			int maxCacheTriangles = maxCacheVerts / 2;
			vertices = new BlitableArray<ZoxelVertex>(maxCacheVerts, Unity.Collections.Allocator.Persistent);
			triangles = new BlitableArray<int>(maxCacheTriangles, Unity.Collections.Allocator.Persistent);
		}
		
		public void Dispose()
		{
			if (modelIndexes.Length > 0)
			{
				modelIndexes.Dispose();
			}
			if (voxelColors.Length > 0)
			{
				voxelColors.Dispose();
			}
			if (uvMaps.Length > 0)
			{
				uvMaps.Dispose();
			}
			if (vertices.Length > 0)
			{
				vertices.Dispose();
			}
			if (triangles.Length > 0)
			{
				triangles.Dispose();
			}
			//Value.Dispose();
		}

		#region ForMesh
		public void SetMeshData(Mesh mesh, bool isModel)
		{
			if (isModel)
			{
				mesh.MarkDynamic();
			}
			var layout = new[]
			{
				new VertexAttributeDescriptor(UnityEngine.Rendering.VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
				new VertexAttributeDescriptor(UnityEngine.Rendering.VertexAttribute.Color, VertexAttributeFormat.Float32, 3),
				new VertexAttributeDescriptor(UnityEngine.Rendering.VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2),
			};
			mesh.SetVertexBufferParams(buildPointer.vertIndex, layout);
        	var verts = new NativeArray<ZoxelVertex>(buildPointer.vertIndex, Allocator.Temp);
			for (int i = 0; i < verts.Length; i++)
			{
				verts[i] = vertices[i];
			}
			// calculate bounds from verts
			if (isModel)
			{
				verts = ChunkRenderer.CentreMesh(verts);
				verts = ChunkRenderer.RotateMesh(verts);
			}
        	mesh.SetVertexBufferData(verts, 0, 0, buildPointer.vertIndex);
      		mesh.SetIndexBufferParams(buildPointer.triangleIndex, IndexFormat.UInt32);
        	var triangles2 = new NativeArray<int>(buildPointer.triangleIndex, Allocator.Temp);
			for (int i = 0; i < triangles2.Length; i++)
			{
				triangles2[i] = triangles[i];
			}
			mesh.SetIndexBufferData(triangles2, 0, 0, buildPointer.triangleIndex);//, MeshUpdateFlags.DontValidateIndices);  
			mesh.SetSubMesh(0, new SubMeshDescriptor() {
				baseVertex = 0,
				bounds = default,
				indexStart = 0,
				indexCount = buildPointer.triangleIndex,
				firstVertex = 0,
				topology = MeshTopology.Triangles,
				vertexCount = buildPointer.vertIndex
			});
			mesh.UploadMeshData(false);
		}

		public NativeArray<ZoxelVertex> GetVertexArray(Mesh mesh)
		{
			var vertos = mesh.vertices;
			List<Vector2> uvs2 = new List<Vector2>();
			mesh.GetUVs(0, uvs2);
			var uvs = uvs2.ToArray();
			var colors = mesh.colors;
        	var verts = new NativeArray<ZoxelVertex>(vertos.Length, Allocator.Persistent);
			//Debug.LogError("Getting verts from mesh: " + vertos.Length);
			for (int i = 0; i < verts.Length; i++)
			{
				verts[i] = new ZoxelVertex {
					position = vertos[i],
					uv = uvs[i],
					color = new float3(colors[i].r, colors[i].g, colors[i].b)
				};
			}
			return verts;
		}

        public static NativeArray<ZoxelVertex> CentreMesh(NativeArray<ZoxelVertex> vertices, Mesh mesh)
        {
            mesh.RecalculateBounds();
			float3 min = mesh.bounds.min;
			float3 extents = mesh.bounds.extents;
            for (int i = 0; i < vertices.Length; i++)
            {
				var verto = vertices[i];
				var position = verto.position;
                position -= min;
                position -= extents;
				verto.position = position;
				vertices[i] = verto;
            }
			return vertices;
        }
        public static NativeArray<ZoxelVertex> CentreMesh(NativeArray<ZoxelVertex> vertices)
        {
            //mesh.RecalculateBounds();
			float3 min = new float3(6666,6666,6666);
			float3 max = new float3(-6666,-6666,-6666);
			for (int i = 0; i < vertices.Length; i++)
			{
				if (vertices[i].position.x < min.x)
				{
					min.x = vertices[i].position.x;
				}
				if (vertices[i].position.y < min.y)
				{
					min.y = vertices[i].position.y;
				}
				if (vertices[i].position.z < min.z)
				{
					min.z = vertices[i].position.z;
				}
				if (vertices[i].position.x > max.x)
				{
					max.x = vertices[i].position.x;
				}
				if (vertices[i].position.y > max.y)
				{
					max.y = vertices[i].position.y;
				}
				if (vertices[i].position.z > max.z)
				{
					max.z = vertices[i].position.z;
				}
			}
			//float3 min = mesh.bounds.min;
			float3 extents = (max - min) / 2f; //mesh.bounds.extents;
            for (int i = 0; i < vertices.Length; i++)
            {
				var verto = vertices[i];
				var position = verto.position;
                position -= min;
                position -= extents;
				verto.position = position;
				vertices[i] = verto;
            }
			return vertices;
        }


        public static NativeArray<ZoxelVertex> RotateMesh(NativeArray<ZoxelVertex> vertices)
        {
			quaternion rot = Quaternion.Euler(180, 0, 180);
            for (int i = 0; i < vertices.Length; i++)
            {
				var verto = vertices[i];
				var position = verto.position;
                position = math.rotate(rot, position);
				verto.position = position;
				vertices[i] = verto;
            }
			return vertices;
        }

		public NativeArray<int> GetTrianglesNativeArray(Mesh mesh)
		{
			var tris = mesh.triangles;
        	var tris2 = new NativeArray<int>(tris.Length, Allocator.Persistent);
			//Debug.LogError("Getting verts from mesh: " + vertos.Length);
			for (int i = 0; i < tris.Length; i++)
			{
				tris2[i] = tris[i];
			}
			return tris2;
		}

		public NativeArray<ZoxelVertex> GetVertexArray()
		{
        	var verts = new NativeArray<ZoxelVertex>(buildPointer.vertIndex, Allocator.Persistent);
			for (int i = 0; i < verts.Length; i++)
			{
				verts[i] = vertices[i];
			}
			return verts;
		}

		
		public int[] GetTriangles()
		{
            var exportedTriangles = new int[buildPointer.triangleIndex];
			//var triangles2 = triangles.ToArray();
			for (int i = 0; i < exportedTriangles.Length; i++)
			{
                exportedTriangles[i] = triangles[i];
			}
			return exportedTriangles;
		}

		public Vector3[] GetVertices()
		{
			var exportedVertices = new Vector3[buildPointer.vertIndex];
			//var vertices = vertices.ToArray();
			for (int i = 0; i < exportedVertices.Length; i++)
			{
                exportedVertices[i] = vertices[i].position;
			}
			return exportedVertices;
		}
		
		public Vector2[] GetUVs()
        {
            var exportedUVs = new Vector2[buildPointer.vertIndex];
			//var uvs2 = uvs.ToArray();
			for (int i = 0; i < exportedUVs.Length; i++)
			{
                exportedUVs[i] = vertices[i].uv;
			}
			return exportedUVs;
		}
		public Color[] GetColors()
		{
			var exportedColors = new Color[buildPointer.vertIndex];
			//var colors2 = colors.ToArray();
			for (int i = 0; i < exportedColors.Length; i++)
			{
				float3 color = vertices[i].color;
                exportedColors[i] = new Color(color.x, color.y, color.z);
			}
			return exportedColors;
		}

		#endregion


		public void SetMetaData(Dictionary<int, VoxelDatam> meta, List<int> voxelIDs)
		{
			// meta data for building models
			modelIndexes = new BlitableArray<int>(meta.Count, Unity.Collections.Allocator.Persistent);
			int a = 0;
			foreach (VoxelDatam voxel in meta.Values)
			{
				modelIndexes[a] = voxel.Value.meshIndex;
				a++;
			}
			uvMaps = new BlitableArray<VoxelUVMap>(voxelIDs.Count, Allocator.Persistent);
			for (int i = 0; i < voxelIDs.Count; i++)
			{
				int metaID = voxelIDs[i];
				VoxelDatam voxels = meta[metaID];
				uvMaps[i] = voxels.uvMap;
			}
		}

	}

}