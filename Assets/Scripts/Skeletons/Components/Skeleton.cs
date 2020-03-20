using Unity.Entities;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Zoxel
{
    public struct Bone : IComponentData
    {
        public int id;
    }

    public struct Skeleton : IComponentData
    {
        public BlitableArray<Entity> bones;

        public void Dispose()
        {
            if (bones.Length > 0)
            {
                bones.Dispose();
            }
        }
        
        public void InitializeBones(EntityManager EntityManager, Entity skeleton, SkeletonDatam meta)
        {
            List<Entity> boneList = new List<Entity>();
            List<BoneData> boneMetas = new List<BoneData>();
            float3 skeletonPosition = EntityManager.GetComponentData<Translation>(skeleton).Value;
            foreach (var boneMeta in meta.data.datas)
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
    }
}