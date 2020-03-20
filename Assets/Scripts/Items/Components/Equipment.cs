using Unity.Collections;
using Unity.Entities;
using System.Collections.Generic;

namespace Zoxel
{

    [System.Serializable]
    public struct InventoryBody
    {
        public int metaID;
        public byte dirty;
    }

    [System.Serializable]
    public struct InventoryEquipment
    {
        public int metaID;
        public float durability;
        public byte dirty;
    }

    [System.Serializable]
    public struct Equipment : IComponentData
    {
        public byte dirty;
        public BlitableArray<InventoryBody> body;
        public BlitableArray<InventoryEquipment> gear;

        public void EquipBody(List<ItemDatam> body_)
        {
            if (body_.Count > 0) 
            {
                dirty = 1;
                body = new BlitableArray<InventoryBody>(body_.Count, Allocator.Persistent);
                for (int j = 0; j < body.Length; j++)
                {
                    body[j] = new InventoryBody { metaID = body_[j].Value.id, dirty = 1 };
                }
            }
        }
        public void EquipGear(List<ItemDatam> body_)
        {
            gear = new BlitableArray<InventoryEquipment>(body_.Count, Allocator.Persistent);
            for (int j = 0; j < gear.Length; j++)
            {
                gear[j] = new InventoryEquipment { metaID = body_[j].Value.id, durability = 1 };
            }
        }
        public void InitializeBody(int count)
        {
            body = new BlitableArray<InventoryBody>(count, Allocator.Persistent);
        }
        public void InitializeGear(int count)
        {
            gear = new BlitableArray<InventoryEquipment>(count, Allocator.Persistent);
        }
        /*ublic void InitializeItems(int count, List<ItemDatam> meta = null)
        {
            items = new BlitableArray<InventoryEquipment>(count, Allocator.Persistent);
            for (int j = 0; j < items.Length; j++)
            {
                if (meta != null && j < meta.Count)
                {
                    items[j] = new InventoryEquipment { metaID = meta[j].Value.id, durability = 1 };
                }
                else
                {
                    items[j] = new InventoryEquipment { metaID = 0, durability = 0 };
                }
            }
        }*/

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
            public InventoryBody[] body;
            public InventoryEquipment[] gear;
        }

        public string GetJson()
        {
            SerializeableEquipment myClone = new SerializeableEquipment();
            myClone.body = body.ToArray();
            return UnityEngine.JsonUtility.ToJson(myClone);
        }

        public static Equipment FromJson(string json)
        {
            SerializeableEquipment myClone = UnityEngine.JsonUtility.FromJson<SerializeableEquipment>(json);
            Equipment equipment = new Equipment { };
            equipment.InitializeBody(myClone.body.Length);
            for (int i = 0; i < myClone.body.Length; i++)
            {
                equipment.body[i] = myClone.body[i];
            }
            equipment.InitializeGear(myClone.body.Length);
            for (int i = 0; i < myClone.gear.Length; i++)
            {
                equipment.gear[i] = myClone.gear[i];
            }
            return equipment;
        }
        #endregion
    }
}
