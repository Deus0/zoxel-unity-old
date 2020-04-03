using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Transforms;
using Unity.Burst;
using UnityEngine;

namespace Zoxel
{
    [DisableAutoCreation]
    public class ItemHitCompleterSystem : ComponentSystem
    {
        public InventoryUISpawnSystem inventoryUISpawnSystem;
        public ItemSpawnerSystem itemSpawnSystem;
        public DoomedToDieSystem doomedToDieSystem;

        protected override void OnUpdate()
        {
            Entities.WithAll<ItemHitTaker>().ForEach((Entity littlebitchEntity, ref ItemHitTaker littlebitch) => // , ZoxID , ref ZoxID littlebitchID
            {
                if (littlebitch.wasHit == 1)
                {
                    littlebitch.wasHit = 0;
                    UseItem(littlebitch.itemID, littlebitchEntity); // littlebitchID.id);
                    // else bullet has already been removed
                }
            });
        }

        // Move to ItemPickupSystem
        void UseItem(int id, Entity character)//int characterID)
        {
            if (itemSpawnSystem.items.ContainsKey(id))
            {
                Entity entity = itemSpawnSystem.items[id];
                var worldItem = World.EntityManager.GetComponentData<WorldItem>(entity);
                var characterInventory = World.EntityManager.GetComponentData<Inventory>(character);
                int metaID = worldItem.data.id;
                bool wasPickedUp = false;
                int updatedItemIndex = -1;
                int updatedItemValue = -1;
                for (int i = 0; i < characterInventory.items.Length; i++)
                {
                    if (characterInventory.items[i].data.id == metaID)
                    {
                        InventoryItem item = characterInventory.items[i];
                        Debug.Log("At " + i + " - Stacking item: " + metaID + " to " + item.quantity + " with added quantity of: " + worldItem.quantity);
                        item.quantity += worldItem.quantity;
                        item.dirtyQuantity = 1;
                        // check max quantity in meta item data
                        characterInventory.items[i] = item;
                        characterInventory.dirty = 1;
                        World.EntityManager.SetComponentData(character, characterInventory);
                        wasPickedUp = true;
                        updatedItemIndex = i;
                        updatedItemValue = item.quantity;
                        break;
                    }
                    else if (characterInventory.items[i].data.id == 0)
                    {
                        InventoryItem item = characterInventory.items[i];
                        item.data = worldItem.data;
                        //item.metaID = metaID;
                        item.quantity = worldItem.quantity;
                        item.dirty = 1;
                        characterInventory.items[i] = item;
                        characterInventory.dirty = 1;
                        World.EntityManager.SetComponentData(character, characterInventory);
                        wasPickedUp = true;
                        updatedItemIndex = i;
                        updatedItemValue = 1;
                        //Debug.Log("At " + i + " - Adding item to inventory: " + metaID + " of quantity: " + worldItem.quantity);
                        break;
                    }
                    //else
                    {
                        //Debug.LogError("At " + i + " - MetaID: " + characterInventory.items[i].metaID);
                    }
                }
                //for (int i = 0; i < characterInventory.items.Length; i++)
                {
                    //    Debug.LogError("Double Checking at: " + i + " - MetaID: " + characterInventory.items[i].metaID);
                }
                if (!wasPickedUp)
                {
                    /*Debug.Log("Inventory is full. "
                        + "Add sound and maybe some text in the log." +
                        "Inventory Items at: " + characterInventory.items.Length);*/
                    return;
                }
                // get inventory Ui
                //ZoxID zoxID = World.EntityManager.GetComponentData<ZoxID>(character);
                //inventoryUISpawnSystem.UpdateIconText(zoxID.id, updatedItemIndex);
                /*if (inventoryUISpawnSystem.uis.ContainsKey(zoxID.id) && updatedItemIndex != -1)
                {
                    Debug.Log("Updating Item at number [" + updatedItemIndex + "] to quantity x" + updatedItemValue);
                    // update entity bunch as numbers with character UI
                    Entity icon = inventoryUISpawnSystem.icons[zoxID.id].icons[updatedItemIndex];
                    string numberString = updatedItemValue.ToString();

                    CharacterInventoryUIData iconContainer = inventoryUISpawnSystem.icons[zoxID.id];
                    EntityBunch number = iconContainer.numbers[updatedItemIndex];
                    number.entities =
                        Bootstrap.instance.systemsManager.UIUtilities.UpdateNumbers(number.entities, icon, numberString);
                    iconContainer.numbers[updatedItemIndex] = number;
                    inventoryUISpawnSystem.icons[zoxID.id] = iconContainer;
                }*/
                //int metaID = worldItem.metaID;
                itemSpawnSystem.items.Remove(id);
                Translation itemPosition = World.EntityManager.GetComponentData<Translation>(entity);
                World.EntityManager.RemoveComponent<WorldItem>(entity);
                World.EntityManager.RemoveComponent<ItemBob>(entity);
                //World.EntityManager.DestroyEntity(entity);
                //Translation characterPosition = World.EntityManager.GetComponentData<Translation>(character);
                World.EntityManager.AddComponentData(entity, new PositionEntityLerper
                {
                    createdTime = UnityEngine.Time.time,
                    lifeTime = 1,
                    positionBegin = itemPosition.Value,
                    positionEnd = character
                });
                doomedToDieSystem.MarkForDeath(entity, 1);
                AudioManager.instance.PlaySound(itemSpawnSystem.meta[metaID].pickedUp, itemPosition.Value);
            }
        }
    }

}