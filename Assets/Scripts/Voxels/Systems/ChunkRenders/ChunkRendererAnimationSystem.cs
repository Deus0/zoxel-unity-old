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

    public struct VertCache
    {
        public NativeArray<ZoxelVertex> verts;
        public NativeArray<int> triangles;

        public void Init(int count, int count2)
        {
            verts = new NativeArray<ZoxelVertex>(count, Allocator.Persistent);
            triangles = new NativeArray<int>(count2, Allocator.Persistent);
        }
    }

    /// <summary>
    /// Turns chunkRenders into meshes on the entities
    /// </summary>
    [DisableAutoCreation, UpdateAfter(typeof(ChunkMeshEndingSystem))]
    public class ChunkRendererAnimationSystem : ComponentSystem
    {
        public Dictionary<int, VertCache> caches = new Dictionary<int, VertCache>();

        protected override void OnUpdate()
		{
            Entities.WithAll<ChunkRenderer>().ForEach((Entity e, ref ChunkRenderer renderer) =>
            {
                if (!World.EntityManager.HasComponent<ChunkRendererBuilder>(e))
                {
                    UpdateRender(e, ref renderer);
                }
            });
		}

        // add noise to render, using original verts and making new one
		public void UpdateRender(Entity entity, ref ChunkRenderer chunk)
        {
            Chunk chunkComponent = World.EntityManager.GetComponentData<Chunk>(chunk.chunk);
            World worldComponent = World.EntityManager.GetComponentData<World>(chunkComponent.world);
            if (worldComponent.modelID != 0 && UnityEngine.Time.time - chunk.timePassed >= (1f/ 4f))
            {
                chunk.timePassed = UnityEngine.Time.time;
                RenderMesh renderer = World.EntityManager.GetSharedComponentData<RenderMesh>(entity);
                Mesh mesh = renderer.mesh;
                if (mesh.vertices.Length == 0)
                {
                    return;
                }
                // if no cache
                ZoxID zoxID = World.EntityManager.GetComponentData<ZoxID>(entity);
                if (!caches.ContainsKey(zoxID.id))
                {
                    var verts = chunk.GetVertexArray(mesh);
                    verts = ChunkRenderer.CentreMesh(verts, mesh);
                    verts = ChunkRenderer.RotateMesh(verts);
                    var tris = chunk.GetTrianglesNativeArray(mesh);
                    caches.Add(zoxID.id, new VertCache { verts = verts, triangles = tris });
                }
                var newVerts = ApplyNoise(caches[zoxID.id].verts);
                //var newVerts = (caches[zoxID.id].verts);
                var layout = new[]
                {
                    new VertexAttributeDescriptor(UnityEngine.Rendering.VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
                    new VertexAttributeDescriptor(UnityEngine.Rendering.VertexAttribute.Color, VertexAttributeFormat.Float32, 3),
                    new VertexAttributeDescriptor(UnityEngine.Rendering.VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2),
                };
                mesh.SetVertexBufferParams(newVerts.Length, layout);
                mesh.SetVertexBufferData(newVerts, 0, 0, newVerts.Length);
                mesh.UploadMeshData(false);
            }
        }
		public NativeArray<ZoxelVertex> ApplyNoise(NativeArray<ZoxelVertex> verts)
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
				/*position += noiseAmplitude * (new float3(
					noise.snoise(new float2(position.x, UnityEngine.Time.time) * noiseValue),
					noise.snoise(new float2(position.y, UnityEngine.Time.time) * noiseValue),
					noise.snoise(new float2(position.z, UnityEngine.Time.time) * noiseValue)
				));*/
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