using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Zoxel;
using Unity.Transforms;
using Unity.Rendering;

namespace Zoxel.Voxels
{
    [DisableAutoCreation]
    public class VoxelPreviewSystem : ComponentSystem
    {
        // create a entity
        public EntityArchetype archtype;
        public Mesh quadMesh;
        public Material quadMaterial;
        private Entity latestEntity;

        protected override void OnCreate()
        {
            base.OnCreate();
            archtype = World.EntityManager.CreateArchetype(
                typeof(Translation),
                typeof(Rotation),
                typeof(Scale),
                // renderer
                typeof(RenderMesh),
                typeof(LocalToWorld)
            );
            quadMesh = CreateQuadMesh();
            quadMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        }

        public void Test()
        {
            quadMaterial = Bootstrap.instance.data.quadMaterial;// new Material(Shader.Find("Universal Render Pipeline/Lit"));
            CreateVoxelPreviewEntity();
        }

        public void CreateVoxelPreviewEntity()
        {
            latestEntity = World.EntityManager.CreateEntity(archtype);
            World.EntityManager.SetComponentData(latestEntity, new Translation { Value = float3.zero });
            World.EntityManager.SetComponentData(latestEntity, new Rotation
            {
                Value = UnityEngine.Quaternion.Euler(new float3(90, 0, 0))
            });//quaternion.identity
            World.EntityManager.SetComponentData(latestEntity, new Scale { Value = 1 });
            World.EntityManager.SetSharedComponentData(latestEntity,
                new RenderMesh { mesh = quadMesh, material = quadMaterial });
        }

        public void SetPosition(float3 newPosition)
        {
            if (World.EntityManager.Exists(latestEntity))
            {
                World.EntityManager.SetComponentData(latestEntity, new Translation { Value = newPosition });
            }
        }

        private Mesh CreateQuadMesh()
        {
            float healthBarHeight = 1;
            //float healthbarDepth = 0;
            float healthBarWidth = 1;
            float healthBackBuffer = 0;
            Mesh mesh = new Mesh();
            Vector3[] newVerts = new Vector3[4];
                newVerts[0] = new Vector3(-0.5f * healthBarWidth - healthBackBuffer, 0.5f * healthBarHeight + healthBackBuffer, 0);
                newVerts[1] = new Vector3(0.5f * healthBarWidth + healthBackBuffer, 0.5f * healthBarHeight + healthBackBuffer, 0);
                newVerts[2] = new Vector3(0.5f * healthBarWidth + healthBackBuffer, -0.5f * healthBarHeight - healthBackBuffer, 0);
                newVerts[3] = new Vector3(-0.5f * healthBarWidth - healthBackBuffer, -0.5f * healthBarHeight - healthBackBuffer, 0);
            
            mesh.vertices = newVerts;
            int[] indices = new int[6];
            indices[0] = 0;
            indices[1] = 1;
            indices[2] = 2;
            indices[3] = 0;
            indices[4] = 2;
            indices[5] = 3;
            mesh.SetIndices(indices, MeshTopology.Triangles, 0);
            Vector2[] uvs = new Vector2[4];
            uvs[0] = new Vector2(0, 0);
            uvs[1] = new Vector2(1, 0);
            uvs[2] = new Vector2(1, 1);
            uvs[3] = new Vector2(0, 1);
            mesh.uv = uvs;
            mesh.colors = new Color[] { Color.white, Color.white, Color.white, Color.white };
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            return mesh;
        }

        protected override void OnUpdate()
        {

        }
    }
}

