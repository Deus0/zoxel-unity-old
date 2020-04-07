using Unity.Entities;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Zoxel.Voxels;
using Unity.Collections;

namespace Zoxel
{
    public struct Skeleton : IComponentData
    {
        private static EntityArchetype boneArchtype;
        public BlitableArray<Entity> bones;
        public int3 size;
        public BlitableArray<int3> positions;
        public BlitableArray<int3> sizes;
        public quaternion mulRotation;
        public BlitableArray<byte> boneIndexes;

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

        public void SetBones(EntityManager EntityManager, Entity skeleton, int3[] bodyPositions, VoxData[] voxes, int3[] positions,int[] parents, byte[] axes)
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
                    typeof(Translation),
                    typeof(Rotation),
                    typeof(NonUniformScale),
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
                var vox = voxes[i];
                int3 halfSize = new int3((vox.size.x / 2), (vox.size.y / 2), (vox.size.z / 2));
                int3 minusHalfSize = new int3(-vox.size.x / 2, -vox.size.y / 2, -vox.size.z / 2);
                int3 bodyMidpoint = bodyPositions[i] + halfSize;
                int3 boneOffset = bodyMidpoint - positions[i];
                //minusHalfSize = new int3(minusHalfSize.x - (vox.size.x % 2), minusHalfSize.y, minusHalfSize.z - (vox.size.z % 2));
                //halfSize = new int3(halfSize.x + 1, halfSize.y + 1, halfSize.z + 1);
                //minusHalfSize = new int3(minusHalfSize.x - 1, minusHalfSize.y - 1, minusHalfSize.z - 1);
                //halfSize = new int3(halfSize.x + 1, halfSize.y, halfSize.z + 1);
                if ((SlotAxis)axes[i] == SlotAxis.Left || (SlotAxis)axes[i] == SlotAxis.Right)
                {
                    boneOffset = new int3(-boneOffset.x, -boneOffset.y, -boneOffset.z);
                    //minusHalfSize.x--;
                }
                minusHalfSize.x -= (vox.size.x % 2);
                //boneOffset.x -=  (vox.size.x % 2);
                Bone newBone = new Bone
                {
                    position = position,
                    skeleton = skeleton,
                    influenceMin = (boneOffset + minusHalfSize),
                    influenceMax = (boneOffset + halfSize)
                };
                EntityManager.SetComponentData(bone, newBone);
                EntityManager.SetComponentData(bone, new Parent { Value = skeleton });
                EntityManager.SetComponentData(bone, new Translation { Value = position });
                EntityManager.SetComponentData(bone, new Rotation { Value = quaternion.identity });
                EntityManager.SetComponentData(bone, new NonUniformScale { Value = new float3(1,1,1) });
                boneList.Add(bone);
            }
            bones = new BlitableArray<Entity>(boneList.Count, Unity.Collections.Allocator.Persistent);
            for (int i = 0; i < bones.Length; i++)
            {
                bones[i] = boneList[i];
            }
            if (Bootstrap.instance.isAddAnimations)
            {
                AddAnimations(EntityManager);
            }
        }

        public void BakeWeights(EntityManager EntityManager, NativeArray<ZoxelVertex> verticesN, int vertsCount)
        {
            var vertices = verticesN.ToArray();
            boneIndexes = new BlitableArray<byte>(vertsCount, Allocator.Persistent);
            for (int j = 0; j < boneIndexes.Length; j++)
            {
                boneIndexes[j] = 0;
            }
            for (int i = 0; i < bones.Length; i++)
            {
                var bone = EntityManager.GetComponentData<Bone>(bones[i]);
                var bonePosition = bone.position;
                float3 influenceSize = bone.GetInfluenceSize();
                float3 influenceOffset = bone.GetInfluenceOffset();

                /*float3 influenceSize = 1.0f * ((bone.influenceMax - bone.influenceMin).ToFloat3() / 32f);
                float3 midPoint = (influenceSize / 2f);
                float3 influenceOffset = (bone.influenceMax.ToFloat3() / 32f - midPoint);*/

                var influenceMin = bonePosition + influenceOffset - influenceSize / 2f;
                var influenceMax = bonePosition + influenceOffset + influenceSize / 2f;
                //Debug.LogError("Baking Weights " + i + " influenceOffset: " + influenceOffset + " influenceSize: " + influenceSize);
                for (int j = 0; j < vertsCount; j++)
                {
                    if (boneIndexes[j] == 0)
                    {
                        var vertPosition = vertices[j].position;
                        if (vertPosition.x >= influenceMin.x && vertPosition.x <= influenceMax.x
                            && vertPosition.y >= influenceMin.y && vertPosition.y <= influenceMax.y
                            && vertPosition.z >= influenceMin.z && vertPosition.z <= influenceMax.z)
                        {
                            //DebugLines.DrawCubeLines(vertPosition, skeletonRotation.Value, 0.5f * influenceSize, Color.red);
                            boneIndexes[j] = (byte)(i + 1);
                        }
                    }
                }
                // influence should have a minimum and max, a bounding box around the bone position
                    // this can be set in bones when generating and debugged with more box lines
                    //DrawDebugSphere(chunk.bones[i], influence);
                    // for each bone, fight weights within radius using vertexes
                }
                //influence += influenceAdd;
            //}

            for (int j = 0; j < boneIndexes.Length; j++)
            {
                if (boneIndexes[j] != 0)
                {
                    boneIndexes[j] = (byte)((int)boneIndexes[j] - 1);
                }
            }
        }

        private void AddAnimations(EntityManager EntityManager) 
        {
            int chestIndex = 0;
            int headIndex = 1;
            int hipsIndex = 2;
            int leftLegBegin = 3;
            int rightLegBegin = 6;
            int leftArmBegin = 9;
            int rightArmBegin = 13;
            float armSwingLength = 3 / 32f;
            float swingSpeed = 0.9f;
            for (int i = 0; i < bones.Length; i++) 
            {
                var bone = bones[i];
                var position = EntityManager.GetComponentData<Bone>(bone).position;
                // head bob
                if (i == headIndex)
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
        }
    }
}

        // start at core (first bone)
        // move to any children nodes
        // for all bones, if parent is this index, then set, and then add its child nodes
        // also set positions during this time as minus their parent one
        /*private void AddChildren(List<Entity> bones)
        {

        }*/

                // chest
                /*if (i == chestIndex)
                {
                    newBone.voxelInfluence = (byte) 22;
                }
                // head
                if (i == headIndex)
                {
                    newBone.voxelInfluence = (byte) 32;
                }
                // hips?
                if (i == hipsIndex)
                {
                    newBone.voxelInfluence = (byte) 22;
                }
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
                // feet
                if (i == leftLegBegin + 2 || i == rightLegBegin + 2)
                {
                    newBone.voxelInfluence = (byte) 16;
                }*/
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