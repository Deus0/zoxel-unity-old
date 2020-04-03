using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Burst;
using Unity.Rendering;
using System.Linq;

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
            //int count = 0;
            Entities.WithAll<ChunkRendererBuilder, ChunkRenderer>().ForEach((Entity e, ref ChunkRendererBuilder chunkRendererBuilder, ref ChunkRenderer renderer) => // , ref RenderMesh renderer
            {
                if ((chunkRendererBuilder.state == 3 && renderer.hasWeights == 0)
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
                }
            });
		}

		void UpdateMesh(Entity entity, ref ChunkRenderer chunk)
        {
            RenderMesh renderer = World.EntityManager.GetSharedComponentData<RenderMesh>(entity);
			Mesh mesh = renderer.mesh;
            if (mesh == null)
			{
				mesh = new Mesh();
                mesh.MarkDynamic();
			}
			else
			{
				mesh.Clear();
			}
            chunk.SetMeshData(mesh);
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
            Chunk chunkComponent = World.EntityManager.GetComponentData<Chunk>(chunk.chunk);
            World worldComponent = World.EntityManager.GetComponentData<World>(chunkComponent.world);
            if (worldComponent.modelID != 0)
            {
                //ChunkMeshEndingSystem.CentreMesh(mesh);
                //ChunkMeshEndingSystem.RotateMesh(mesh);
            }
            if (mesh.vertexCount != mesh.colors.Length || mesh.vertexCount != mesh.uv.Length)
			{
				Debug.LogError("ChunkMesh has inconsistent data: " + mesh.vertexCount + ":" + mesh.uv.Length + ":" + mesh.colors.Length);
            }
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
			//mesh.UploadMeshData(false);
            renderer.mesh = mesh;
			renderer.subMesh = 0;
            World.EntityManager.SetSharedComponentData(entity, renderer);
            if (World.EntityManager.HasComponent<RenderBounds>(entity))
            {
                RenderBounds renderBounds2 = World.EntityManager.GetComponentData<RenderBounds>(entity);
                renderBounds2.Value.Extents = mesh.bounds.extents;
                renderBounds2.Value.Center = new float3(mesh.bounds.center.x, mesh.bounds.center.y, mesh.bounds.center.z);
                World.EntityManager.SetComponentData(entity, renderBounds2);
            }
        }

        public static void CentreMesh(Mesh mesh)
        {
            mesh.RecalculateBounds();
            /*mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();*/
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
        }
    }
}
            /*mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();*/
            /*mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();*/