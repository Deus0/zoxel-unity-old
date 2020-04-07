using Unity.Entities;
using System.Collections.Generic;
using Unity.Mathematics;
using Zoxel.Voxels;
using UnityEngine;

namespace Zoxel
{

    public struct VoxBuildLayer
    {
        public List<VoxData> voxes;
        public List<int3> positions;
        public List<VoxOperation> operations;
        public List<int> parents;
        public List<int3> bonePositions;
        public List<byte> axes;
        //public List<float3> realPositions;

        public void Init()
        {
            voxes = new List<VoxData>();
            positions = new List<int3>();
            operations = new List<VoxOperation>();
            parents = new List<int>();
            bonePositions = new List<int3>();
            axes = new List<byte>();
        }
    }

    public enum VoxOperation
    {
        None,
        FlipX
    }

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
                    //var voxes = new List<VoxData>();
                    //var positions = new List<int3>();    // after added them all, offset all positions by min, also get max to use as size
                    //var operations = new List<VoxOperation>(); 
                    var bodyLayer = new VoxBuildLayer();
                    bodyLayer.Init();
                    var gearLayer = new VoxBuildLayer();
                    gearLayer.Init();
                    for (int i = 0; i < equipment.body.Length; i++)
                    {
                        // first find core
                        // then for its meta
                        // find any children of that core
                        // then loop for them
                        // different merge function for each axis enum + the offset of the slot
                        var core = equipment.body[i].data;
                        if (meta.ContainsKey(core.id))
                        {
                            if (core.maleSlot.id == 0)
                            {
                                var coreDatam = meta[core.id];
                                //Debug.LogError("Adding Core");
                                bodyLayer.voxes.Add(coreDatam.model.data);
                                bodyLayer.positions.Add(int3.Zero());
                                bodyLayer.operations.Add(VoxOperation.None);
                                bodyLayer.parents.Add(-1);
                                bodyLayer.bonePositions.Add(coreDatam.model.data.size / 2);
                                bodyLayer.axes.Add((byte)SlotAxis.Center);
                                //float3 realPosition = coreDatam.model.data.size.ToFloat3()/2f;
                                //bodyLayer.realPositions.Add(realPosition);
                                AddChildren(ref equipment,
                                    ref bodyLayer, ref gearLayer, //ref voxes, ref positions, ref operations,
                                    VoxOperation.None, core, i, int3.Zero());
                                break;
                            }
                        }
                    }
                    // combine voxes
                    var combinedVoxes = new List<VoxData>();
                    combinedVoxes.AddRange(bodyLayer.voxes);
                    combinedVoxes.AddRange(gearLayer.voxes);
                    var combinedPositions = new List<int3>();
                    combinedPositions.AddRange(bodyLayer.positions);
                    combinedPositions.AddRange(gearLayer.positions);
                    int3 size = VoxData.GetSize(combinedVoxes, combinedPositions);
                    int3 min;
                    int3 max;
                    VoxData.CalculateMinMax(combinedVoxes, combinedPositions, out min, out max);
                    int3 addition = VoxData.CalculateAddition(min, max);
                    bodyLayer.positions = VoxData.FixPositions(bodyLayer.positions, addition);
                    gearLayer.positions = VoxData.FixPositions(gearLayer.positions, addition);
                    model.Build(bodyLayer, gearLayer, size);
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
                    if (World.EntityManager.HasComponent<Skeleton>(e))
                    {
                        var skeleton = World.EntityManager.GetComponentData<Skeleton>(e);
                        //skeleton.SetBody(model.size, bodyLayer.positions, bodyLayer.voxes);
                        combinedPositions = VoxData.FixPositions(combinedPositions, addition);
                        bodyLayer.bonePositions = VoxData.FixPositions(bodyLayer.bonePositions, addition);
                        skeleton.SetBody(model.size, combinedPositions, combinedVoxes);
                        skeleton.SetBones(World.EntityManager, e, bodyLayer.positions.ToArray(), bodyLayer.voxes.ToArray(), bodyLayer.bonePositions.ToArray(), 
                            bodyLayer.parents.ToArray(), bodyLayer.axes.ToArray());
                        World.EntityManager.SetComponentData(e, skeleton);
                    }
                }
            });
        }
        // real positions
        // first take away center offset - half bounds of mesh
        //float3 halfBounds = 0.5f * model.size.ToFloat3();
        // Debug.LogError("model.size: " + model.size + ", halfBounds: " + (halfBounds / 16f));
        // Debug.LogError("min: " + min + ", max: " + max + ", addition: " + addition);
        /*for (int i = 0; i < bodyLayer.realPositions.Count; i++)
        {
            bodyLayer.realPositions[i] += addition.ToFloat3();
            bodyLayer.realPositions[i] -= halfBounds;
            bodyLayer.realPositions[i] = new float3(
                bodyLayer.realPositions[i].x / ((float)model.size.x),
                bodyLayer.realPositions[i].y / ((float)model.size.y),
                bodyLayer.realPositions[i].z / ((float)model.size.z)
            );
            //Debug.LogError("Offset is: " + bodyLayer.positions[i] + " and real: " + bodyLayer.realPositions[i]);
        }*/

        bool AddChildren(ref Equipment equipment,
            ref VoxBuildLayer bodyLayer, ref VoxBuildLayer gearLayer,
            VoxOperation lastOperation, Item parentBodyPart, int bodyIndex, int3 position)
        {
            // for all female slots, find an empty one if it is the slot of our male one
            var parentModel = meta[parentBodyPart.id].model.data;   // get the part im attached to
            for (int j = 0; j < parentBodyPart.femaleSlots.Length; j++)
            {
                var femaleSlot = parentBodyPart.femaleSlots[j];
                byte femaleModifier = 0;
                if (j < parentBodyPart.femaleModifiers.Length)
                {
                    femaleModifier = parentBodyPart.femaleModifiers[j];
                }
                int3 femaleOffset = int3.Zero();
                if (j < parentBodyPart.femaleOffsets.Length)
                {
                    femaleOffset = parentBodyPart.femaleOffsets[j];
                }
                // find child index - bodyIndex is childIndex
                for (int k = 0; k < equipment.body.Length; k++)
                {
                    var attachedBodyPart = equipment.body[k];
                    if (bodyIndex != k && attachedBodyPart.bodyIndex == bodyIndex && attachedBodyPart.slotIndex == j && meta.ContainsKey(attachedBodyPart.data.id))
                    {
                        var attachItem = attachedBodyPart.data;
                        var maleSlot = attachItem.maleSlot;
                        if (maleSlot.id == femaleSlot.id)
                        {
                            var attachedModel = meta[attachItem.id].model.data;       // new part adding to body
                            var operation = lastOperation;
                            if (((VoxOperation)femaleModifier) != VoxOperation.None)
                            {
                                operation = ((VoxOperation)femaleModifier);
                            }
                            int3 offset = position;

                            // using axis, calculate position offset

                            if (femaleSlot.axis == ((byte)SlotAxis.Bottom))
                            { 
                                offset += new int3(0, -attachedModel.size.y, 0);
                                offset.x += (parentModel.size.x - attachedModel.size.x) / 2;
                                offset.z += (parentModel.size.z - attachedModel.size.z) / 2;
                            }
                            else if (femaleSlot.axis == ((byte)SlotAxis.Top)) 
                            { 
                                offset += new int3(0, parentModel.size.y, 0);
                                offset.x += (parentModel.size.x - attachedModel.size.x) / 2;
                                offset.z += (parentModel.size.z - attachedModel.size.z) / 2;
                            }
                            else if (femaleSlot.axis == ((byte)SlotAxis.Left)) 
                            { 
                                offset.y += (parentModel.size.y - attachedModel.size.y) / 2;
                                offset.z += (parentModel.size.z - attachedModel.size.z) / 2;
                                if (operation == VoxOperation.FlipX)
                                {
                                    offset += new int3(parentModel.size.x, 0, 0);
                                }
                                else
                                {
                                    offset += new int3(-attachedModel.size.x, 0, 0);
                                }
                            }
                            else if (femaleSlot.axis == ((byte)SlotAxis.Right)) 
                            { 
                                offset.y += (parentModel.size.y - attachedModel.size.y) / 2;
                                offset.z += (parentModel.size.z - attachedModel.size.z) / 2;
                                if (operation == VoxOperation.FlipX)
                                {
                                    offset += new int3(-attachedModel.size.x, 0, 0);
                                }
                                else
                                {
                                    offset += new int3(parentModel.size.x, 0, 0);
                                }
                            }
                            // done
                            if (operation == VoxOperation.FlipX)
                            {
                                offset +=new int3(-attachItem.offset.x,  attachItem.offset.y,  attachItem.offset.z);
                                offset +=new int3(-femaleOffset.x,  femaleOffset.y,  femaleOffset.z);
                            }
                            else 
                            {
                                offset += attachItem.offset;
                                offset += femaleOffset;
                                //Debug.LogError("Norm: Offsetting model by: " + (femaleOffset.x));
                                if (femaleSlot.axis == ((byte)SlotAxis.Bottom) || femaleSlot.axis == ((byte)SlotAxis.Top))
                                {
                                    offset += new int3((attachedModel.size.x % 2), 0, (attachedModel.size.z % 2));
                                    offset += new int3((parentModel.size.x % 2), 0, (parentModel.size.z % 2));
                                }
                            }
                            int3 bonePosition = offset;
                            if (femaleSlot.axis == ((byte)SlotAxis.Bottom) || femaleSlot.axis == ((byte)SlotAxis.Top))
                            {
                                bonePosition.x += attachedModel.size.x / 2;
                                bonePosition.z += attachedModel.size.z / 2;
                            }
                            else if (femaleSlot.axis == ((byte)SlotAxis.Left) || femaleSlot.axis == ((byte)SlotAxis.Right))
                            {
                                bonePosition.y += attachedModel.size.y / 2;
                                bonePosition.z += attachedModel.size.z / 2;
                                if (operation == VoxOperation.FlipX)
                                {
                                    bonePosition.x += attachedModel.size.x;
                                }
                            }
                            if (femaleSlot.layer == 0)
                            {
                                bodyLayer.voxes.Add(attachedModel);
                                bodyLayer.positions.Add(offset);
                                bodyLayer.operations.Add(operation);
                                bodyLayer.axes.Add(femaleSlot.axis);
                                bodyLayer.parents.Add(bodyIndex);
                                bodyLayer.bonePositions.Add(bonePosition);
                            }
                            else
                            {
                                gearLayer.voxes.Add(attachedModel);
                                gearLayer.positions.Add(offset);
                                gearLayer.operations.Add(operation);
                                gearLayer.axes.Add(femaleSlot.axis);
                                gearLayer.parents.Add(bodyIndex);
                                gearLayer.bonePositions.Add(bonePosition);
                            }
                            if (!AddChildren(ref equipment,ref bodyLayer, ref gearLayer, operation, attachItem, k, offset))
                            {
                                return false;
                            }
                            break;
                        }
                    }
                }
            }
            return true;
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