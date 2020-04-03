using Unity.Entities;
using Unity.Mathematics;
using Zoxel.UI;
using Unity.Collections;
//using UnityEngine;
using System.Collections;
using System.Collections.Generic;
// system will use button component, and itemUI
// if button was clicked, handle it here instead of in inventory ui
// button clicked contains which character did the clicking

namespace Zoxel
{
    public struct ItemUI : IComponentData 
    {
        public int index;   // what item it relates to in the inventory
    }

    [DisableAutoCreation]
    public class ItemUISystem : ComponentSystem
    {
        public UnityEngine.Texture2D defaultIconTexture;
        public Dictionary<int, ItemDatam> meta = new Dictionary<int, ItemDatam>();

        protected override void OnUpdate()
        {
            Entities.WithAll<Button, ItemUI, ButtonClickEvent>().ForEach((Entity e, ref Button button, ref ItemUI itemUI, ref ButtonClickEvent buttonEvent) =>
            {
                // do the thing
                OnClickedButton(e, buttonEvent.character, (ButtonType) buttonEvent.buttonType, itemUI.index);
                // finish with event
                World.EntityManager.RemoveComponent<ButtonClickEvent>(e);
            });     
            Entities.WithAll<Button, ItemUI, ButtonSelectEvent>().ForEach((Entity e, ref Button button, ref ItemUI itemUI, ref ButtonSelectEvent buttonEvent) =>
            {
                // do the thing
                //OnClickedButton(buttonEvent.character, (ButtonType) buttonEvent.buttonType, itemUI.index);
                // add tooltip here for inventory
                // finish with event
                World.EntityManager.RemoveComponent<ButtonSelectEvent>(e);
            });   
        }

        public void OnClickedButton(Entity button, Entity player, ButtonType buttonType, int arrayIndex)

        {
            if (buttonType != ButtonType.ButtonA && buttonType != ButtonType.ButtonX)
            {
                return;
            }
            if (World.EntityManager.HasComponent<Inventory>(player) == false)
            {
                UnityEngine.Debug.LogError("Character " + player.Index + " does not have inventory component.");
                return;
            }
            var inventory = World.EntityManager.GetComponentData<Inventory>(player);
            var itemClicked = inventory.items[arrayIndex];
            var itemDataClicked = itemClicked.data;
            // button x is equip
            if (buttonType == ButtonType.ButtonX)
            {
                if (itemDataClicked.id != 0)
                {
                    if (World.EntityManager.HasComponent<Equipment>(player) == false)
                    {
                        UnityEngine.Debug.LogError("Character " + player.Index + " does not have equipment component.");
                        return;
                    }
                    //int characterID = World.EntityManager.GetComponentData<ZoxID>(player).id;
                    var equipment = World.EntityManager.GetComponentData<Equipment>(player);
                    // new item to equip, check if any slots
                    float2 foundIndex = equipment.FindNewBodyIndex(itemDataClicked);
                    if (foundIndex.x != -1 && foundIndex.y != -1)
                    {
                        int bodyPartIndex = (int)foundIndex.x;
                        int bodyPartSlotIndex = (int)foundIndex.y;
                        equipment.AddNewBodyPart(ref inventory, arrayIndex, bodyPartIndex, bodyPartSlotIndex);
                        World.EntityManager.SetComponentData(player, equipment);
                        World.EntityManager.SetComponentData(player, inventory);
                        // add item event so it updates ui
                        SetItemUIIcon(button, player, arrayIndex);
                        SetItemUIText(button, player, arrayIndex);
                    }
                    else
                    {
                        // try replace another?
                        UnityEngine.Debug.LogError("Replacing for item clicked: " + meta[itemDataClicked.id].name);
                        var bodyPartIndex = equipment.FindReplaceBodyIndex(itemDataClicked);
                        if (bodyPartIndex != -1)
                        {
                            equipment.ReplaceBodyPart(ref inventory, arrayIndex, bodyPartIndex);
                            World.EntityManager.SetComponentData(player, equipment);
                            World.EntityManager.SetComponentData(player, inventory);
                            // add item event so it updates ui
                            SetItemUIIcon(button, player, arrayIndex);
                            SetItemUIText(button, player, arrayIndex);
                        }
                    }
                }
                return;
            }
            else if (buttonType == ButtonType.ButtonA)
            {
                // button a is item pickup / swap
                if (inventory.selected == 0)
                {
                    if (itemDataClicked.id != 0)
                    {
                        //isFirst = !isFirst;
                        //firstSelected = arrayIndex;
                        inventory.selected = (byte)(arrayIndex + 1);
                        inventory.selectedButton = button;
                        World.EntityManager.SetComponentData(player, inventory);
                        // change selected icon
                        ///ChangeSelectedIcon(characterID, uiDatam.selectedActioning);
                    }
                }
                else
                {
                    //isFirst = !isFirst;
                    // switch items
                    int selected = ((int)inventory.selected) - 1;// = (byte)(arrayIndex + 1);
                    InventoryItem itemFirstClicked = inventory.items[selected];
                    inventory.items[selected] = itemClicked;
                    inventory.items[arrayIndex] = itemFirstClicked;

                    World.EntityManager.SetComponentData(player, inventory);
                    // update those items/quantities and tooltip
                    int characterID = World.EntityManager.GetComponentData<ZoxID>(player).id;
                    
                    //OnSelectedButton(characterID, arrayIndex);

                    // also store selected button
                    SetItemUIIcon(inventory.selectedButton, player, selected);
                    SetItemUIText(inventory.selectedButton, player, selected);

                    SetItemUIIcon(button, player, arrayIndex);
                    SetItemUIText(button, player, arrayIndex);
                    inventory.selected = 0;
                    World.EntityManager.SetComponentData(player, inventory);
                }
            }
        }
        
        private void SetItemUIIcon(Entity iconEntity, Entity character, int arrayIndex)
        {
            Inventory inventory = World.EntityManager.GetComponentData<Inventory>(character);
            UnityEngine.Texture2D iconTexture = null;
            var itemMetaID = inventory.items[arrayIndex].data.id;
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
                iconTexture = defaultIconTexture;
            }
            //Childrens childrens = World.EntityManager.GetComponentData<Childrens>(uis[characterID]);
            //Entity iconEntity = childrens.children[arrayIndex]; //icons[characterID].entities[arrayIndex];
            var renderer = World.EntityManager.GetSharedComponentData<Unity.Rendering.RenderMesh>(iconEntity);
            renderer.material.SetTexture("_BaseMap", iconTexture);
            //World.EntityManager.SetSharedComponentData(iconEntity, renderer);
        }

        public void SetItemUIText(Entity iconEntity, Entity character, int arrayIndex)
        {
            if (arrayIndex != -1)
            {
                Inventory inventory = World.EntityManager.GetComponentData<Inventory>(character);
                string numberString = inventory.items[arrayIndex].quantity.ToString();
                if (numberString == "0" || numberString == "1")
                {
                    numberString = "";
                }
                //Childrens childrens = World.EntityManager.GetComponentData<Childrens>(uis[characterID]);
                Entity textGrandChild = World.EntityManager.GetComponentData<Childrens>(iconEntity).children[0];
                RenderText text = World.EntityManager.GetComponentData<RenderText>(textGrandChild);
                if (numberString == "0" || numberString == "1")
                {
                    numberString = "";
                }
                text.SetText(numberString);
                World.EntityManager.SetComponentData(textGrandChild, text);
            }
        }

    }
}