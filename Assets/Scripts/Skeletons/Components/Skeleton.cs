using Unity.Entities;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Zoxel.Voxels;
using Unity.Collections;

namespace Zoxel
{
    public struct Bone : IComponentData
    {
        public byte voxelInfluence;
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
        public BlitableArray<byte> boneIndexes;

        public void BakeWeights(EntityManager EntityManager, NativeArray<ZoxelVertex> verticesN)
        {
            var vertices = verticesN.ToArray();
            boneIndexes = new BlitableArray<byte>(vertices.Length, Allocator.Persistent);

            for (int j = 0; j < boneIndexes.Length; j++)
            {
                boneIndexes[j] = 0;
            }
            // for each bone - give it weight for the distance it is to the positions
            float influence = 2 / 32f; // 0.04f;
            float influenceAdd = 2 / 32f; //0.04f;
            for (int k = 0; k < 32; k += 2)
            {
                for (int i = 0; i < bones.Length; i++)
                {
                    float boneInfluence = (int) EntityManager.GetComponentData<Bone>(bones[(int)i]).voxelInfluence;
                    if (influence <= boneInfluence / 32f)
                    {
                        var bonePosition = EntityManager.GetComponentData<Bone>(bones[(int)i]).position;
                        for (int j = 0; j < vertices.Length; j++)
                        {
                            if (boneIndexes[j] == 0)
                            {
                                float distanceTo = math.distance(vertices[j].position, bonePosition);
                                if (distanceTo < influence)
                                {
                                    boneIndexes[j] = (byte)(i + 1);
                                }
                            }
                        }
                    }
                    // influence should have a minimum and max, a bounding box around the bone position
                    // this can be set in bones when generating and debugged with more box lines
                    //DrawDebugSphere(chunk.bones[i], influence);
                    // for each bone, fight weights within radius using vertexes
                }
                influence += influenceAdd;
            }

            for (int j = 0; j < boneIndexes.Length; j++)
            {
                if (boneIndexes[j] != 0)
                {
                    boneIndexes[j] = (byte)((int)boneIndexes[j] - 1);
                }
            }
        }

        public void SetBones(EntityManager EntityManager, Entity skeleton, int3[] positions, int[] parents)
        {
            Dispose(EntityManager);
            List<Entity> boneList = new List<Entity>();
            float3 skeletonPosition = EntityManager.GetComponentData<Translation>(skeleton).Value;
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
            Quaternion rot2 = Quaternion.Euler(0, 180, 0);
            quaternion rot3 = rot2;
            for (int i = 0; i < positions.Length; i++) 
            {
                Entity bone = EntityManager.CreateEntity(boneArchtype);
                float3 position = positions[i].ToFloat3() / 32f;
                position -= totalSize / 2f;
                position = math.rotate(rot3, position);
                Bone newBone = new Bone {
                    position = position,
                    skeleton = skeleton,
                    voxelInfluence = 6
                };
                // chest
                if (i == 0)
                {
                    newBone.voxelInfluence = (byte) 22;
                }
                // head
                if (i == 1)
                {
                    newBone.voxelInfluence = (byte) 32;
                }
                // hips?
                if (i == 2)
                {
                    newBone.voxelInfluence = (byte) 22;
                }
                int leftArmBegin = 9;
                int rightArmBegin = 13;
                if (i == leftArmBegin || i == rightArmBegin)
                {
                    newBone.voxelInfluence = (byte) 12;
                }
                // biceps
                if (i == leftArmBegin + 1 || i == rightArmBegin + 1)
                {
                    newBone.voxelInfluence = (byte) 12;
                }
                // calfs
                if (i == leftArmBegin + 2 || i == rightArmBegin + 2)
                {
                    newBone.voxelInfluence = (byte) 12;
                }
                // hands
                if (i == leftArmBegin + 3 || i == rightArmBegin + 3)
                {
                    newBone.voxelInfluence = (byte) 8;
                }
                int leftLegBegin = 3;
                int rightLegBegin = 6;
                // feet
                if (i == leftLegBegin + 2 || i == rightLegBegin + 2)
                {
                    newBone.voxelInfluence = (byte) 16;
                }
                EntityManager.SetComponentData(bone, newBone);
                boneList.Add(bone);
                // set pos
                EntityManager.SetComponentData(boneList[i], new Parent { Value = skeleton });
                EntityManager.SetComponentData(bone, new Translation { Value = position });
                EntityManager.SetComponentData(bone, new Rotation { Value = quaternion.identity });
                EntityManager.SetComponentData(bone, new NonUniformScale { Value = new float3(1,1,1) });
                
                // head bob
                if (i == 1)
                {
                    EntityManager.AddComponentData(bone, new PositionLerper
                    {
                        createdTime = UnityEngine.Time.time,
                        lifeTime = 0.9f,
                        positionBegin = position - new float3(0, 1 / 32f, 0f),
                        positionEnd = position + new float3(0, 1 / 32f, 0f),
                        loop = 1
                    });
                }
                float armSwingLength = 3 / 32f;
                float swingSpeed = 0.9f;
                // left arm animation
                if (i >= leftArmBegin && i <= leftArmBegin + 3)
                {
                    EntityManager.AddComponentData(bone, new PositionLerper
                    {
                        createdTime = UnityEngine.Time.time,
                        lifeTime = swingSpeed,
                        positionBegin = position - new float3(0, 0, armSwingLength + (i - leftArmBegin) * armSwingLength),
                        positionEnd = position + new float3(0, 0, armSwingLength + (i - leftArmBegin) * armSwingLength),
                        loop = 1
                    });
                }
                // right arm animation
                if (i >= rightArmBegin && i <= rightArmBegin + 3)
                {
                    EntityManager.AddComponentData(bone, new PositionLerper
                    {
                        createdTime = UnityEngine.Time.time,
                        lifeTime = swingSpeed,
                        positionEnd = position - new float3(0, 0, armSwingLength + (i - rightArmBegin) * armSwingLength),
                        positionBegin = position + new float3(0, 0, armSwingLength + (i - rightArmBegin) * armSwingLength),
                        loop = 1
                    });
                }
                // right leg animation
                if (i >= rightLegBegin && i <= rightLegBegin + 2)
                {
                    EntityManager.AddComponentData(bone, new PositionLerper
                    {
                        createdTime = UnityEngine.Time.time,
                        lifeTime = swingSpeed,
                        positionBegin = position - new float3(0, 0, armSwingLength  / 4f+ (i - rightLegBegin) * armSwingLength),
                        positionEnd = position + new float3(0, 0, armSwingLength  / 4f + (i - rightLegBegin) * armSwingLength),
                        loop = 1
                    });
                }
                
                if (i >= leftLegBegin && i <= leftLegBegin + 2)
                {
                    EntityManager.AddComponentData(bone, new PositionLerper
                    {
                        createdTime = UnityEngine.Time.time,
                        lifeTime = swingSpeed,
                        positionEnd = position - new float3(0, 0, armSwingLength  / 4f+ (i - leftLegBegin) * armSwingLength),
                        positionBegin = position + new float3(0, 0, armSwingLength  / 4f + (i - leftLegBegin) * armSwingLength),
                        loop = 1
                    });
                }
            }
            // set proper parents for bone heirarchy rather then all relative to skeleton positions
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
            }
            for (int i = 0; i < boneList.Count; i++)
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