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

        // i want an item i can replace
        private int FindReplaceBodyIndex(Equipment equipment, ItemDatam item)
        {
            if (item.maleSlots.Count == 0)
            {
                return -1;
            }
            var maleSlot =  item.maleSlots[0];
            string maleSlotNeeded = maleSlot.name;
            for (int i = 0; i < equipment.body.Length; i++)
            {
                var thisItem = meta[equipment.body[i].metaID];
                
                if (thisItem.maleSlots.Count > 0 && 
                    thisItem.maleSlots[0].name == maleSlotNeeded)
                {
                    return i;
                }
            }
            return -1;
        }

        private float2 FindNewBodyIndex(Equipment equipment, ItemDatam item)
        {
            if (item.maleSlots.Count == 0)
            {
                return new float2(-1, -1);
            }
            var maleSlot =  item.maleSlots[0];
            string femaleSlotNeeded = maleSlot.name;
            for (int i = 0; i < equipment.body.Length; i++)
            {
                var thisItem = meta[equipment.body[i].metaID];
                // for all female slots, find an empty one if it is the slot of our male one
                for (int j = 0; j < thisItem.femaleSlots.Count; j++)
                {
                    var femaleSlot = thisItem.femaleSlots[j];
                    string connectToSlot = femaleSlot.name;
                    if (connectToSlot == femaleSlotNeeded
                        && (femaleSlot.axis == SlotAxis.Bottom && maleSlot.axis == SlotAxis.Top) 
                        || (femaleSlot.axis == SlotAxis.Top && maleSlot.axis == SlotAxis.Bottom)
                        || (femaleSlot.axis == SlotAxis.Left && maleSlot.axis == SlotAxis.Right)
                        || (femaleSlot.axis == SlotAxis.Right && maleSlot.axis == SlotAxis.Left))
                    {
                        // check if slot is the male we need
                        bool anythingConnectedToIt = false;
                        // check if anything connected to it
                        for (int k = 0; k < equipment.body.Length; k++)
                        {
                            if (i != k)
                            {
                                if (equipment.body[k].bodyIndex == i && equipment.body[k].slotIndex == j)
                                {
                                    anythingConnectedToIt = true;
                                    break;
                                }
                            }
                        }
                        if (!anythingConnectedToIt)
                        {
                            // return the index of body part to connect to, and the slot index (female) it will connect to, say Chest, and Head slot
                            return new float2(i, j);
                        }
                    }
                }
            }
            return new float2(-1, -1);
        }

        private void ReplaceBodyPart(ref Inventory inventory, int itemArrayIndex, ref Equipment equipment, int bodyPartIndex)
        {
            var bodyPart = equipment.body[bodyPartIndex];
            var bodyItem = inventory.items[itemArrayIndex];
            UnityEngine.Debug.LogError("Replacing body part: " + meta[bodyPart.metaID].name
                 + " with " +  meta[bodyItem.metaID].name);
            // switch their metaID around
            int tempMetaID = bodyPart.metaID;
            bodyPart.metaID = bodyItem.metaID;
            bodyItem.metaID = tempMetaID;
            // then set in components
            equipment.body[bodyPartIndex] = bodyPart;
            inventory.items[itemArrayIndex] = bodyItem;
            equipment.dirty = 1;
        }

        private void AddNewBodyPart(ref Inventory inventory, int itemArrayIndex, ref Equipment equipment, int bodyPartIndex, int slotIndex)
        {
            var oldBodyParts = equipment.body.ToArray();
            equipment.body = new BlitableArray<InventoryBody>(oldBodyParts.Length + 1, Allocator.Persistent);
            for (int i = 0; i < oldBodyParts.Length; i++)
            {
                equipment.body[i] = oldBodyParts[i];
            }
            InventoryItem item2 = inventory.items[itemArrayIndex];
            var bodyPart = new InventoryBody();
            bodyPart.metaID = item2.metaID;
            bodyPart.bodyIndex = bodyPartIndex;
            bodyPart.slotIndex = slotIndex;

            item2.metaID = 0;
            item2.quantity = 0;
            inventory.items[itemArrayIndex] = item2;

            equipment.body[oldBodyParts.Length] = bodyPart;
            equipment.dirty = 1;
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
            Inventory inventory = World.EntityManager.GetComponentData<Inventory>(player);
            InventoryItem item2 = inventory.items[arrayIndex];
            // button x is equip
            if (buttonType == ButtonType.ButtonX)
            {
                if (item2.metaID != 0)
                {
                    if (World.EntityManager.HasComponent<Equipment>(player) == false)
                    {
                        UnityEngine.Debug.LogError("Character " + player.Index + " does not have equipment component.");
                        return;
                    }
                    //int characterID = World.EntityManager.GetComponentData<ZoxID>(player).id;
                    var equipment = World.EntityManager.GetComponentData<Equipment>(player);
                    // new item to equip, check if any slots
                    var newBodyPart = meta[item2.metaID];
                    float2 foundIndex = FindNewBodyIndex(equipment, newBodyPart);
                    if (foundIndex.x != -1 && foundIndex.y != -1)
                    {
                        int bodyPartIndex = (int)foundIndex.x;
                        int bodyPartSlotIndex = (int)foundIndex.y;
                        AddNewBodyPart(ref inventory, arrayIndex, ref equipment, bodyPartIndex, bodyPartSlotIndex);
                        World.EntityManager.SetComponentData(player, equipment);
                        World.EntityManager.SetComponentData(player, inventory);
                        // add item event so it updates ui
                        SetItemUIIcon(button, player, arrayIndex);
                        SetItemUIText(button, player, arrayIndex);
                    }
                    else
                    {
                        // try replace another?
                        var bodyPartIndex = FindReplaceBodyIndex(equipment, newBodyPart);
                        if (bodyPartIndex != -1)
                        {
                            ReplaceBodyPart(ref inventory, arrayIndex, ref equipment, bodyPartIndex);
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
                    if (item2.metaID != 0)
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
                    InventoryItem item1 = inventory.items[selected];
                    inventory.items[selected] = item2;
                    inventory.items[arrayIndex] = item1;

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