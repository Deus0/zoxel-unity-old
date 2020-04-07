using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Burst;
using Unity.Rendering;
using System.Linq;
using Unity.Collections;
using Unity.Rendering;
using UnityEngine.Rendering;

using Unity.Jobs;

namespace Zoxel.Voxels
{
    public struct RenderData : IComponentData
    {
        public byte dirty;
        //public NativeArray<ZoxelVertex> vertices;
    }


    public struct VertCache
    {
        public NativeArray<ZoxelVertex> vertices;

        public void Init(int count)
        {
            Dispose();
            vertices = new NativeArray<ZoxelVertex>(count, Allocator.Persistent);
        }

        public void Dispose()
        {
            if (vertices.Length > 0)
            {
                vertices.Dispose();
            }
        }
    }

    public class ChunkRenderProcessingSystem : JobComponentSystem
	{

		[BurstCompile]
		struct ChunkMeshFinisherJob : IJobForEach<ChunkMeshAnimation, ChunkMesh>
		{
			public void Execute(ref ChunkMeshAnimation chunkMeshAnimation, ref ChunkMesh chunkMesh)
			{
                if (!(chunkMesh.buildPointer.vertIndex > chunkMeshAnimation.boneIndexes.Length
                    || chunkMesh.buildPointer.vertIndex > chunkMesh.vertices.Length))
                {
                    ApplyBones(ref chunkMesh, ref chunkMeshAnimation, chunkMesh.buildPointer.vertIndex);
                }
            }

            public void ApplyBones(ref ChunkMesh chunkMesh, ref ChunkMeshAnimation chunkMeshAnimation, int vertsCount)
            {
                //var verticesA = chunkMesh.vertices.ToArray();
                //var boneIndexes = chunkMeshAnimation.boneIndexes.ToArray();
                //var bonePositions = chunkMeshAnimation.bonePositions.ToArray();
                //var boneRotations = chunkMeshAnimation.boneRotations.ToArray();
                //float timeBegun = UnityEngine.Time.realtimeSinceStartup;
                for (int i = 0; i < vertsCount; i++)
                {
                    var vert = chunkMesh.vertices[i];
                    int boneIndex = chunkMeshAnimation.boneIndexes[i];
                    var position = vert.position;
                    //int bonesCount = skeleton.bones.Length;//math.min(skeleton.bones.Length, 3);
                    quaternion boneRotation = chunkMeshAnimation.boneRotations[boneIndex]; // World.EntityManager.GetComponentData<Rotation>(boneEntity).Value;
                    position += chunkMeshAnimation.bonePositions[boneIndex];
                    //vert.position = math.rotate(boneRotation, position);
                    vert.position = position;
                    /*if (Bootstrap.instance.isDebugWeightColours)
                    {
                        var color = vert.color;
                        color = weightColours[boneIndex];
                        vert.color = color;
                    }*/
                    chunkMeshAnimation.vertices[i] = vert;
                }
                //float timeEnded = UnityEngine.Time.realtimeSinceStartup;
                //Debug.LogError("Time taking to process verts: " + vertsCount + ":" + 1000 * (timeEnded - timeBegun) + "ms");
            }
        }

		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			return new ChunkMeshFinisherJob { }.Schedule(this, inputDeps);
		}
	}

    /// <summary>
    /// Turns chunkRenders into meshes on the entities
    ///     Todo:
    ///         Move maths to a job system - do it in paralell
    ///         For this, update mesh thread, grab the array from a ComponentData and convert to NativeArray - then push to mesh
    /// </summary>
    [DisableAutoCreation, UpdateAfter(typeof(ChunkMeshEndingSystem))]
    public class ChunkRendererAnimationSystem : ComponentSystem
    {
        public Dictionary<int, VertCache> caches = new Dictionary<int, VertCache>();
        public const float animationSpeed = 0; // 1f / 4f;
        private static float3[] weightColours;

        protected override void OnCreate()
        {
            weightColours = new float3[18];
            for (int i = 0; i < weightColours.Length; i++)
            {
                weightColours[i] = new float3(UnityEngine.Random.Range(0, 1f), UnityEngine.Random.Range(0, 1f), UnityEngine.Random.Range(0, 1f));
            }
        }

        protected override void OnDestroy()
        {
            foreach (var cache in caches.Values)
            {
                cache.Dispose();
            }
        }

        protected override void OnUpdate()
		{
            Entities.WithAll<ChunkRenderer, ChunkMesh>().ForEach((Entity e, ref ChunkRenderer chunkRenderer, ref ChunkMesh chunkMesh, ref ChunkMeshAnimation chunkMeshAnimation) =>
            {
                if (World.EntityManager.HasComponent<ChunkMeshLink>(e) == false)
                {
                    UpdateRender(e, ref chunkRenderer, ref chunkMesh, ref chunkMeshAnimation);
                }
            });
		}

        // add noise to render, using original verts and making new one
		public void UpdateRender(Entity entity, ref ChunkRenderer chunkRenderer, ref ChunkMesh chunkMesh, ref ChunkMeshAnimation chunkMeshAnimation)
        {
            if (UnityEngine.Time.time - chunkMesh.timePassed >= animationSpeed)
            {
                chunkMesh.timePassed = UnityEngine.Time.time;
                var chunk = World.EntityManager.GetComponentData<Chunk>(chunkRenderer.chunk);
                if (World.EntityManager.Exists(chunk.world) == false || !World.EntityManager.HasComponent<World>(chunk.world))
                {
                    return;
                }
                var worldComponent = World.EntityManager.GetComponentData<World>(chunk.world);
                var renderer = World.EntityManager.GetSharedComponentData<RenderMesh>(entity);
                var mesh = renderer.mesh;
                // apply animation
                if (World.EntityManager.HasComponent<Skeleton>(chunk.world))
                {
                    var skeleton = World.EntityManager.GetComponentData<Skeleton>(chunk.world);
                    if (skeleton.bones.Length > 0)
                    {
                        var vertsCount = chunkMesh.buildPointer.vertIndex;
                        float3[] bonePositions = new float3[skeleton.bones.Length];
                        quaternion[] boneRotations = new quaternion[skeleton.bones.Length];
                        for (int i = 0; i < skeleton.bones.Length; i++)
                        {
                            var boneEntity = skeleton.bones[i];
                            //originalBonePositions[i] = World.EntityManager.GetComponentData<Bone>(boneEntity).position;
                            //bonePositions[i] = World.EntityManager.GetComponentData<Translation>(boneEntity).Value;
                            boneRotations[i] = World.EntityManager.GetComponentData<Rotation>(boneEntity).Value;
                            bonePositions[i] = World.EntityManager.GetComponentData<Translation>(boneEntity).Value - World.EntityManager.GetComponentData<Bone>(boneEntity).position;
                            //Debug.LogError(i + " - Bone Position: " + bonePositions[i]);
                        }
                        chunkMeshAnimation.SetBones(bonePositions, boneRotations);

                        var layout = new[]
                        {
                            new VertexAttributeDescriptor(UnityEngine.Rendering.VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
                            new VertexAttributeDescriptor(UnityEngine.Rendering.VertexAttribute.Color, VertexAttributeFormat.Float32, 3),
                            new VertexAttributeDescriptor(UnityEngine.Rendering.VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2),
                        };
                        mesh.SetVertexBufferParams(vertsCount, layout);
                        //mesh.SetVertexBufferData(cache.vertices, 0, 0, vertsCount);
                        mesh.SetVertexBufferData(chunkMeshAnimation.GetTempArray(), 0, 0, vertsCount);
                        mesh.UploadMeshData(false);
                        //mesh.RecalculateBounds();
                        //mesh.RecalculateNormals();
                    }
                    else
                    {
                        return;
                        //newVerts = ApplyRandomNoise(caches[zoxID.id].verts);
                    }
                }
                else
                {
                    return;
                }
            }
        }

		/*public void ApplyBones(ref ChunkMesh chunkMesh, ref ChunkMeshAnimation chunkMeshAnimation, int vertsCount, Skeleton skeleton)
		{
            //float3[] originalBonePositions = new float3[skeleton.boneIndexes.Length];
            float3[] bonePositions = new float3[skeleton.bones.Length];
            quaternion[] boneRotations = new quaternion[skeleton.bones.Length];
            for (int i = 0; i < skeleton.bones.Length; i++)
            {
                var boneEntity = skeleton.bones[i];
                //originalBonePositions[i] = World.EntityManager.GetComponentData<Bone>(boneEntity).position;
                //bonePositions[i] = World.EntityManager.GetComponentData<Translation>(boneEntity).Value;
                boneRotations[i] = World.EntityManager.GetComponentData<Rotation>(boneEntity).Value;
                bonePositions[i] = World.EntityManager.GetComponentData<Translation>(boneEntity).Value - World.EntityManager.GetComponentData<Bone>(boneEntity).position;
                //Debug.LogError(i + " - Bone Position: " + bonePositions[i]);
            }
			//NativeArray<ZoxelVertex> outputVerts = new NativeArray<ZoxelVertex>(verts.Length, Allocator.Temp);
            var verticesA = chunkMesh.vertices.ToArray();
            var boneIndexes = skeleton.boneIndexes.ToArray();
            float timeBegun = UnityEngine.Time.realtimeSinceStartup;
			for (int i = 0; i < vertsCount; i++)
			{
                // get bone difference of position
                //var boneEntity = skeleton.bones[boneIndex];
                //var bone = World.EntityManager.GetComponentData<Bone>(boneEntity);
                // get difference of positions
                //float3 bonePosition = bonePositions[boneIndex]; // World.EntityManager.GetComponentData<Translation>(boneEntity).Value;
                //Matrix4x4 boneMatrix = World.EntityManager.GetComponentData<LocalToWorld>(boneEntity).Value;
                //Vector3 bonePositionV = boneMatrix.GetColumn(3);
                //float3 bonePosition = bonePositionV;
                //var difference = bonePosition - originalBonePositions[boneIndex];
				var vert = verticesA[i];
				//var vert = new ZoxelVertex {
				//	position = new float3(verto.position.x, verto.position.y, verto.position.z),
				//	uv = verto.uv,
				//	color = verto.color
				//};
				var position = vert.position;
                int bonesCount = skeleton.bones.Length;//math.min(skeleton.bones.Length, 3);
                int boneIndex = boneIndexes[i];
                quaternion boneRotation = boneRotations[boneIndex]; // World.EntityManager.GetComponentData<Rotation>(boneEntity).Value;
                position += bonePositions[boneIndex];
				//vert.position = math.rotate(boneRotation, position);
                vert.position = position;
                if (Bootstrap.instance.isDebugWeightColours)
                {
                    var color = vert.color;
                    color = weightColours[boneIndex];
                    vert.color = color;
                }
				chunkMeshAnimation.vertices[i] = vert;
			}
            float timeEnded = UnityEngine.Time.realtimeSinceStartup;
            Debug.LogError("Time taking to process verts: " + vertsCount + ":" + 1000 * (timeEnded - timeBegun) + "ms");
		}*/
        
		public NativeArray<ZoxelVertex> ApplySimplexNoise(NativeArray<ZoxelVertex> verts)
		{
			float noiseAmplitude = 0.02f;
			float noiseValue = 0.5f;
			NativeArray<ZoxelVertex> outputVerts = new NativeArray<ZoxelVertex>(verts.Length, Allocator.Temp);
			for (int i = 0; i < verts.Length; i++)
			{
				var verto = verts[i];
				var vert = new ZoxelVertex {
					position = new float3(verto.position.x, verto.position.y, verto.position.z),
					uv = verto.uv,
					color = verto.color
				};
				var position = vert.position;
				// applies 3600 verts - this mathematics
                var time = UnityEngine.Time.time;
                float noiseA = noise.snoise(new float3(position.x, position.y + time, position.z - time) * noiseValue);
                //float noiseX = noise.snoise(new float2(position.x, UnityEngine.Time.time) * noiseValue);
                //float noiseY = noise.snoise(new float2(position.y, UnityEngine.Time.time) * noiseValue);
                //float noiseZ = noise.snoise(new float2(position.z, UnityEngine.Time.time) * noiseValue);
				position += noiseAmplitude * (new float3(noiseA, noiseA, noiseA));
				vert.position = position;
				outputVerts[i] = vert;
			}
			return outputVerts;
		}

		public NativeArray<ZoxelVertex> ApplyRandomNoise(NativeArray<ZoxelVertex> verts)
		{
			float noiseAmplitude = 0.02f;
			float noiseValue = 0.5f;
			NativeArray<ZoxelVertex> outputVerts = new NativeArray<ZoxelVertex>(verts.Length, Allocator.Temp);
			for (int i = 0; i < verts.Length; i++)
			{
				var verto = verts[i];
				var vert = new ZoxelVertex {
					position = new float3(verto.position.x, verto.position.y, verto.position.z),
					uv = verto.uv,
					color = verto.color
				};
				var position = vert.position;
				position += noiseAmplitude * (new float3(
					UnityEngine.Random.Range(-1, 1), 
					UnityEngine.Random.Range(-1, 1), 
					UnityEngine.Random.Range(-1, 1)));
				vert.position = position;
				outputVerts[i] = vert;
			}
			return outputVerts;
		}
    }
}
                /*var triangles2 = caches[zoxID.id].triangles;
                //mesh.SetIndexBufferParams(triangles2.Length, IndexFormat.UInt32);
                //mesh.SetIndexBufferData(triangles2, 0, 0, triangles2.Length);
                mesh.SetSubMesh(0, new SubMeshDescriptor() {
                    baseVertex = 0,
                    bounds = default,
                    indexStart = 0,
                    indexCount = triangles2.Length,
                    firstVertex = 0,
                    topology = MeshTopology.Triangles,
                    vertexCount = newVerts.Length
                });*/
                //renderer.mesh = mesh;
                //World.EntityManager.SetSharedComponentData(entity, renderer);


                        //newVerts = ApplyRandomNoise(newVerts);
                        //var id = World.EntityManager.GetComponentData<ZoxID>(entity).id;
                        //NativeArray<ZoxelVertex> newVerts;
                        /*VertCache cache;
                        if (caches.ContainsKey(id) == false)
                        {
                            chunkMeshAnimation.dirty = 0;
                            cache = new VertCache { vertices = chunkMeshAnimation.GetVertexNativeArray() };
                            caches.Add(id, cache);
                        }
                        else
                        {
                            if (chunkMeshAnimation.dirty == 1)
                            {
                                chunkMeshAnimation.dirty = 0;
                                //Debug.LogError("Anim was dirty.");
                                caches[id].Dispose();
                                cache = new VertCache { vertices = chunkMeshAnimation.GetVertexNativeArray() };
                                caches[id] = cache;
                            }
                            else {
                                cache = caches[id];
                                chunkMeshAnimation.CopyTo(ref cache);
                            }
                        }*/