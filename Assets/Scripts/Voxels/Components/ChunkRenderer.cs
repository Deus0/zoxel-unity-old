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

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
	public struct ZoxelVertex
	{
		public float3 position;
		public float3 color;
		public float2 uv;
	}
		
	public struct ChunkRenderer : IComponentData
	{
		[ReadOnly]
		public ChunkData Value;
		// 0 for diffuse, 1 for water, 2 for glass, etc
		public byte materialID;
		// if has weights, does a weight baker system pass after mesh baking
		public byte hasWeights;

		// Build Data
		// UVs per voxel - 0 for cube - 1 for the 0th index of uvs - uvMaps[i - 1]
		public BlitableArray<int> modelIndexes;
		public BlitableArray<float3> voxelColors;
		public BlitableArray<VoxelUVMap> uvMaps;

		// Culling Sides Data
		public BlitableArray<byte> sidesUp;
		public BlitableArray<byte> sidesDown;
		public BlitableArray<byte> sidesLeft;
		public BlitableArray<byte> sidesRight;
		public BlitableArray<byte> sidesBack;
		public BlitableArray<byte> sidesForward;

		// mesh build data
		public BuildPointer buildPointer;
		public BlitableArray<ZoxelVertex> vertices;
		/*public BlitableArray<float3> vertices;
		public BlitableArray<float2> uvs;
		public BlitableArray<float3> colors;*/
		public BlitableArray<int> triangles;

		public BlitableArray<int> boneWeightsIndexes0;
		public BlitableArray<float> boneWeights0;
		public BlitableArray<float3> bonePositions;
		public BlitableArray<quaternion> boneRotations;
		public BlitableArray<float> boneInfluences;

		#region ForMesh
		public void SetMeshData(Mesh mesh)
		{
			var layout = new[]
			{
				new VertexAttributeDescriptor(UnityEngine.Rendering.VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
				new VertexAttributeDescriptor(UnityEngine.Rendering.VertexAttribute.Color, VertexAttributeFormat.Float32, 3),
				new VertexAttributeDescriptor(UnityEngine.Rendering.VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2),
			};
			mesh.SetVertexBufferParams(buildPointer.vertIndex, layout);
        	var verts = new NativeArray<ZoxelVertex>(buildPointer.vertIndex, Allocator.Temp);
			//var vertArray = vertices.ToArray();
			for (int i = 0; i < verts.Length; i++)
			{
				verts[i] = vertices[i];
			}
        	mesh.SetVertexBufferData(verts, 0, 0, buildPointer.vertIndex);
		
			/*mesh.vertices = GetVertices();
			mesh.uv = GetUVs();
			mesh.colors = GetColors();*/

      		mesh.SetIndexBufferParams(buildPointer.triangleIndex, IndexFormat.UInt32);
        	var triangles2 = new NativeArray<int>(buildPointer.triangleIndex, Allocator.Temp);
			for (int i = 0; i < triangles2.Length; i++)
			{
				triangles2[i] = triangles[i];
			}
			//mesh.SetTriangles(triangles2, 0);
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
			//mesh.MarkDynamic();	// use this for animating
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
		}public Vector2[] GetUVs()
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

        /**/

		/*public void SetVertices(out Vector3[] exportedVertices)
		{
			exportedVertices = new Vector3[buildPointer.vertIndex];
			for (int i = 0; i < exportedVertices.Length; i++)
			{
                exportedVertices[i] = vertices[i];
			}
		}*/

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


		public void InitializeBoneWeights(int maxCacheVerts, List<BoneData> boneDatas)
		{
			//weights
			hasWeights = 1;
			boneWeightsIndexes0 = new BlitableArray<int>(maxCacheVerts, Unity.Collections.Allocator.Persistent);
			boneWeights0 = new BlitableArray<float>(maxCacheVerts, Unity.Collections.Allocator.Persistent);
			bonePositions = new BlitableArray<float3>(boneDatas.Count, Unity.Collections.Allocator.Persistent);
			boneRotations = new BlitableArray<quaternion>(boneDatas.Count, Unity.Collections.Allocator.Persistent);
			boneInfluences = new BlitableArray<float>(boneDatas.Count, Unity.Collections.Allocator.Persistent);
			for (int i = 0; i < boneDatas.Count; i++)
			{
				boneWeightsIndexes0[i] = 0;
				boneWeights0[i] = 0;
				boneInfluences[i] = boneDatas[i].influence;
				//bones[i] = boneDatas[i].position + new float3(0.5f, 1f, 0.5f);	// plus half mesh bounds
				//bonePositions[i] = (new float3(boneDatas[i].position.x,
				//	boneDatas[i].position.y, boneDatas[i].position.z + 0.15f) + new float3(0.5f, 1f, 0.5f));
				bonePositions[i] = boneDatas[i].position;
				boneRotations[i] = boneDatas[i].rotation;
			}
		}

		public Color[] GetWeightsAsColors()
		{
			Color[] weights = new Color[buildPointer.vertIndex];
			for (int i = 0; i < weights.Length; i++)
			{
				if (boneWeightsIndexes0[i] == 0)
				{
					weights[i] = new Color(0, boneWeights0[i], 0);
				}
				else if(boneWeightsIndexes0[i] == 1)
				{
					weights[i] = new Color(boneWeights0[i], 0, 0);
				}
				else if (boneWeightsIndexes0[i] == 2)
				{
					weights[i] = new Color(0, 0, boneWeights0[i]);
				}
				else if (boneWeightsIndexes0[i] == 3)
				{
					weights[i] = new Color(0, boneWeights0[i], boneWeights0[i]);
				}
				else if (boneWeightsIndexes0[i] == 4)
				{
					weights[i] = new Color(boneWeights0[i], 0, boneWeights0[i]);
				}
				else if (boneWeightsIndexes0[i] == 5)
				{
					weights[i] = new Color(boneWeights0[i], boneWeights0[i], 0);
				}
				else if (boneWeightsIndexes0[i] == 6)
				{
					weights[i] = new Color(0.5f, 0.5f, boneWeights0[i]);
				}
				else if (boneWeightsIndexes0[i] == 7)
				{
					weights[i] = new Color(0, 0.5f, boneWeights0[i]);
				}
				else if (boneWeightsIndexes0[i] == 8)
				{
					weights[i] = new Color(0.5f, 0, boneWeights0[i]);
				}
				else if (boneWeightsIndexes0[i] == 9)
				{
					weights[i] = new Color(0.5f, boneWeights0[i], 0.5f);
				}
				else if (boneWeightsIndexes0[i] == 10)
				{
					weights[i] = new Color(0.5f, boneWeights0[i], 0);
				}
				else if (boneWeightsIndexes0[i] == 11)
				{
					weights[i] = new Color(0, boneWeights0[i], 0.5f);
				}
				else if (boneWeightsIndexes0[i] == 12)
				{
					weights[i] = new Color(boneWeights0[i], 0.5f, 0.5f);
				}
				else
				{
					weights[i] = new Color(boneWeights0[i], boneWeights0[i], boneWeights0[i]);
				}
			}
			return weights;
		}

		public Matrix4x4[] GetBonePoses()
		{
			Matrix4x4[] poses = new Matrix4x4[bonePositions.Length];
			for (int i = 0; i < poses.Length; i++)
			{
				poses[i] = new Matrix4x4();
				//Transform test = new GameObject("Boom").transform;
				//test.position = bonePositions[i];

				//poses[i] = test.worldToLocalMatrix;
				//GameObject.Destroy(test.gameObject);
				poses[i].SetTRS(bonePositions[i], boneRotations[i], new Vector3(1, 1, 1));
				poses[i] = Matrix4x4.Inverse(poses[i]);
				//poses[i].c0.x = bones[i].x;
				//poses[i].c1.y = bones[i].y;
				//poses[i].c2.z = bones[i].z;
				//poses[i].c3.w = 1;	
				//poses[i] = math.float4x4(new float4(bones[i].x, bones[i].y, bones[i].z, 0)));
			}
			//Debug.LogError("Getting bone poses length: " + poses.Length);
			return poses;
		}

		public BoneWeight[] GetWeights()
		{
			BoneWeight[] weights = new BoneWeight[buildPointer.vertIndex];
			for (int i = 0; i < weights.Length; i++)
			{
				weights[i] = new BoneWeight
				{
					boneIndex0 = boneWeightsIndexes0[i],
					weight0 = boneWeights0[i]
				};
			}
			return weights;
		}

		public void InitializeData(int3 voxelDimensions, int maxCacheVerts, int maxCacheTriangles)
		{
			//float3 voxelDimensions = new float3(16, 64, 16);
			int xyzSize = (int)(voxelDimensions.x * voxelDimensions.y * voxelDimensions.z);
			vertices = new BlitableArray<ZoxelVertex>(maxCacheVerts, Unity.Collections.Allocator.Persistent);
			/*vertices = new BlitableArray<float3>(maxCacheVerts, Unity.Collections.Allocator.Persistent);
			colors = new BlitableArray<float3>(maxCacheVerts, Unity.Collections.Allocator.Persistent);
			uvs = new BlitableArray<float2>(maxCacheVerts, Unity.Collections.Allocator.Persistent);*/
			triangles = new BlitableArray<int>(maxCacheTriangles, Unity.Collections.Allocator.Persistent);
			sidesBack = new BlitableArray<byte>(xyzSize, Unity.Collections.Allocator.Persistent);
			sidesDown = new BlitableArray<byte>(xyzSize, Unity.Collections.Allocator.Persistent);
			sidesUp = new BlitableArray<byte>(xyzSize, Unity.Collections.Allocator.Persistent);
			sidesForward = new BlitableArray<byte>(xyzSize, Unity.Collections.Allocator.Persistent);
			sidesLeft = new BlitableArray<byte>(xyzSize, Unity.Collections.Allocator.Persistent);
			sidesRight = new BlitableArray<byte>(xyzSize, Unity.Collections.Allocator.Persistent);
		}

		
		public void Dispose()
		{
			if (modelIndexes.Length > 0) {
				modelIndexes.Dispose();
				voxelColors.Dispose();
				uvMaps.Dispose();
			}

			if (sidesUp.Length > 0) {
				sidesUp.Dispose();
				sidesDown.Dispose();
				sidesLeft.Dispose();
				sidesRight.Dispose();
				sidesBack.Dispose();
				sidesForward.Dispose();
			}

			if (vertices.Length > 0) {
				vertices.Dispose();
				triangles.Dispose();
				//uvs.Dispose();
				//colors.Dispose();
			}

			if (hasWeights == 1)
			{
				boneWeightsIndexes0.Dispose();
				boneWeights0.Dispose();
				bonePositions.Dispose();
				boneRotations.Dispose();
				boneInfluences.Dispose();
			}
			Value.Dispose();
		}

		public static void Destroy(EntityManager entityManager, Entity e)
		{
			if (entityManager.Exists(e))
			{
				//if (entityManager.HasComponent<RenderMesh>(e))
				{
					/*if (Application.isPlaying)
					{
						GameObject.Destroy(entityManager.GetSharedComponentData<RenderMesh>(e).mesh);
					}
					else
					{
						GameObject.DestroyImmediate(entityManager.GetSharedComponentData<RenderMesh>(e).mesh);
					}*/
				}
				if (entityManager.HasComponent<ChunkRenderer>(e))
				{
					ChunkRenderer chunkRenderer = entityManager.GetComponentData<ChunkRenderer>(e);
					//chunkRenderer.Dispose();
				}
				entityManager.DestroyEntity(e);
			}
		}

	}

}