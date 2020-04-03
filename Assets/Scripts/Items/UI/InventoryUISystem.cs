using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using Zoxel.UI;

namespace Zoxel
{
    public enum UIAnchoredPosition
    {
        BottomLeft,
        BottomMiddle,
        BottomRight,
        Left,
        Middle,
        Right,
        TopLeft,
        TopMiddle,
        TopRight
    }

    [DisableAutoCreation]
    public class InventoryUISpawnSystem : PlayerUISpawnSystem
    {
        public Dictionary<int, ItemDatam> meta = new Dictionary<int, ItemDatam>();

        #region Spawning-Removing
        public struct SpawnInventoryUI : IComponentData
        {
            public Entity character;
        }
        public struct RemoveInventoryUI : IComponentData
        {
            public Entity character;
        }
        public static void SpawnUI(EntityManager EntityManager, Entity character)
        {
            Entity e = EntityManager.CreateEntity();
            EntityManager.AddComponentData(e, new SpawnInventoryUI { character = character });
        }
        public static void RemoveUI(EntityManager EntityManager, Entity character)
        {
            Entity e = EntityManager.CreateEntity();
            EntityManager.AddComponentData(e, new RemoveInventoryUI { character = character });
        }
        protected override void OnUpdate()
        {
            base.OnUpdate();
            Entities.WithAll<SpawnInventoryUI>().ForEach((Entity e, ref SpawnInventoryUI command) =>
            {
                SpawnUI(command.character, command);//, panelSize);
                World.EntityManager.DestroyEntity(e);
            });
            Entities.WithAll<RemoveInventoryUI>().ForEach((Entity e, ref RemoveInventoryUI command) =>
            {
                RemoveUI(command.character);
                World.EntityManager.DestroyEntity(e);
            });
        }

        protected override void OnSpawnedPanel(Entity character, Entity panelUI, object spawnData)
        {
            if (World.EntityManager.HasComponent<Inventory>(character) == false)
            {
                Debug.LogError("Cannot spawn Inventory UI as character does not have inventory.");
                return;
            }
            int zoxID = World.EntityManager.GetComponentData<ZoxID>(character).id;
            Inventory inventory = World.EntityManager.GetComponentData<Inventory>(character);
            List<Entity> buttons = new List<Entity>();
            float2 iconSize = uiDatam.defaultIconSize;
            for (int i = 0; i < inventory.items.Length; i++)
            {
                Texture2D iconTexture = null;
                if (inventory.items[i].data.id  != 0 && meta.ContainsKey(inventory.items[i].data.id ))
                {
                    ItemDatam itemDatam = meta[inventory.items[i].data.id ];
                    if (itemDatam != null && itemDatam.texture)
                    {
                        iconTexture = itemDatam.texture.texture;
                    }
                }
                Entity button = UIUtilities.SpawnButton(
                    World.EntityManager,
                    panelUI,
                    float3.zero,
                    iconSize,
                    iconTexture,
                    uiDatam.defaultPlayerIcon);
                World.EntityManager.AddComponentData(button, new ItemUI { index = i });
                buttons.Add(button);
                Childrens textLink = new Childrens { children = new BlitableArray<Entity>(1, Unity.Collections.Allocator.Persistent) };
                string numberString = ((int)inventory.items[i].quantity).ToString();
                if (numberString == "0" || numberString == "1")
                {
                    numberString = "";
                }
                textLink.children[0] = UIUtilities.SpawnText(World.EntityManager, button, numberString, float3.zero, uiDatam.overlayTextColor);
                World.EntityManager.AddComponentData(button, textLink);
            }

            Childrens children = new Childrens { };
            children.children = new BlitableArray<Entity>(buttons.Count, Unity.Collections.Allocator.Persistent);
            for (int i = 0; i < buttons.Count; i++)
            {
                children.children[i] = buttons[i];
            }
            World.EntityManager.AddComponentData(panelUI, children);
            
            World.EntityManager.AddComponentData(panelUI, new GridUI
            {
                dirty = 1,
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


//bool isFirst = true;
//int firstSelected;

/*public override void OnSelectedButton(int characterID, int arrayIndex)
{
    if (!characterSpawnSystem.characters.ContainsKey(characterID))
    {
        return;
    }
    Entity character = characterSpawnSystem.characters[characterID];
    Inventory inventory = World.EntityManager.GetComponentData<Inventory>(character);
    if (arrayIndex < inventory.items.Length)
    {
        InventoryItem item = inventory.items[arrayIndex];
        if (item.metaID != 0)
        {
            ItemDatam metaItem = meta[item.metaID];
            //Debug.LogError("Selecting item: " + metaItem.name);
            if (item.quantity != 1)
            {
                //SetTooltipText(characterID, metaItem.name + " x" + item.quantity);
            }
            else
            {
                //SetTooltipText(characterID, metaItem.name);
            }
        }
        else
        {
            //SetTooltipText(characterID, "");
        }
    }
}*/
