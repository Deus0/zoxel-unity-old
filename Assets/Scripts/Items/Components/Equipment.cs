using Unity.Collections;
using Unity.Entities;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Zoxel
{
    [System.Serializable]
    public struct Equipment : IComponentData
    {
        public byte dirty;
        public BlitableArray<EquipmentItem> body;
        public BlitableArray<EquipmentItem> gear;

        // this should use the methods in ItemUISystem when trying to equip a new itemMeta onto the body
        public void EquipBody(List<ItemDatam> body_)
        {
            if (body_.Count > 0) 
            {
                dirty = 1;
                body = new BlitableArray<EquipmentItem>(0, Allocator.Persistent);
                foreach (var item in body_)
                {
                    AddNewBodyPart(item.data);
                }
            }
        }


        public void EquipGear(List<ItemDatam> body_)
        {
            if (body_.Count > 0) 
            {
                dirty = 1;
                gear = new BlitableArray<EquipmentItem>(0, Allocator.Persistent);
                foreach (var item in body_)
                {
                    AddNewBodyPart(item.data);
                }
            }
        }

        public bool AddNewBodyPart(Item item)
        {
            if (item.maleSlot.id == 0)
            {
                AddNewBodyPart(0, 0, item);
                return true;
            }
            float2 foundIndex = FindNewBodyIndex(item);
            if (foundIndex.x != -1 && foundIndex.y != -1)
            {
                int bodyPartIndex = (int)foundIndex.x;
                int bodyPartSlotIndex = (int)foundIndex.y;
                AddNewBodyPart(bodyPartIndex, bodyPartSlotIndex, item);
                return true;
            }
            Debug.LogError("Could not find place index for item: " + item.id);
            return false;
        }

        public void AddNewBodyPart(ref Inventory inventory, int itemArrayIndex, int bodyPartIndex, int slotIndex)
        {
            var item = inventory.items[itemArrayIndex];
            //var itemDatam = meta[item2.metaID];
            AddNewBodyPart(bodyPartIndex, slotIndex, item.data);
            item.data = new Item();
            item.quantity = 0;
            inventory.items[itemArrayIndex] = item;
        }

        public void AddNewBodyPart(int bodyPartIndex, int slotIndex, Item item)
        {
            var oldBodyParts = body.ToArray();
            body = new BlitableArray<EquipmentItem>(oldBodyParts.Length + 1, Allocator.Persistent);
            for (int i = 0; i < oldBodyParts.Length; i++)
            {
                body[i] = oldBodyParts[i];
            }
            var bodyPart = new EquipmentItem();
            bodyPart.data = item;
            bodyPart.bodyIndex = bodyPartIndex;
            bodyPart.slotIndex = slotIndex;
            body[oldBodyParts.Length] = bodyPart;
            dirty = 1;
        }

        public void ReplaceBodyPart(ref Inventory inventory, int itemArrayIndex, int bodyPartIndex)
        {
            var bodyPart = body[bodyPartIndex];
            var bodyItem = inventory.items[itemArrayIndex];
            //UnityEngine.Debug.LogError("Replacing body part: " + bodyPart.data.id + " with " +  bodyItem.data.id);
            // switch their metaID around
            var tempData = bodyPart.data;
            bodyPart.data = bodyItem.data;
            bodyItem.data = tempData;
            // then set in components
            body[bodyPartIndex] = bodyPart;
            inventory.items[itemArrayIndex] = bodyItem;
            //UnityEngine.Debug.LogError("Replaced body part: " + bodyPart.data.id + " with " +  bodyItem.data.id + ", bodyPartIndex: " + bodyPartIndex);
            dirty = 1;
        }

        // i want an item i can replace
        public int FindReplaceBodyIndex(Item item)
        {
            var maleSlotNeeded = item.maleSlot.id;
            for (int i = 0; i < body.Length; i++)
            {
                var thisItem = body[i];// meta[body[i].metaID];
                if (thisItem.data.maleSlot.id == maleSlotNeeded)
                {
                    //Debug.LogError("Finding ReplaceBodyIndex: " + thisItem.data.maleSlot.id + ":" + i);
                    return i;
                }
            }
            return -1;
        }

        public float2 FindNewBodyIndex(Item item)
        {
            var maleSlot =  item.maleSlot;
            var femaleSlotNeeded = maleSlot.id;
            for (int i = 0; i < body.Length; i++)
            {
                var thisItem = body[i].data;//meta[body[i].metaID];
                // for all female slots, find an empty one if it is the slot of our male one
                for (int j = 0; j < thisItem.femaleSlots.Length; j++)
                {
                    var femaleSlot = thisItem.femaleSlots[j];
                    var connectToSlot = femaleSlot.id;
                    if (connectToSlot == femaleSlotNeeded)
                        /*&& (femaleSlot.axis == SlotAxis.Bottom && maleSlot.axis == SlotAxis.Top) 
                        || (femaleSlot.axis == SlotAxis.Top && maleSlot.axis == SlotAxis.Bottom)
                        || (femaleSlot.axis == SlotAxis.Left && maleSlot.axis == SlotAxis.Right)
                        || (femaleSlot.axis == SlotAxis.Right && maleSlot.axis == SlotAxis.Left))*/
                    {
                        // check if slot is the male we need
                        bool anythingConnectedToIt = false;
                        // check if anything connected to it
                        for (int k = 0; k < body.Length; k++)
                        {
                            if (i != k)
                            {
                                if (body[k].bodyIndex == i && body[k].slotIndex == j)
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

        public void InitializeBody(int count)
        {
            body = new BlitableArray<EquipmentItem>(count, Allocator.Persistent);
        }
        public void InitializeGear(int count)
        {
            gear = new BlitableArray<EquipmentItem>(count, Allocator.Persistent);
        }

        public void Dispose()
        {
            if (body.Length > 0)
            {
                body.Dispose();
            }
        }

        #region SerializableBlittableArray
        [System.Serializable]
        public struct SerializeableEquipment
        {
            public EquipmentItem.SerializableEquipmentItem[] body;
            public EquipmentItem.SerializableEquipmentItem[] gear;
        }

        public string GetJson()
        {
            var clone = new SerializeableEquipment();
            clone.body = new EquipmentItem.SerializableEquipmentItem[body.Length];
            for (int i = 0; i < body.Length; i++)
            {
                clone.body[i] = body[i].GetSerializeableClone();
            }
            clone.gear = new EquipmentItem.SerializableEquipmentItem[gear.Length];
            for (int i = 0; i < gear.Length; i++)
            {
                clone.gear[i] = gear[i].GetSerializeableClone();
            }
            var json = UnityEngine.JsonUtility.ToJson(clone);
            //Debug.LogError("json: " + json + ":" + body.Length);
            return json;
        }

        public static Equipment FromJson(string json)
        {
            SerializeableEquipment myClone = UnityEngine.JsonUtility.FromJson<SerializeableEquipment>(json);
            Equipment equipment = new Equipment { };
            if (myClone.body != null)
            {
                equipment.InitializeBody(myClone.body.Length);
                for (int i = 0; i < myClone.body.Length; i++)
                {
                    equipment.body[i] = myClone.body[i].GetRealOne();
                }
            }
            else
            {
                Debug.LogError("Body is null from equipment clone: " + json);
            }
            if (myClone.gear != null)
            {
                equipment.InitializeGear(myClone.gear.Length);
                for (int i = 0; i < myClone.gear.Length; i++)
                {
                    equipment.gear[i] = myClone.gear[i].GetRealOne();
                }
            }
            else
            {
                Debug.LogError("Gear is null from equipment clone: " + json);
            }
            return equipment;
        }
        #endregion
    }
}
