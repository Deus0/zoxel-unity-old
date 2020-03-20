using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Zoxel.Voxels;

namespace Zoxel
{

    [DisableAutoCreation]
    public class MapUISpawnSystem : PlayerUISpawnSystem
    {
        public ChunkMapCompleterSystem chunkMapSystem;
        //private Dictionary<int, EntityBunch> mapIcons = new Dictionary<int, EntityBunch>();
        /*protected override float2 GetElementMargins() { return float2.zero; }
        protected override float2 GetIconSize()
        {
            return new float2(uiDatam.skillbarIconSize, uiDatam.skillbarIconSize) * (6 / ((float)Bootstrap.mapResolution));
        }
        */
        protected override void OnSpawnedPanel(Entity character, Entity panelUI, object spawnData)
        {
            var position = World.EntityManager.GetComponentData<ChunkStreamPoint>(character).chunkPosition;
            //EntityBunch icons = new EntityBunch();
            List<Entity> icons2 = new List<Entity>();
            int mapResolution = 4;
            if (Bootstrap.instance) {
                mapResolution = Bootstrap.instance.mapResolution;
            }
            int rowsCount = mapResolution;
            int columnsCount = mapResolution;
            float2 iconSize = uiDatam.defaultIconSize;
            //for (int j = -(columnsCount / 2); j < (columnsCount / 2); j++)
            for (int j = (columnsCount / 2)  -1; j >= -(columnsCount / 2); j--)
            {
                for (int i = -(rowsCount / 2); i < (rowsCount / 2); i++)
                {
                    float2 mapPosition = new float2(i + position.x, j + position.z);
                    if (chunkMapSystem.maps.ContainsKey(mapPosition))
                    {
                        float3 localMapPosition = float3.zero;// GetGridPosition(i + rowsCount / 2, -(j - (columnsCount / 2) + 1), rowsCount, columnsCount); // + columnsCount / 2
                        icons2.Add(UIUtilities.SpawnVisualElement(
                            World.EntityManager, 
                            panelUI, localMapPosition,
                            iconSize,
                            chunkMapSystem.maps[mapPosition], uiDatam.mapIcon));
                    }
                    else
                    {
                        //Debug.LogError("Could not find map for positoin: " + mapPosition.ToString());
                    }
                }
            }
            Childrens children = new Childrens { };
            children.children = new BlitableArray<Entity>(icons2.Count, Unity.Collections.Allocator.Persistent);
            for (int i = 0; i < icons2.Count; i++)
            {
                children.children[i] = icons2[i]; // icons2.Count - 1 - 
            }
            World.EntityManager.AddComponentData(panelUI, children);
            World.EntityManager.AddComponentData(panelUI, new GridUI
            {
                updated = 1,
                gridSize = new float2(mapResolution, mapResolution),
                iconSize = iconSize,
                margins = new float2(0.003f, 0.003f),
                padding = new float2(0.00f, 0.00f),
            });
            byte uiIndex = ((byte)((int)PlayerUIType.SkillbookUI));
            ZoxID characterID = World.EntityManager.GetComponentData<ZoxID>(character);
            World.EntityManager.SetComponentData(panelUI, new PanelUI
            {
                id = uiIndex,
                characterID = characterID.id,
                orbitDepth = uiDatam.orbitDepth,
                anchor = (byte)UIAnchoredPosition.Middle
            });

            /*icons.entities = icons2.ToArray();;
            if (mapIcons.ContainsKey(characterID.id))
            {
                mapIcons.Remove(characterID.id);
               // Debug.LogError("Map Icons in there twice for: " + characterID.id);
            }
            mapIcons.Add(characterID.id, icons);*/
        }

        /*protected override void OnRemoveUI(int id) 
        {
            // rmeove icons
            if (mapIcons.ContainsKey(id) == false)
            {
                Debug.LogError("Problem synching map icons in (MapUISpawnSystem:OnRemoveUI): " + id);
                return;
            }
            EntityBunch bunch = mapIcons[id];
            for (int i = 0; i < bunch.entities.Length; i++)
            {
                if (World.EntityManager.Exists(bunch.entities[i]))
                {
                    World.EntityManager.DestroyEntity(bunch.entities[i]);
                }
            }
            mapIcons.Remove(id);
        }
        */
       /* private float3 GetIconPosition(float2 mapPosition)
        {
            return new float3(
                mapPosition.x * (iconSize + iconInnerMargin) + (iconSize / 2f),//+ (iconSize / 2f), //  - (panelSize.x / 2f)
                //-panelOuterMargin.y - mapPosition.y * (iconSize + iconInnerMargin),// - (iconSize / 2f), //  + (panelSize.y / 2f)
                mapPosition.y * (iconSize + iconInnerMargin) + (iconSize / 2f),
                iconDepth);
        }*/

        #region Spawning-Removing
        public struct SpawnMapUI : IComponentData
        {
            public Entity character;
        }

        public struct RemoveMapUI : IComponentData
        {
            public Entity character;
        }

        public static void SpawnUI(EntityManager EntityManager, Entity character)
        {
            Entity e = EntityManager.CreateEntity();
            EntityManager.AddComponentData(e, new SpawnMapUI { character = character });
        }

        public static void RemoveUI(EntityManager EntityManager, Entity character)
        {
            Entity e = EntityManager.CreateEntity();
            EntityManager.AddComponentData(e, new RemoveMapUI { character = character });
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
            Entities.WithAll<SpawnMapUI>().ForEach((Entity e, ref SpawnMapUI command) =>
            {
                //float2 panelSize = GetGridPanelSize(Bootstrap.mapResolution, Bootstrap.mapResolution);
                SpawnUI(command.character, command);//, panelSize);
                World.EntityManager.DestroyEntity(e);
            });
            Entities.WithAll<RemoveMapUI>().ForEach((Entity e, ref RemoveMapUI command) =>
            {
                RemoveUI(command.character);
                World.EntityManager.DestroyEntity(e);
            });
        }
        #endregion
    }
}
