using Unity.Entities;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;

namespace Zoxel
{
    [DisableAutoCreation]
    public class EquipmentUISpawnSystem : PlayerUISpawnSystem
    {
        public Dictionary<int, ItemDatam> meta = new Dictionary<int, ItemDatam>();

        #region Spawning-Removing
        public struct SpawnEquipmentUI : IComponentData
        {
            public Entity character;
        }
        public struct RemoveEquipmentUI : IComponentData
        {
            public Entity character;
        }

        public static void SpawnUI(EntityManager EntityManager, Entity character)
        {
            Entity e = EntityManager.CreateEntity();
            EntityManager.AddComponentData(e, new SpawnEquipmentUI { character = character });
        }

        public static void RemoveUI(EntityManager EntityManager, Entity character)
        {
            Entity e = EntityManager.CreateEntity();
            EntityManager.AddComponentData(e, new RemoveEquipmentUI { character = character });
        }

        protected override void OnUpdate()
        {
            Entities.WithAll<SpawnEquipmentUI>().ForEach((Entity e, ref SpawnEquipmentUI command) =>
            {
                SpawnUI(command.character, command);
                World.EntityManager.DestroyEntity(e);
            });
            Entities.WithAll<RemoveEquipmentUI>().ForEach((Entity e, ref RemoveEquipmentUI command) =>
            {
                RemoveUI(command.character);
                World.EntityManager.DestroyEntity(e);
            });
        }

        protected override void OnSpawnedPanel(Entity character, Entity panelUI, object spawnData)
        {
            int zoxID = World.EntityManager.GetComponentData<ZoxID>(character).id;
            Equipment inventory = World.EntityManager.GetComponentData<Equipment>(character);
            List<Entity> icons = new List<Entity>();
            float2 iconSize = uiDatam.defaultIconSize;
            for (int i = 0; i < inventory.body.Length; i++)
            {
                var piece = inventory.body[i];
                Texture2D iconTexture = null;
                if (piece.metaID != 0)
                {
                    ItemDatam itemDatam = meta[piece.metaID];
                    if (itemDatam != null && itemDatam.texture)
                    {
                        iconTexture = itemDatam.texture.texture;
                    }
                }
                Entity icon = UIUtilities.SpawnVisualElement(
                    World.EntityManager,
                    panelUI,
                    float3.zero,
                    iconSize,
                    iconTexture,
                    uiDatam.defaultPlayerIcon);
                icons.Add(icon);
            }
            for (int i = 0; i < inventory.gear.Length; i++)
            {
                var piece = inventory.gear[i];
                Texture2D iconTexture = null;
                if (piece.metaID != 0)
                {
                    ItemDatam itemDatam = meta[piece.metaID];
                    if (itemDatam != null && itemDatam.texture)
                    {
                        iconTexture = itemDatam.texture.texture;
                    }
                }
                float3 position = float3.zero; // GetGridPosition(i, 3, 3);
                Entity icon = UIUtilities.SpawnVisualElement(
                    World.EntityManager,
                    panelUI,
                    position,
                    iconSize,
                    iconTexture,
                    uiDatam.defaultPlayerIcon);
                Childrens textLink = new Childrens { children = new BlitableArray<Entity>(1, Unity.Collections.Allocator.Persistent) };
                textLink.children[0] = UIUtilities.SpawnText(World.EntityManager, icon, ((int)piece.durability).ToString()); //, iconSize);
                World.EntityManager.AddComponentData(icon, textLink);
                icons.Add(icon);
            }
            Childrens children = new Childrens { };
            children.children = new BlitableArray<Entity>(icons.Count, Unity.Collections.Allocator.Persistent);
            for (int i = 0; i < icons.Count; i++)
            {
                children.children[i] = icons[i];
            }
            World.EntityManager.AddComponentData(panelUI, children);
            World.EntityManager.AddComponentData(panelUI, new GridUI
            {
                updated = 1,
                gridSize = uiDatam.inventoryGridSize,
                iconSize = iconSize,
                margins = new float2(0.003f, 0.003f),
                padding = new float2(0.003f, 0.003f),
            });
            byte uiIndex = ((byte)((int)PlayerUIType.InventoryUI));
            World.EntityManager.SetComponentData(panelUI, new PanelUI
            {
                id = uiIndex,
                characterID = zoxID,
                orbitDepth = uiDatam.orbitDepth,
                anchor = (byte)UIAnchoredPosition.Middle
            });
            OnSelectedButton(zoxID, 0);
        }
        #endregion
    }
}
