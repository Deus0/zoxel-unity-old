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
        public int3 influenceMin;
        public int3 influenceMax;
        public float3 position;           // position within chunk - used for skinning
        public Entity parent;           // parent bone
        public Entity skeleton;

        public float3 GetWorldPosition(EntityManager EntityManager, Entity boneEntity)
        {
            Matrix4x4 localToWorldPos = EntityManager.GetComponentData<LocalToWorld>(boneEntity).Value;
            Vector3 position = localToWorldPos.GetColumn(3);
            float3 position2 = position;
            return position2;
        }

        // used to increase bounding boxes for skinning
        public float3 GetInfluenceSize()
        {
            return 1.03f * ((influenceMax - influenceMin).ToFloat3() / 32f);
        }
        public float3 GetInfluenceSizeBase()
        {
            return 1f * ((influenceMax - influenceMin).ToFloat3() / 32f);
        }

        public float3 GetInfluenceOffset()
        {
            float3 midPoint = (GetInfluenceSizeBase() / 2f);
            float3 influenceOffset = (influenceMax.ToFloat3() / 32f - midPoint);
            return influenceOffset;
        }
    }

}