using Unity.Collections;
using Unity.Entities;
using System.Collections.Generic;

namespace Zoxel
{
    [System.Serializable]
    public struct Inventory : IComponentData
    {
        public byte dirty;
        public BlitableArray<InventoryItem> items;
        public byte selected;
        public Entity selectedButton;

        public void InitializeItems(int count, List<ItemDatam> meta = null)
        {
            items = new BlitableArray<InventoryItem>(count, Allocator.Persistent);
            for (int j = 0; j < items.Length; j++)
            {
                if (meta != null && j < meta.Count)
                {
                    items[j] = new InventoryItem { data = meta[j].data, quantity = 1 };
                }
                else
                {
                    items[j] = new InventoryItem { };
                }
            }
        }
        public void Dispose()
        {
            items.Dispose();
        }

        #region SerializableBlittableArray
        [System.Serializable]
        public struct SerializeableInventory
        {
            public InventoryItem.SerializableInventoryItem[] items;
        }
        public string GetJson()
        {
            var clone = new SerializeableInventory();
            //lone.items = items.ToArray();
            clone.items = new InventoryItem.SerializableInventoryItem[items.Length];
            for (int i = 0; i < items.Length; i++)
            {
                clone.items[i] = items[i].GetSerializeableClone();
            }
            return UnityEngine.JsonUtility.ToJson(clone);
        }
        public static Inventory FromJson(string json)
        {
            if (json == null || json == "")
            {
                Inventory blank = new Inventory { };
                blank.InitializeItems(9);
                return blank;
            }
            SerializeableInventory myClone;
            try
            {
                myClone = UnityEngine.JsonUtility.FromJson<SerializeableInventory>(json);
            }
            catch (System.Exception e)
            {
                Inventory blank = new Inventory { };
                blank.InitializeItems(9);
                return blank;
            }
            Inventory inventory = new Inventory { };
            if (myClone.items != null)
            {
                inventory.InitializeItems(myClone.items.Length);
                for (int i = 0; i < myClone.items.Length; i++)
                {
                    inventory.items[i] = myClone.items[i].GetRealOne();
                }
            }
            else
            {
                UnityEngine.Debug.LogError("Items is null from inventory clone.");
            }
            return inventory;
        }
        #endregion
    }
}