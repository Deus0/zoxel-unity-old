using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Rendering;
using UnityEngine;
using System.Collections.Generic;

namespace Zoxel
{
    /// <summary>
    /// Todo
    ///     Make flash white when gotten attacked for .3 seconds
    /// </summary>
    [DisableAutoCreation]
    public class StatbarSystem : ComponentSystem
    {
        //public UIUtilities UIUtilities;
        // references
        public UIDatam uiData;
        // ecs
        private EntityArchetype frontbarArchtype;
        private EntityArchetype backbarArchtype;
        public static Dictionary<int, Entity> frontBars = new Dictionary<int, Entity>();
        public static Dictionary<int, Entity> backBars = new Dictionary<int, Entity>();

        // queue
        //private List<Entity> commandEntities = new List<Entity>();
        //private List<Entity> commandPlayerEntities = new List<Entity>();

        public static float healthBackBuffer = 0.0f; // .01
        public static float healthbarDepth = 0.0f;  // -.01
        public float3 orbitPosition = new float3(0, 0.08f, 0.25f);
        public float2 panelSize = new float2(0.2f, 0.04f);

        public float2 panelSizeNPC = new float2(0.3f, 0.08f);
        private Mesh healthbarMesh;
        private Mesh healthbarMeshNPC;
        private Mesh healthbarMeshBackNPC;

        protected override void OnCreate()
        {
            orbitPosition = UIUtilities.GetOrbitAnchors(UIAnchoredPosition.TopMiddle, orbitPosition, panelSize);
            orbitPosition.y += 0.01f;
            healthbarMesh = CreateQuadMesh(panelSize, true);
            healthbarMeshNPC = CreateQuadMesh(panelSizeNPC, true);
            healthbarMeshBackNPC = CreateQuadMesh(panelSizeNPC, false);
            backbarArchtype = World.EntityManager.CreateArchetype(
                typeof(ZoxID),
                typeof(FaceCameraComponent),
                typeof(UITrailer),
                // transform
                typeof(Translation),
                typeof(Rotation),
                typeof(NonUniformScale),
                // renderer
                typeof(RenderMesh),
                typeof(LocalToWorld)
            );
            frontbarArchtype = World.EntityManager.CreateArchetype(
                // unique
                typeof(StatBarUI),
                typeof(ZoxID),
                // transforms
                typeof(Parent),
                typeof(LocalToParent),
                typeof(LocalToWorld),
                // transform
                typeof(Translation),
                typeof(Rotation),
                typeof(NonUniformScale),
                // renderer
                typeof(RenderMesh)
            );
        }

        public void Clear()
        {
            foreach (Entity e in frontBars.Values)
            {
                if (World.EntityManager.Exists(e))
                {
                    World.EntityManager.DestroyEntity(e);
                }
            }
            foreach (Entity e in backBars.Values)
            {
                if (World.EntityManager.Exists(e))
                {
                    World.EntityManager.DestroyEntity(e);
                }
            }
            frontBars.Clear();
            backBars.Clear();
        }

        public void SpawnNPCBar(Entity statsEntity)
        {
            ZoxID zoxID = World.EntityManager.GetComponentData<ZoxID>(statsEntity);
            if (frontBars.ContainsKey(zoxID.id))
            {
                Debug.LogError("Trying to add duplicate statbar onto character.");
                return;
            }
            Translation translation = World.EntityManager.GetComponentData<Translation>(statsEntity);
            Body body = World.EntityManager.GetComponentData<Body>(statsEntity);
            float positionOffset = body.size.y * 1.2f + panelSize.y * 2f;
            float3 spawnPosition = translation.Value + new float3(0, positionOffset, 0);
            Entity backbar = CreateBarUI(backbarArchtype, spawnPosition, uiData.backBarMaterial, healthbarMeshBackNPC);
            Entity frontbar = CreateBarUI(frontbarArchtype, spawnPosition, uiData.frontBarMaterial, healthbarMeshNPC);
            frontBars.Add(zoxID.id, frontbar);
            backBars.Add(zoxID.id, backbar);
            // backbar
            World.EntityManager.SetComponentData(backbar, zoxID);
            World.EntityManager.SetComponentData(backbar, new UITrailer { heightAddition = positionOffset });
            // front bar
            World.EntityManager.SetComponentData(frontbar, zoxID);
            World.EntityManager.SetComponentData(frontbar, new Parent { Value = backbar });
            World.EntityManager.SetComponentData(frontbar, new StatBarUI { percentage = 1, targetPercentage = 1, width = panelSizeNPC.x });
        }

        private Entity CreateBarUI(EntityArchetype archtype, Vector3 position, Material materialA, Mesh mesh)
        {
            Entity newBar = World.EntityManager.CreateEntity(archtype);
            World.EntityManager.SetSharedComponentData(newBar, new RenderMesh { material = new Material(materialA), mesh = mesh });
            World.EntityManager.SetComponentData(newBar, new Translation { Value = position });
            World.EntityManager.SetComponentData(newBar, new Rotation { Value = quaternion.identity });
            World.EntityManager.SetComponentData(newBar, new NonUniformScale { Value = new float3(1f, 1f, 1f) });
            return newBar;
        }

        private Mesh CreateQuadMesh(float2 panelSize, bool isLeftAligned)
        {
            Mesh mesh = new Mesh();
            Vector3[] newVerts = new Vector3[4];
            if (isLeftAligned)
            {
                newVerts[0] = new Vector3(0, 0.5f * panelSize.y, healthbarDepth);
                newVerts[1] = new Vector3(panelSize.x, 0.5f * panelSize.y, healthbarDepth);
                newVerts[2] = new Vector3(panelSize.x, -0.5f * panelSize.y, healthbarDepth);
                newVerts[3] = new Vector3(0, -0.5f * panelSize.y, healthbarDepth);
            }
            else
            {
                // Back Bar
                newVerts[0] = new Vector3(-0.5f * panelSize.x - healthBackBuffer, 0.5f * panelSize.y + healthBackBuffer, 0);
                newVerts[1] = new Vector3(0.5f * panelSize.x + healthBackBuffer, 0.5f * panelSize.y + healthBackBuffer, 0);
                newVerts[2] = new Vector3(0.5f * panelSize.x + healthBackBuffer, -0.5f * panelSize.y - healthBackBuffer, 0);
                newVerts[3] = new Vector3(-0.5f * panelSize.x - healthBackBuffer, -0.5f * panelSize.y - healthBackBuffer, 0);
            }
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

        #region Spawning-Despawning
        private struct SpawnStatbar : IComponentData
        {
            public Entity character;
            public byte isPlayer;
        }

        private struct RemoveStatbar : IComponentData
        {
            public Entity character;
        }


        public static void SpawnPlayerStatbar(EntityManager EntityManager, Entity entity)
        {
            Entity e = EntityManager.CreateEntity();
            EntityManager.AddComponentData(e, new SpawnStatbar
            {
                character = entity,
                isPlayer = 1
            });
        }

        public void QueueBar(Entity entity)
        {
            Entity e = World.EntityManager.CreateEntity();
            World.EntityManager.AddComponentData(e, new SpawnStatbar
            {
                character = entity
            });
        }

        protected override void OnUpdate()
        {
            Entities.WithAll<SpawnStatbar>().ForEach((Entity e, ref SpawnStatbar command) =>
            {
                /*if (command.isPlayer == 1)
                {
                    SpawnPlayerBar(command.character);
                }
                else
                {
                    SpawnNPCBar(command.character);
                }*/
                SpawnNPCBar(command.character);
                World.EntityManager.DestroyEntity(e);
            });
            Entities.WithAll<RemoveStatbar>().ForEach((Entity e, ref RemoveStatbar command) =>
            {
                //RemoveUI(command.character);
                World.EntityManager.DestroyEntity(e);
            });
        }
        #endregion

        private void SpawnPlayerBar(Entity character)//, float heightAddition)
        {
            ZoxID zoxID = World.EntityManager.GetComponentData<ZoxID>(character);
            if (frontBars.ContainsKey(zoxID.id))
            {
                Debug.LogError("Trying to add duplicate statbar onto character.");
                return;
            }
            Translation translation = World.EntityManager.GetComponentData<Translation>(character);
            Entity backbar = UIUtilities.SpawnPanel(World.EntityManager,character, uiData.backBarMaterial, null, panelSize);
            Entity frontbar = CreateBarUI(frontbarArchtype, translation.Value, uiData.frontBarMaterial, healthbarMesh);
            frontBars.Add(zoxID.id, frontbar);
            backBars.Add(zoxID.id, backbar);
            // backbar
            World.EntityManager.AddComponentData(backbar, zoxID);
            World.EntityManager.SetComponentData(frontbar, new Parent { Value = backbar });
            World.EntityManager.SetComponentData(frontbar, new StatBarUI { width = panelSize.x });
            World.EntityManager.SetComponentData(frontbar, zoxID);
            UIUtilities.UpdateOrbiter(World.EntityManager, backbar, orbitPosition, uiData.crosshairLerpSpeed);
        }
    }
}
