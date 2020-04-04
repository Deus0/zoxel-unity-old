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

	public struct ChunkRendererWeights : IComponentData
	{
		public BlitableArray<int> boneWeightsIndexes0;
		public BlitableArray<float> boneWeights0;
		public BlitableArray<float3> bonePositions;
		public BlitableArray<quaternion> boneRotations;
		public BlitableArray<float> boneInfluences;

		public void Dispose()
		{
			if (boneWeightsIndexes0.Length > 0)
			{
				boneWeightsIndexes0.Dispose();
				boneWeights0.Dispose();
				bonePositions.Dispose();
				boneRotations.Dispose();
				boneInfluences.Dispose();
			}
        }
        		
        public void InitializeBoneWeights(int maxCacheVerts, List<BoneData> boneDatas)
		{
			//weights
			//hasWeights = 1;
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
			Color[] weights = new Color[boneWeightsIndexes0.Length];
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
			BoneWeight[] weights = new BoneWeight[boneWeightsIndexes0.Length];
			for (int i = 0; i < boneWeightsIndexes0.Length; i++)
			{
				weights[i] = new BoneWeight
				{
					boneIndex0 = boneWeightsIndexes0[i],
					weight0 = boneWeights0[i]
				};
			}
			return weights;
		}
		
    }
}