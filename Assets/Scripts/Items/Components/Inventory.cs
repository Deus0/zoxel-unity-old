using Unity.Collections;
using Unity.Entities;
using System.Collections.Generic;

namespace Zoxel
{
    [System.Serializable]
    public struct InventoryItem
    {
        public int metaID;
        public int quantity;
    }

    [System.Serializable]
    public struct Inventory : IComponentData
    {
        public byte updated;
        public BlitableArray<InventoryItem> items;

        public void InitializeItems(int count, List<ItemDatam> meta = null)
        {
            items = new BlitableArray<InventoryItem>(count, Allocator.Persistent);
            for (int j = 0; j < items.Length; j++)
            {
                if (meta != null && j < meta.Count)
                {
                    items[j] = new InventoryItem { metaID = meta[j].Value.id, quantity = 1 };
                }
                else
                {
                    items[j] = new InventoryItem { metaID = 0, quantity = 0 };
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
            public InventoryItem[] items;
        }
        public string GetJson()
        {
            SerializeableInventory myClone = new SerializeableInventory();
            myClone.items = items.ToArray();
            return UnityEngine.JsonUtility.ToJson(myClone);
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
            inventory.InitializeItems(myClone.items.Length);
            for (int i = 0; i < myClone.items.Length; i++)
            {
                inventory.items[i] = myClone.items[i];
            }
            return inventory;
        }
        #endregion
    }
}