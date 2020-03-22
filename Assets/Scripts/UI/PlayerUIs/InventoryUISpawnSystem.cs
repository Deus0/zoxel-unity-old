using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

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
        bool isFirst = true;
        int firstSelected;

        public override void OnClickedButton(Entity player, Entity ui, int arrayIndex, ButtonType buttonType)
        {
            if (buttonType != ButtonType.ButtonA && buttonType != ButtonType.ButtonX)
            {
                return;
            }
            if (World.EntityManager.HasComponent<Inventory>(player) == false)
            {
                Debug.LogError("Character " + player.Index + " does not have inventory component.");
                return;
            }
            Inventory inventory = World.EntityManager.GetComponentData<Inventory>(player);
            InventoryItem item2 = inventory.items[arrayIndex];
            // button x is equip
            if (buttonType == ButtonType.ButtonX)
            {
                if (item2.metaID != 0)
                {
                    if (World.EntityManager.HasComponent<Equipment>(player) == false)
                    {
                        Debug.LogError("Character " + player.Index + " does not have equipment component.");
                        return;
                    }
                    int characterID = World.EntityManager.GetComponentData<ZoxID>(player).id;
                    var equipment = World.EntityManager.GetComponentData<Equipment>(player);
                    var bodyPart = equipment.body[0];
                    //Debug.LogError("Equiping item: " + item2.metaID + " over item " + bodyPart.metaID);
                    int bodyID = bodyPart.metaID;
                    bodyPart.metaID = item2.metaID;
                    equipment.body[0] = bodyPart;
                    equipment.dirty = 1;
                    World.EntityManager.SetComponentData(player, equipment);
                    item2.metaID = bodyID;
                    inventory.items[arrayIndex] = item2;
                    World.EntityManager.SetComponentData(player, inventory);
                    RefreshUI(characterID, arrayIndex);
                    UpdateIconText(characterID, arrayIndex);
                }
                return;
            }
            else
            {
                // button a is item pickup / swap
                /*if (isFirst)
                {
                    if (item2.metaID != 0)
                    {
                        isFirst = !isFirst;
                        firstSelected = arrayIndex;
                        // change selected icon
                        ///ChangeSelectedIcon(characterID, uiDatam.selectedActioning);
                    }
                }
                else
                {
                    isFirst = !isFirst;
                    // switch items
                    InventoryItem item1 = inventory.items[firstSelected];
                    inventory.items[firstSelected] = item2;
                    inventory.items[arrayIndex] = item1;

                    World.EntityManager.SetComponentData(player, inventory);
                    // update those items/quantities and tooltip
                    int characterID = World.EntityManager.GetComponentData<ZoxID>(player).id;
                    RefreshUI(characterID, firstSelected);
                    UpdateIconText(characterID, firstSelected);
                    RefreshUI(characterID, arrayIndex);
                    UpdateIconText(characterID, arrayIndex);
                    OnSelectedButton(characterID, arrayIndex);
                    //ChangeSelectedIcon(characterID, uiDatam.selectedDefault);
                }*/
            }
        }

        private void RefreshUI(int characterID, int arrayIndex)
        {
            if (characterSpawnSystem.characters.ContainsKey(characterID) == false)
            {
                Debug.LogError("ID not contained in characters: " + characterID);
                return;
            }
            if (uis.ContainsKey(characterID) == false)
            {
                Debug.LogError("ID not contained in uis: " + characterID);
                return;
            }
            Entity character = characterSpawnSystem.characters[characterID];
            Inventory inventory = World.EntityManager.GetComponentData<Inventory>(character);
            Texture2D iconTexture = null;
            int itemMetaID = inventory.items[arrayIndex].metaID;
            if (itemMetaID != 0)
            {
                ItemDatam itemDatam = meta[itemMetaID];
                if (itemDatam != null && itemDatam.texture)
                {
                    iconTexture = itemDatam.texture.texture;
                }
            }
            else
            {
                iconTexture = uiDatam.defaultItemIcon;
            }
            Childrens childrens = World.EntityManager.GetComponentData<Childrens>(uis[characterID]);
            Entity iconEntity = childrens.children[arrayIndex]; //icons[characterID].entities[arrayIndex];
            var renderer = World.EntityManager.GetSharedComponentData<Unity.Rendering.RenderMesh>(iconEntity);
            renderer.material.SetTexture("_BaseMap", iconTexture);
            //World.EntityManager.SetSharedComponentData(iconEntity, renderer);
        }

        public void UpdateIconText(int characterID, int arrayIndex)
        {
            if (uis.ContainsKey(characterID) && arrayIndex != -1)
            {
                Entity character = characterSpawnSystem.characters[characterID];
                Inventory inventory = World.EntityManager.GetComponentData<Inventory>(character);
                string numberString = inventory.items[arrayIndex].quantity.ToString();
                if (numberString == "0" || numberString == "1")
                {
                    numberString = "";
                }
                Childrens childrens = World.EntityManager.GetComponentData<Childrens>(uis[characterID]);

                Entity textGrandChild = World.EntityManager.GetComponentData<Childrens>(childrens.children[arrayIndex]).children[0];

                RenderText text = World.EntityManager.GetComponentData<RenderText>(textGrandChild);
                if (numberString == "0" || numberString == "1")
                {
                    numberString = "";
                }
                text.SetText(numberString);
                World.EntityManager.SetComponentData(textGrandChild, text);
            }
        }

        public override void OnSelectedButton(int characterID, int arrayIndex)
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
        }

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
            List<Entity> statIcons = new List<Entity>();
            float2 iconSize = uiDatam.defaultIconSize;
            for (int i = 0; i < inventory.items.Length; i++)
            {
                Texture2D iconTexture = null;
                if (inventory.items[i].metaID != 0)
                {
                    ItemDatam itemDatam = meta[inventory.items[i].metaID];
                    if (itemDatam != null && itemDatam.texture)
                    {
                        iconTexture = itemDatam.texture.texture;
                    }
                }
                float3 position = float3.zero; // GetGridPosition(i, 3, 3);
                Entity icon = UIUtilities.SpawnButton(
                    World.EntityManager,
                    panelUI,
                    position,
                    iconSize,
                    iconTexture,
                    uiDatam.defaultPlayerIcon);
                statIcons.Add(icon);
                Childrens textLink = new Childrens { children = new BlitableArray<Entity>(1, Unity.Collections.Allocator.Persistent) };
                string numberString = ((int)inventory.items[i].quantity).ToString();
                if (numberString == "0" || numberString == "1")
                {
                    numberString = "";
                }
                textLink.children[0] = UIUtilities.SpawnText(World.EntityManager, icon, numberString, float3.zero, uiDatam.overlayTextColor);
                //    (byte)(uiDatam.overlayTextColor.r * 255), (byte)(uiDatam.overlayTextColor.g * 255), (byte)(uiDatam.overlayTextColor.b * 255));//, iconSize);
                World.EntityManager.AddComponentData(icon, textLink);
            }
            Childrens children = new Childrens { };
            children.children = new BlitableArray<Entity>(statIcons.Count, Unity.Collections.Allocator.Persistent);
            for (int i = 0; i < statIcons.Count; i++)
            {
                children.children[i] = statIcons[i];
            }
            World.EntityManager.AddComponentData(panelUI, children);
            //Debug.LogError("Adding GridUI to new Inventory.");
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