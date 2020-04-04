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

namespace Zoxel.Voxels
{
    public struct RenderData : IComponentData
    {
        public byte dirty;
        //public NativeArray<ZoxelVertex> vertices;
    }


    public struct VertCache
    {
        public NativeArray<ZoxelVertex> verts;

        public void Init(int count, int count2)
        {
            verts = new NativeArray<ZoxelVertex>(count, Allocator.Persistent);
        }

        public void Dispose()
        {
            verts.Dispose();
        }
    }

    /// <summary>
    /// Turns chunkRenders into meshes on the entities
    /// </summary>
    [DisableAutoCreation, UpdateAfter(typeof(ChunkMeshEndingSystem))]
    public class ChunkRendererAnimationSystem : ComponentSystem
    {
        public Dictionary<int, VertCache> caches = new Dictionary<int, VertCache>();
        public const float animationSpeed = 1f / 8f;

        protected override void OnUpdate()
		{
            Entities.WithAll<ChunkRenderer>().ForEach((Entity e, ref ChunkRenderer renderer) =>
            {
                if (!World.EntityManager.HasComponent<ChunkRendererBuilder>(e))
                {
                    UpdateRender(e, ref renderer);
                }
                if (World.EntityManager.HasComponent<RenderData>(e) == false)
                {
                    World.EntityManager.AddComponentData(e, new RenderData{});
                }
            });
		}

        protected override void OnDestroy()
        {
            foreach (var cache in caches.Values)
            {
                cache.Dispose();
            }
        }

        // add noise to render, using original verts and making new one
		public void UpdateRender(Entity entity, ref ChunkRenderer chunk)
        {
            if (World.EntityManager.Exists(chunk.chunk) == false)
            {
                Debug.LogError("Leftover chunkRender from chunk: " + chunk.Value.chunkPosition);
                return;
            }
            if (World.EntityManager.HasComponent<Chunk>(chunk.chunk) == false)
            {
                return;
            }
            Chunk chunkComponent = World.EntityManager.GetComponentData<Chunk>(chunk.chunk);
            if (World.EntityManager.Exists(chunkComponent.world) == false)
            {
                return;
            }
            if (!World.EntityManager.HasComponent<World>(chunkComponent.world))
            {
                return;
            }
            var worldComponent = World.EntityManager.GetComponentData<World>(chunkComponent.world);
            if (worldComponent.modelID != 0 && UnityEngine.Time.time - chunk.timePassed >= animationSpeed)
            {
                chunk.timePassed = UnityEngine.Time.time;
                var renderer = World.EntityManager.GetSharedComponentData<RenderMesh>(entity);
                var mesh = renderer.mesh;
                if (mesh.vertices.Length == 0)
                {
                    return;
                }
                // if no cache
                var zoxID = World.EntityManager.GetComponentData<ZoxID>(entity);
                if (!caches.ContainsKey(zoxID.id))
                {
                    var verts = chunk.GetVertexArray(mesh);
                    caches.Add(zoxID.id, new VertCache { verts = verts });
                    
                    if (World.EntityManager.HasComponent<Skeleton>(chunkComponent.world))
                    {
                        var skeleton = World.EntityManager.GetComponentData<Skeleton>(chunkComponent.world);
                        if (skeleton.bones.Length > 0)
                        {
                            skeleton.BakeWeights(World.EntityManager, verts);
                            World.EntityManager.SetComponentData(chunkComponent.world, skeleton);
                        }
                    }
                }
                // if mesh has updated, update the cache
                if (mesh.vertices.Length != caches[zoxID.id].verts.Length)
                {
                    var verts = chunk.GetVertexArray(mesh);
                    caches[zoxID.id] =  new VertCache { verts = verts };
                }

                // apply animation
                NativeArray<ZoxelVertex> newVerts;
                if (World.EntityManager.HasComponent<Skeleton>(chunkComponent.world))
                {
                    var skeleton = World.EntityManager.GetComponentData<Skeleton>(chunkComponent.world);
                    if (skeleton.bones.Length > 0)
                    {
                        newVerts = ApplyBones(caches[zoxID.id].verts, skeleton);
                        //newVerts = ApplyRandomNoise(newVerts);
                    }
                    else
                    {
                        //return;
                        newVerts = ApplyRandomNoise(caches[zoxID.id].verts);
                    }
                }
                else
                {
                    //return;
                    newVerts = ApplyRandomNoise(caches[zoxID.id].verts);
                }

                var layout = new[]
                {
                    new VertexAttributeDescriptor(UnityEngine.Rendering.VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
                    new VertexAttributeDescriptor(UnityEngine.Rendering.VertexAttribute.Color, VertexAttributeFormat.Float32, 3),
                    new VertexAttributeDescriptor(UnityEngine.Rendering.VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2),
                };
                mesh.SetVertexBufferParams(newVerts.Length, layout);
                mesh.SetVertexBufferData(newVerts, 0, 0, newVerts.Length);
                mesh.UploadMeshData(false);
                newVerts.Dispose();
            }
        }
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

		public NativeArray<ZoxelVertex> ApplyBones(NativeArray<ZoxelVertex> verts, Skeleton skeleton)
		{
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
                int bonesCount = skeleton.bones.Length;//math.min(skeleton.bones.Length, 3);

                int boneIndex = skeleton.boneIndexes[i];
                // get bone difference of position
                var boneEntity = skeleton.bones[boneIndex];
                var bone = World.EntityManager.GetComponentData<Bone>(boneEntity);
                // get difference of positions
                float3 bonePosition = World.EntityManager.GetComponentData<Translation>(boneEntity).Value;
                //Matrix4x4 boneMatrix = World.EntityManager.GetComponentData<LocalToWorld>(boneEntity).Value;
                //Vector3 bonePositionV = boneMatrix.GetColumn(3);
                //float3 bonePosition = bonePositionV;
                var difference = bonePosition - bone.position;

                position += difference;
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