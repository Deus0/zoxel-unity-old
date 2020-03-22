using Unity.Entities;
using System.Collections.Generic;
using Unity.Mathematics;
using Zoxel.Voxels;
using UnityEngine;

namespace Zoxel
{
    // Equip system will add updates / resizes to the world if body is dirty
    [DisableAutoCreation]
    public class EquipmentSystem : ComponentSystem
    {
        public Dictionary<int, ItemDatam> meta;

        protected override void OnUpdate()
        {
            Entities.WithAll<Equipment>().ForEach((Entity e, ref Equipment equipment) =>
            {
                if (equipment.dirty == 1)
                {
                    equipment.dirty = 0;
                    // get VoxData of the character stored in world system
                    VoxData model = new VoxData {
                        id = Bootstrap.GenerateUniqueID()
                    };    // make this part of equipment
                    for (int i = 0; i < equipment.body.Length; i++)
                    {
                        //if (equipment.body[i].dirty == 1)
                        {
                            var bodyPart = equipment.body[i];
                            bodyPart.dirty = 0;
                            equipment.body[i] = bodyPart;
                            if (i == 0)
                            {
                                model.Merge(meta[equipment.body[i].metaID].model.data, int3.Zero());
                            }
                            else
                            {
                                var body = meta[equipment.body[i - 1].metaID].model.data;   // get the part im attached to
                                var head = meta[equipment.body[i].metaID].model.data;       // new part adding to body
                                model.Merge(head, new int3(1, body.size.y, 0));
                            }
                            // add body part to voxData (merge into it)
                        }
                    }
                    for (int i = 0; i < equipment.gear.Length; i++)
                    {
                        //if (equipment.gear[i].dirty == 1)
                        {
                            var gearPart = equipment.gear[i];
                            gearPart.dirty = 0;
                            equipment.gear[i] = gearPart;
                            // merge gear over the top - in locations of the head
                        }
                    }
                    // updates body model using this new vox data
                    
                    float3 bodySize = model.GetSize();
                    World.EntityManager.SetComponentData(e, new Body { size = bodySize });
                    WorldBound worldBound = World.EntityManager.GetComponentData<WorldBound>(e);
                    worldBound.size = bodySize;
                    World.EntityManager.SetComponentData(e, worldBound);
                    // this can be done in equip system
                    int id = World.EntityManager.GetComponentData<ZoxID>(e).id;
                    //Debug.LogError("Equipment Mesh updated for: " + id);
                    WorldSpawnSystem.QueueUpdateModel(World.EntityManager, e, id, model);
                }
            });
        }
    }
}