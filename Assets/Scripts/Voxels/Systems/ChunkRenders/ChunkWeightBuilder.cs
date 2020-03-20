using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;

namespace Zoxel.Voxels
{

    [DisableAutoCreation, UpdateAfter(typeof(ChunkMeshBuilderSystem))]
    public class ChunkWeightBuilder : JobComponentSystem
    {
        [BurstCompile]
        struct ChunkMeshBuilderJob : IJobForEach<ChunkRendererBuilder, ChunkRenderer> //IJobForEach<ChunkRenderer>
        {
            public void Execute(ref ChunkRendererBuilder chunkRendererBuilder, ref ChunkRenderer chunk)   //Entity entity, int index,  
            {
                if ((chunkRendererBuilder.state == 3) && 
                    (chunk.hasWeights == 1))
                {
                    /*if (ChunkSpawnSystem.isDebugLog)
                    {
                        UnityEngine.Debug.LogError("Processing ChunkWeightBuilder meshes.");
                    }*/
                    chunkRendererBuilder.state = 4;
                    // for each bone
                    for (int i = 0; i < chunk.bonePositions.Length; i++)
                    {
                        float influence = chunk.boneInfluences[i];
                        //DrawDebugSphere(chunk.bones[i], influence);
                        // for each bone, fight weights within radius using vertexes
                        for (int j = 0; j < chunk.vertices.Length; j++)
                        {
                            float distanceTo = math.distance(chunk.vertices[j].position, chunk.bonePositions[i]);
                            if (distanceTo < influence)
                            {
                                chunk.boneWeights0[j] = 1;
                                chunk.boneWeightsIndexes0[j] = i;
                            }
                        }
                    }
                }
            }

            private void DrawDebugSphere(float3 position, float sphereDebugRadius)
            {
                position = position * 16;
                sphereDebugRadius *= 16;
                UnityEngine.Debug.DrawLine(position, position + new float3(0, sphereDebugRadius, 0),
                    UnityEngine.Color.red, 5);
                UnityEngine.Debug.DrawLine(position, position + new float3(0, -sphereDebugRadius, 0),
                    UnityEngine.Color.red, 5);
                UnityEngine.Debug.DrawLine(position, position + new float3(sphereDebugRadius, 0, 0),
                    UnityEngine.Color.red, 5);
                UnityEngine.Debug.DrawLine(position, position + new float3(-sphereDebugRadius, 0, 0),
                    UnityEngine.Color.red, 5);
                UnityEngine.Debug.DrawLine(position, position + new float3(0, 0, sphereDebugRadius),
                    UnityEngine.Color.red, 5);
                UnityEngine.Debug.DrawLine(position, position + new float3(0, 0, -sphereDebugRadius),
                    UnityEngine.Color.red, 5);
                UnityEngine.Debug.DrawLine(position, position + new float3(0, sphereDebugRadius, sphereDebugRadius),
                    UnityEngine.Color.red, 5);
                UnityEngine.Debug.DrawLine(position, position + new float3(0, sphereDebugRadius, -sphereDebugRadius),
                    UnityEngine.Color.red, 5);
                UnityEngine.Debug.DrawLine(position, position + new float3(sphereDebugRadius, sphereDebugRadius, 0),
                    UnityEngine.Color.red, 5);
                UnityEngine.Debug.DrawLine(position, position + new float3(sphereDebugRadius, -sphereDebugRadius, 0),
                    UnityEngine.Color.red, 5);
            }
        }
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return new ChunkMeshBuilderJob { }.Schedule(this, inputDeps);
        }
    }
}