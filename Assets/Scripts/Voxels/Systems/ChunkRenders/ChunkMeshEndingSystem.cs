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
        public WorldSpawnSystem worldSpawnSystem;
        public ChunkSpawnSystem chunkSpawnSystem;

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
                    FinishChunkRender(e, ref renderer);
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

		public void FinishChunkRender(Entity entity, ref ChunkRenderer chunk)
        {
            ZoxID rendererID = World.EntityManager.GetComponentData<ZoxID>(entity);
            RenderMesh renderer = World.EntityManager.GetSharedComponentData<RenderMesh>(entity);
			Mesh mesh = renderer.mesh;
            if (mesh == null)
			{
				mesh = new Mesh();
			}
			else
			{
				mesh.Clear();
			}

            chunk.SetMeshData(mesh);
            
            if (chunk.hasWeights == 1)
            {
                if (Bootstrap.DebugMeshWeights)
                {
                    Debug.LogError("Debugging Weights on Model / chunk mesh.");
                    mesh.colors = chunk.GetWeightsAsColors();
                }
                mesh.bindposes = chunk.GetBonePoses();
                mesh.boneWeights = chunk.GetWeights();
                mesh.RecalculateBounds();
                // centre mesh
                mesh.vertices = mesh.vertices;
                mesh.RecalculateTangents();
            }
            else {
			    mesh.RecalculateBounds();
            }
            mesh.RecalculateNormals();
            if (mesh.vertexCount != mesh.colors.Length || mesh.vertexCount != mesh.uv.Length)
			{
				Debug.LogError("ChunkMesh has inconsistent data: " + mesh.vertexCount + ":" + mesh.uv.Length + ":" + mesh.colors.Length);
            }

            Chunk chunkComponent = World.EntityManager.GetComponentData<Chunk>(chunkSpawnSystem.chunks[rendererID.creatorID]);
            World worldComponent = World.EntityManager.GetComponentData<World>(worldSpawnSystem.worlds[chunkComponent.worldID]);
            if (worldComponent.modelID != 0)
            {
                CentreMesh(mesh);
                RotateMesh(mesh);
            }

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
            //Bootstrap.instance.DebugMesh = renderer.mesh;
        }
        public void CentreMesh(Mesh mesh)
        {
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
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
        }

        public void RotateMesh(Mesh mesh)
        {
            Vector3[] verts = mesh.vertices;
            for (int i = 0; i < verts.Length; i++)
            {
                //verts[i].z = -verts[i].z;
                verts[i] = math.rotate(Quaternion.Euler(180, 0, 180), verts[i]);
            }
            mesh.vertices = verts;
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
        }
    }
}