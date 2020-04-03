using Unity.Entities;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Zoxel.Voxels;

namespace Zoxel
{
    public struct Bone : IComponentData
    {
        public float3 position;           // position within chunk - used for skinning
        public Entity parent;           // parent bone
        public Entity skeleton;
    }
        //public int id;
        //public int3 localPosition;
        //public int3 local;              // position offset from parent position - use this for animation - moving bones

    public struct Skeleton : IComponentData
    {
        public BlitableArray<Entity> bones;
        public int3 size;
        public BlitableArray<int3> positions;
        public BlitableArray<int3> sizes;
        public quaternion mulRotation;

        public void Dispose(EntityManager EntityManager)
        {
            if (bones.Length > 0)
            {
                for (int i = 0; i < bones.Length; i++)
                {
                    if (EntityManager.Exists(bones[i]))
                    {
                        EntityManager.DestroyEntity(bones[i]);
                    }
                }
                bones.Dispose();
            }
        }

        public void SetBody(int3 size_, List<int3> positions_, List<VoxData> voxes)
        {
            size = size_;
            sizes = new BlitableArray<int3>(voxes.Count, Unity.Collections.Allocator.Persistent);
            for (int i = 0; i < voxes.Count; i++)
            {
                sizes[i] = voxes[i].size;
            }
            positions = new BlitableArray<int3>(positions_.Count, Unity.Collections.Allocator.Persistent);
            for (int i = 0; i < positions_.Count; i++)
            {
                //Debug.LogError(i + " - Adding position: " + positions_[i] +", and size: " + sizes[i]);
                //Quaternion backwards = Quaternion.Euler(new Vector3(0, 180, 0));
                //quaternion backwards2 = backwards;
                //positions[i] = new int3(-positions_[i].x, positions_[i].y, positions_[i].z); //math.rotate(backwards2, positions_[i]);
                positions[i] = positions_[i];
            }
        }
        private static EntityArchetype boneArchtype;

        public void SetBones(EntityManager EntityManager, Entity skeleton, int3[] positions, int[] parents)
        {
            Dispose(EntityManager);
            List<Entity> boneList = new List<Entity>();
            //List<BoneData> boneMetas = new List<BoneData>();
            float3 skeletonPosition = EntityManager.GetComponentData<Translation>(skeleton).Value;
            /*if (EntityManager.HasComponent<Parent>(skeleton) == false)
            {
                EntityManager.AddComponentData(skeleton, new Parent { });
            }*/
            if (boneArchtype.Valid == false)
            {
                boneArchtype = EntityManager.CreateArchetype(
                    typeof(Parent),
                    typeof(LocalToParent),
                    typeof(LocalToWorld),
                    //transform
                    typeof(Translation),
                    typeof(Rotation),
                    typeof(NonUniformScale),
                    //typeof(Scale),
                    typeof(Bone)
                    );
            }
            float3 totalSize = size.ToFloat3() / 32f;
            /*var rotation = EntityManager.GetComponentData<Rotation>(skeleton);
            quaternion rot = rotation.Value;
            Quaternion rot2 = Quaternion.Euler(0, 180, 0);
            quaternion rot3 = rot2;
            rot = math.mul(rot, rot3);*/
            Quaternion rot2 = Quaternion.Euler(0, 180, 0);
            quaternion rot3 = rot2;
            for (int i = 0; i < positions.Length; i++) 
            {
                Entity bone = EntityManager.CreateEntity(boneArchtype);
                float3 position = positions[i].ToFloat3() / 32f;
                position -= totalSize / 2f;
                position = math.rotate(rot3, position);
                EntityManager.SetComponentData(bone, new Bone
                {  
                    position = position,
                    skeleton = skeleton 
                });
                boneList.Add(bone);
                // set pos
                EntityManager.SetComponentData(boneList[i], new Parent { Value = skeleton });
                EntityManager.SetComponentData(bone, new Translation { Value = position });
                EntityManager.SetComponentData(bone, new Rotation { Value = quaternion.identity });
                EntityManager.SetComponentData(bone, new NonUniformScale { Value = new float3(1,1,1) });
                //EntityManager.SetComponentData(bone, new Scale { Value = 1 });
            }
            //Debug.LogError("Checking parents for : " + boneList.Count + " bones in skeleton of meta: " + boneMetas.Count);

            /*List<float3> newPositions = new List<float3>();
            for (int i = 0; i < boneList.Count; i++)
            {
                if (i >= parents.Length) {
                    continue;
                }
                int j = parents[i];
                if (j != -1 && j < boneList.Count)
                {
                    EntityManager.SetComponentData(boneList[i], new Parent { Value = boneList[j] });
                    var bone = EntityManager.GetComponentData<Bone>(boneList[i]);
                    var boneParent =  EntityManager.GetComponentData<Bone>(boneList[j]);
                    newPositions.Add(bone.localPosition - boneParent.localPosition);
                    //UnityEngine.Debug.LogError(i + ":" + bone.localPosition + ":" + newPositions[newPositions.Count - 1] + ", is a sub bone of " + j);
                }
                else
                {
                    var bone = EntityManager.GetComponentData<Bone>(boneList[i]);
                    EntityManager.SetComponentData(boneList[i], new Parent { Value = skeleton });
                    newPositions.Add(bone.localPosition);
                }
            }*/

            /*for (int i = 0; i < boneList.Count; i++)
            {
                EntityManager.SetComponentData(boneList[i], new Bone {  localPosition = newPositions[i], skeleton = skeleton });
                EntityManager.SetComponentData(boneList[i], new Translation {  Value = newPositions[i]});
            }*/
            bones = new BlitableArray<Entity>(boneList.Count, Unity.Collections.Allocator.Persistent);
            for (int i = 0; i < bones.Length; i++)
            {
                bones[i] = boneList[i];
            }
        }

        // start at core (first bone)
        // move to any children nodes
        // for all bones, if parent is this index, then set, and then add its child nodes
        // also set positions during this time as minus their parent one
        private void AddChildren(List<Entity> bones)
        {

        }
    }
}

        /*
        public void InitializeBones(EntityManager EntityManager, Entity skeleton, SkeletonData meta)
        {
            List<Entity> boneList = new List<Entity>();
            List<BoneData> boneMetas = new List<BoneData>();
            float3 skeletonPosition = EntityManager.GetComponentData<Translation>(skeleton).Value;
            foreach (var boneMeta in meta.datas)
            {
                Entity bone = EntityManager.CreateEntity();
                float3 localPosition = boneMeta.position + skeletonPosition;
                boneList.Add(bone);
                EntityManager.AddComponentData(bone, new Bone { });
                // set pos
                EntityManager.AddComponentData(bone, new Parent { Value =  skeleton });
                EntityManager.AddComponentData(bone, new LocalToParent { });
                EntityManager.AddComponentData(bone, new LocalToWorld { });
                EntityManager.AddComponentData(bone, new Translation { Value = localPosition });
                EntityManager.AddComponentData(bone, new Rotation { Value = quaternion.identity });
                EntityManager.AddComponentData(bone, new NonUniformScale { Value = new float3(1,1,1) });
                if (boneMeta.name == "Hip")
                {
                    EntityManager.AddComponentData(bone, new PositionLerper
                    {
                        createdTime = UnityEngine.Time.time,
                        lifeTime = 60,
                        positionBegin = boneMeta.position + skeletonPosition,
                        positionEnd = boneMeta.position + skeletonPosition + new float3(0, 8, 0)
                    });
                }
                
                // EntityUtilities.SetParent(EntityManager, skeleton, bone, localPosition, boneMeta.rotation, new float3(1, 1, 1));
                if (boneMeta.name.Contains("Shoulder"))
                {
                    EntityManager.AddComponentData(bone, new SinRotator
                    {
                        timeBegun = UnityEngine.Time.time,
                        multiplier = 2f
                    });   
                }
                boneMetas.Add(boneMeta);
            }
            //Debug.LogError("Checking parents for : " + boneList.Count + " bones in skeleton of meta: " + boneMetas.Count);
            for (int i = 0; i < boneList.Count; i++)
            {
                var boneMetaChild = boneMetas[i];
                for (int j = 0; j < boneList.Count; j++)
                {
                    if (boneMetaChild.parentID == boneMetas[j].id)
                    {
                        //UnityEngine.Debug.LogError("Parent found at " + j + ":" + boneMetas[j].name + " from " + i + ":" + boneMetaChild.name);
                        EntityManager.SetComponentData(boneList[i], new Parent
                        {
                            Value = boneList[j]
                        });
                        break;
                    }
                }
            }
            bones = new BlitableArray<Entity>(boneList.Count, Unity.Collections.Allocator.Persistent);
            for (int i = 0; i < bones.Length; i++)
            {
                bones[i] = boneList[i];
            }
        }
*/