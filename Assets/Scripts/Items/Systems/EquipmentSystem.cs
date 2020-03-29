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
                    List<VoxData> voxes = new List<VoxData>();
                    List<int3> positions = new List<int3>();    // after added them all, offset all positions by min, also get max to use as size
                    for (int i = 0; i < equipment.body.Length; i++)
                    {
                        // first find core
                        // then for its meta
                        // find any children of that core
                        // then loop for them
                        // different merge function for each axis enum + the offset of the slot
                        if (meta.ContainsKey(equipment.body[i].metaID))
                        {
                            var core = meta[equipment.body[i].metaID];
                            if (core.maleSlots.Count > 0 && core.maleSlots[0].name == "core")
                            {
                                //Debug.LogError("Adding Core");
                                voxes.Add(core.model.data);
                                positions.Add(int3.Zero());
                                AddChildren(ref equipment, ref voxes, ref positions, core, i, int3.Zero());
                                break;
                            }
                        }
                    }
                    int3 size = VoxData.GetSize(voxes, positions);
                    positions = VoxData.FixPositions(voxes, positions);
                    model.Build(voxes, positions, size);
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

        void AddChildren(ref Equipment equipment, ref List<VoxData> voxes, ref List<int3> positions, ItemDatam parentBodyPart, int bodyIndex, int3 position)
        {
            // for all female slots, find an empty one if it is the slot of our male one
            var parentBodyPartVox = parentBodyPart.model.data;   // get the part im attached to
            for (int j = 0; j < parentBodyPart.femaleSlots.Count; j++)
            {
                var femaleSlot = parentBodyPart.femaleSlots[j];
                for (int k = 0; k < equipment.body.Length; k++)
                {
                    var bodyPart = equipment.body[k];
                    if (bodyIndex != k && bodyPart.bodyIndex == bodyIndex && bodyPart.slotIndex == j && meta.ContainsKey(bodyPart.metaID))
                    {
                        var possibleConnected = meta[bodyPart.metaID];
                        if (possibleConnected.maleSlots.Count > 0)
                        {
                            var maleSlot = possibleConnected.maleSlots[0];
                            if (maleSlot.name == femaleSlot.name)
                            {
                                var head = possibleConnected.model.data;       // new part adding to body
                                int3 offset = position; //new int3();
                                offset += femaleSlot.offset;
                                offset += maleSlot.offset;
                                if (femaleSlot.axis == SlotAxis.Bottom && maleSlot.axis == SlotAxis.Top)
                                { 
                                    offset += new int3(0, -head.size.y, 0);
                                }
                                else if (femaleSlot.axis == SlotAxis.Left && maleSlot.axis == SlotAxis.Right)
                                { 
                                    offset += new int3(-head.size.x, 0, 0);
                                }
                                else if (femaleSlot.axis == SlotAxis.Top && maleSlot.axis == SlotAxis.Bottom)
                                { 
                                    offset += new int3(0, parentBodyPartVox.size.y, 0);
                                }
                                else if (femaleSlot.axis == SlotAxis.Right && maleSlot.axis == SlotAxis.Left)
                                { 
                                    offset += new int3(parentBodyPartVox.size.x, 0, 0);
                                }
                                voxes.Add(head);
                                positions.Add(offset);  
                                //Debug.LogError("Adding " + maleSlot.name + " with item: " + possibleConnected.name
                                //    + " and offset: " + offset + " and vox size of: " + head.size);
                                AddChildren(ref equipment, ref voxes, ref positions, possibleConnected, k, offset);
                                // use body / previous added position in addition to offset
                                //offset += new int3(0, head.size.y, 0);
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}


/*else if (maleSlot.axis == SlotAxis.Bottom)
{ 
    //offset += new int3(0, -head.size.y, 0);
}
else if (maleSlot.axis == SlotAxis.Top)
{ 
    // if less then previous body parts? how to check?
    offset += new int3(0, head.size.y, 0); // needs to be relative to position of chest
    // if hips below chest, we know position of hips connects at top
    // and also at bottom of chest, therefor overall position of hips will be 0
    // if has really long arms will need to be relative to bottom point
    // so we need a position of chest and chest position will be set when it gets moved too
}*/
//if (equipment.body[i].dirty == 1)
    //var bodyPart = equipment.body[i];
    //bodyPart.dirty = 0;
    //equipment.body[i] = bodyPart;
    //if (meta.ContainsKey(equipment.body[i].metaID))
    //{
        // if (i == 0)
        // {
        // }
        // else
        // {
        // }
    //}
    // add body part to voxData (merge into it)