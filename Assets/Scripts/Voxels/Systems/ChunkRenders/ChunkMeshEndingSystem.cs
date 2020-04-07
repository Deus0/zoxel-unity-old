using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Burst;
using Unity.Rendering;
using System.Linq;
using Unity.Jobs;
using Unity.Rendering;
using UnityEngine.Rendering;
using Unity.Collections;

namespace Zoxel.Voxels
{
    /// <summary>
    /// Turns chunkRenders into meshes on the entities
    /// </summary>
    [DisableAutoCreation, UpdateAfter(typeof(ChunkMeshBuilderSystem))]
    public class ChunkMeshEndingSystem : ComponentSystem
    {
        protected override void OnUpdate()
		{
            Entities.WithAll<ChunkMesh>().ForEach((Entity e, ref ChunkMesh chunkMesh) =>
            {
                if (chunkMesh.verticesDirty == 1)
                {
                    chunkMesh.verticesDirty = 0;
                    chunkMesh.isPushMesh = 1;
                    var vertsNativeArray = chunkMesh.GetVertexNativeArray();
                    var layout = new[]
                    {
                        new VertexAttributeDescriptor(UnityEngine.Rendering.VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
                        new VertexAttributeDescriptor(UnityEngine.Rendering.VertexAttribute.Color, VertexAttributeFormat.Float32, 3),
                        new VertexAttributeDescriptor(UnityEngine.Rendering.VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2),
                    };
                    Mesh mesh = null;
                    if (World.EntityManager.HasComponent<ChunkMeshLink>(e))
                    {
                        mesh = World.EntityManager.GetSharedComponentData<ChunkMeshLink>(e).mesh;
                    }
                    else if (World.EntityManager.HasComponent<RenderMesh>(e))
                    {
                        mesh = World.EntityManager.GetSharedComponentData<RenderMesh>(e).mesh;
                    }
                    mesh.SetVertexBufferParams(chunkMesh.buildPointer.vertIndex, layout);
                    mesh.SetVertexBufferData(vertsNativeArray, 0, 0, chunkMesh.buildPointer.vertIndex);

                    // bake skeleton!
                    var chunkRenderer = World.EntityManager.GetComponentData<ChunkRenderer>(e);
                    var chunk = World.EntityManager.GetComponentData<Chunk>(chunkRenderer.chunk);
                    if (World.EntityManager.HasComponent<Skeleton>(chunk.world))
                    {
                        var skeleton = World.EntityManager.GetComponentData<Skeleton>(chunk.world);
                        var chunkMeshAnimation = World.EntityManager.GetComponentData<ChunkMeshAnimation>(e);
                        if (skeleton.bones.Length > 0)
                        {
                            skeleton.BakeWeights(World.EntityManager, vertsNativeArray, chunkMesh.buildPointer.vertIndex);
                            World.EntityManager.SetComponentData(chunk.world, skeleton);
                            chunkMeshAnimation.boneIndexes = skeleton.boneIndexes;
                        }
                        chunkMeshAnimation.DisposeVertices();
                        chunkMeshAnimation.vertices = new BlitableArray<ZoxelVertex>(chunkMesh.buildPointer.vertIndex, Allocator.Persistent);
                        chunkMeshAnimation.dirty = 1;
                        World.EntityManager.SetComponentData(e, chunkMeshAnimation);
                    }
                    vertsNativeArray.Dispose();
                }
                if (chunkMesh.trianglesDirty == 1)
                {
                    chunkMesh.trianglesDirty = 0;
                    chunkMesh.isPushMesh = 1;
                    var trisNativeArray = chunkMesh.GetTrianglesNativeArray();
                    Mesh mesh = null;
                    if (World.EntityManager.HasComponent<ChunkMeshLink>(e))
                    {
                        mesh = World.EntityManager.GetSharedComponentData<ChunkMeshLink>(e).mesh;
                    }
                    else if (World.EntityManager.HasComponent<RenderMesh>(e))
                    {
                        mesh = World.EntityManager.GetSharedComponentData<RenderMesh>(e).mesh;
                    }
                    mesh.SetIndexBufferParams(chunkMesh.buildPointer.triangleIndex, IndexFormat.UInt32);
                    mesh.SetIndexBufferData(trisNativeArray, 0, 0, chunkMesh.buildPointer.triangleIndex);
                    trisNativeArray.Dispose();
                }
                if (chunkMesh.isPushMesh == 1)
                {
                    chunkMesh.isPushMesh = 0;
                    Mesh mesh = null;
                    if (World.EntityManager.HasComponent<ChunkMeshLink>(e))
                    {
                        mesh = World.EntityManager.GetSharedComponentData<ChunkMeshLink>(e).mesh;
                    }
                    else if (World.EntityManager.HasComponent<RenderMesh>(e))
                    {
                        mesh = World.EntityManager.GetSharedComponentData<RenderMesh>(e).mesh;
                    }
                    mesh.SetSubMesh(0, new SubMeshDescriptor() {
                        baseVertex = 0,
                        bounds = default,
                        indexStart = 0,
                        indexCount = chunkMesh.buildPointer.triangleIndex,
                        firstVertex = 0,
                        topology = MeshTopology.Triangles,
                        vertexCount = chunkMesh.buildPointer.vertIndex
                    });
                    mesh.UploadMeshData(false);
                    mesh.RecalculateBounds();
                    mesh.RecalculateNormals();
                    if (World.EntityManager.HasComponent<RenderBounds>(e))
                    {
                        var renderBounds = World.EntityManager.GetComponentData<RenderBounds>(e);
                        var bounds = renderBounds.Value;
                        bounds.Extents = mesh.bounds.extents;
                        bounds.Center = new float3(mesh.bounds.center.x, mesh.bounds.center.y, mesh.bounds.center.z);
                        renderBounds.Value = bounds;
                        World.EntityManager.SetComponentData(e, renderBounds);
                    }
                    //Debug.LogError("Uploaded Mesh: " + chunkMesh.buildPointer.vertIndex);
                }
            });
		}
    }
}


                    /*if (World.EntityManager.HasComponent<ChunkMeshLink>(e))
                    {
                        var chunkMeshLink = World.EntityManager.GetSharedComponentData<ChunkMeshLink>(e);
                        chunkMeshLink.mesh = mesh;
                        World.EntityManager.SetSharedComponentData<ChunkMeshLink>(e, chunkMeshLink);
                    }*/
    //[DisableAutoCreation, UpdateAfter(typeof(ChunkToRendererSystem))]
    /*public class ChunkMeshFinisherSystem : JobComponentSystem
	{
		[BurstCompile]
		struct ChunkMeshFinisherJob : IJobForEach<ChunkRendererBuilder, ChunkRenderer, ChunkMesh>
		{
			public void Execute(ref ChunkRendererBuilder chunkRendererBuilder, ref ChunkRenderer chunkRenderer, ref ChunkMesh chunkMesh) //, ref RenderBounds renderBounds)
			{
                if ((chunkRendererBuilder.state == 3 && chunkRenderer.hasWeights == 0)
                || (chunkRendererBuilder.state == 4 && chunkRenderer.hasWeights == 1))
                {
                    chunkRendererBuilder.state = 9;
                    //Chunk chunkComponent = World.EntityManager.GetComponentData<Chunk>(chunk.chunk);
                    //World worldComponent = World.EntityManager.GetComponentData<World>(chunkComponent.world);
                    //chunkRenderer.SetMeshData(ref chunkMesh, false);

                    //, worldComponent.modelID != 0);
                    //renderBounds.Value.Extents = mesh.bounds.extents;
                    //renderBounds.Value.Center = new float3(mesh.bounds.center.x, mesh.bounds.center.y, mesh.bounds.center.z);
                    //mesh.RecalculateBounds();
                    //mesh.RecalculateNormals();
                    //mesh.RecalculateTangents();
			}
		}

		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			return new ChunkMeshFinisherJob { }.Schedule(this, inputDeps);
		}
	}*/
        
        /*if (chunk.hasWeights == 1)
        {
            if (Bootstrap.DebugMeshWeights)
            {
                Debug.LogError("Debugging Weights on Model / chunk mesh.");
                mesh.colors = chunk.GetWeightsAsColors();
            }
            mesh.bindposes = chunk.GetBonePoses();
            mesh.boneWeights = chunk.GetWeights();
            //mesh.RecalculateBounds();
            // centre mesh
            mesh.vertices = mesh.vertices;
            mesh.RecalculateTangents();
        }*/
        /*public static void CentreMesh(Mesh mesh)
        {
            mesh.RecalculateBounds();
            Vector3[] verts = mesh.vertices;
            for (int i = 0; i < verts.Length; i++)
            {
                 verts[i] -= mesh.bounds.min;
            }
            for (int i = 0; i < verts.Length; i++)
            {
                verts[i] -= mesh.bounds.extents;
            }
            mesh.vertices = verts;
        }

        public static void RotateMesh(Mesh mesh)
        {
            Vector3[] verts = mesh.vertices;
            for (int i = 0; i < verts.Length; i++)
            {
                verts[i] = math.rotate(Quaternion.Euler(180, 0, 180), verts[i]);
            }
            mesh.vertices = verts;
        }*/

        /*var verts = new NativeArray<ZoxelVertex>(buildPointer.vertIndex, Allocator.Temp);
        for (int i = 0; i < verts.Length; i++)
        {
            verts[i] = vertices[i];
        }*/
        // calculate bounds from verts
        /*if (isCenter)
        {
            verts = ChunkRenderer.CentreMesh(verts);
            verts = ChunkRenderer.RotateMesh(verts);
        }*/

		/*void UpdateMesh(Entity entity, ref ChunkRenderer chunk)
        {
            Mesh mesh = null;
            var renderer = new RenderMesh();
            var chunkRenderer = new ChunkMeshLink();
            if (World.EntityManager.HasComponent<RenderMesh>(entity))
            {
                renderer = World.EntityManager.GetSharedComponentData<RenderMesh>(entity);
			    mesh = renderer.mesh;
            }
            else if (World.EntityManager.HasComponent<ChunkMeshLink>(entity))
            {
                chunkRenderer = World.EntityManager.GetSharedComponentData<ChunkMeshLink>(entity);
			    mesh = chunkRenderer.mesh;
            }
            if (mesh == null)
			{
				mesh = new Mesh();
                mesh.MarkDynamic();
			}
			else
			{
				mesh.Clear();
			}
            Chunk chunkComponent = World.EntityManager.GetComponentData<Chunk>(chunk.chunk);
            World worldComponent = World.EntityManager.GetComponentData<World>(chunkComponent.world);
            chunk.SetMeshData(mesh, worldComponent.modelID != 0);
            if (mesh.vertexCount != mesh.colors.Length || mesh.vertexCount != mesh.uv.Length)
			{
				Debug.LogError("ChunkMesh has inconsistent data: " + mesh.vertexCount + ":" + mesh.uv.Length + ":" + mesh.colors.Length);
            }
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
			//mesh.UploadMeshData(false);
            
            if (World.EntityManager.HasComponent<RenderMesh>(entity))
            {
                renderer.mesh = mesh;
                //renderer.subMesh = 0;
                World.EntityManager.SetSharedComponentData(entity, renderer);
            }
            else if (World.EntityManager.HasComponent<ChunkMeshLink>(entity))
            {
                chunkRenderer.mesh = mesh;
                World.EntityManager.SetSharedComponentData(entity, chunkRenderer);
            }
            if (World.EntityManager.HasComponent<RenderBounds>(entity))
            {
                RenderBounds renderBounds2 = World.EntityManager.GetComponentData<RenderBounds>(entity);
                renderBounds2.Value.Extents = mesh.bounds.extents;
                renderBounds2.Value.Center = new float3(mesh.bounds.center.x, mesh.bounds.center.y, mesh.bounds.center.z);
                World.EntityManager.SetComponentData(entity, renderBounds2);
            }
        }*/


        
            //int count = 0;
            /*Entities.WithAll<ChunkRendererBuilder, ChunkRenderer>().ForEach((Entity e, ref ChunkRendererBuilder chunkRendererBuilder, ref ChunkRenderer renderer) =>
            {
                if ((chunkRendererBuilder.state == 3 && renderer.hasWeights == 0)
                || (chunkRendererBuilder.state == 4 && renderer.hasWeights == 1))
                {
                    World.EntityManager.RemoveComponent<ChunkRendererBuilder>(e);
                }
            });*/
            /*if ((chunkRendererBuilder.state == 3 && renderer.hasWeights == 0)
            || (chunkRendererBuilder.state == 4 && renderer.hasWeights == 1))
            {
                if (ChunkSpawnSystem.isDebugLog)
                {
                    Debug.LogError("Finished building mesh for chunk at: " + renderer.Value.chunkPosition + " has weights: " + renderer.hasWeights);
                }
                UpdateMesh(e, ref renderer);
                // remove chunk renderer if not skeleton
                if (renderer.hasWeights == 0)
                {
                    //World.EntityManager.RemoveComponent<ChunkRenderer>(e);
                }
                // remove thingo
                World.EntityManager.RemoveComponent<ChunkRendererBuilder>(e);
            } */