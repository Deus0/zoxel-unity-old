using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Unity.Mathematics;

namespace Zoxel
{
    [DisableAutoCreation]
    public class DebugBoneSystem : ComponentSystem
    {
        public static float boneDebugSize = 1 / 32f;
        public static float shrinkSize = 64;

        protected override void OnUpdate()
        {
            // make bones keep int3 position
            // make them reposition off their parent bones
            Entities.WithAll<Skeleton, Translation, Rotation, LocalToWorld>().ForEach((Entity e, ref Skeleton skeleton, ref Translation translation, ref Rotation rotation, ref LocalToWorld localToWorld) =>
            {
                quaternion rot = rotation.Value;
                Quaternion rot2 = Quaternion.Euler(0, 180, 0);
                quaternion rot3 = rot2;
                rot = math.mul(rot, rot3);
                skeleton.mulRotation = rot;
                float3 totalSize = skeleton.size.ToFloat3() / 32f;
                DebugLines.DrawCubeLines(translation.Value, rotation.Value, totalSize * 0.5f, Color.black);
                for (int i = 0; i < skeleton.positions.Length; i++)
                {
                    float3 newSize = skeleton.sizes[i].ToFloat3() / 32f;
                    float3 newPosition = skeleton.positions[i].ToFloat3() / 32f;
                    newPosition -= totalSize / 2f;
                    newPosition += newSize / 2f;
                    newPosition = math.rotate(rot, newPosition);
                    DebugLines.DrawCubeLines(translation.Value + newPosition, rotation.Value, newSize * 0.5f, Color.grey);
                }
            });

            Entities.WithAll<Bone>().ForEach((Entity e, ref Bone bone) =>
            {
                if (World.EntityManager.Exists(bone.skeleton) && World.EntityManager.HasComponent<Skeleton>(bone.skeleton))
                {
                    var skeleton = World.EntityManager.GetComponentData<Skeleton>(bone.skeleton);
                    var skeletonRotation = World.EntityManager.GetComponentData<Rotation>(bone.skeleton);
                    var skeletonPosition = World.EntityManager.GetComponentData<Translation>(bone.skeleton);
                    var newPosition = math.rotate(skeletonRotation.Value, bone.position);
                    DebugLines.DrawCubeLines(skeletonPosition.Value + newPosition, skeleton.mulRotation,
                                new float3(boneDebugSize, boneDebugSize, boneDebugSize), Color.red);
                }
            });
            
            Entities.WithAll<Bone, LocalToWorld>().ForEach((Entity e, ref Bone bone, ref LocalToWorld localToWorld) =>
            {
                if (World.EntityManager.Exists(bone.skeleton) && World.EntityManager.HasComponent<Skeleton>(bone.skeleton))
                {
                    Matrix4x4 localToWorldPos = localToWorld.Value;
                    Vector3 position = localToWorldPos.GetColumn(3);
                    var skeleton = World.EntityManager.GetComponentData<Skeleton>(bone.skeleton);
                    var skeletonRotation = World.EntityManager.GetComponentData<Rotation>(bone.skeleton);
                    //var newPosition = math.rotate(skeleton.mulRotation, position);
                    DebugLines.DrawCubeLines(position, skeletonRotation.Value,
                                1.2f * (new float3(boneDebugSize, boneDebugSize, boneDebugSize)), Color.green);
                }
            });
        }

    }
}

                /*if (World.EntityManager.Exists(parent.Value) && World.EntityManager.HasComponent<LocalToWorld>(parent.Value))
                {
                    Matrix4x4 localToWorldPos = localToWorld.Value;
                    Vector3 position3 = localToWorldPos.GetColumn(3);
                    float3 position2 = position3;
                    var rot = localToWorldPos.rotation;
                    //position2 = math.rotate(skeleton.mulRotation, position2);
                    DebugLines.DrawCubeLines(position2, rot,
                            new float3(boneDebugSize, boneDebugSize, boneDebugSize), Color.red);
                    Matrix4x4 parentLocalToWorld = World.EntityManager.GetComponentData<LocalToWorld>(parent.Value).Value;
                    Vector3 parentLocalToWorldPos = parentLocalToWorld.GetColumn(3);
                    Debug.DrawLine(position2, parentLocalToWorldPos, Color.red);
                    //position.Value = position2;
                }*/
//
//float3 position2 = math.transform(parentLocalToWorld.Value, position.Value);
/*var parentTranslation = World.EntityManager.GetComponentData<Translation>(parent.Value);
var parentRotation = World.EntityManager.GetComponentData<Rotation>(parent.Value);
var parentScale = World.EntityManager.GetComponentData<NonUniformScale>(parent.Value);*/



// scale it
//position2 = new float3(parentScale.Value.x * position2.x, parentScale.Value.y * position2.y, parentScale.Value.z * position2.z);
//position2 = 0.5f * position2;
// rotate it too
//position2 = math.rotate(parentRotation.Value, position2);
// addition
//position2 += parentTranslation.Value;
//position2 += new float3(0, 1, 0);
/*;*/



                /*if (World.EntityManager.Exists(parent.Value) && World.EntityManager.HasComponent<LocalToWorld>(parent.Value))
                {
                    Matrix4x4 localToWorldPos = localToWorld.Value;
                    Vector3 position3 = localToWorldPos.GetColumn(3);
                    float3 position2 = position3;
                    var rot = localToWorldPos.rotation;
                    //position2 = math.rotate(skeleton.mulRotation, position2);
                    DebugLines.DrawCubeLines(position2, rot,
                            new float3(boneDebugSize, boneDebugSize, boneDebugSize), Color.red);
                    Matrix4x4 parentLocalToWorld = World.EntityManager.GetComponentData<LocalToWorld>(parent.Value).Value;
                    Vector3 parentLocalToWorldPos = parentLocalToWorld.GetColumn(3);
                    Debug.DrawLine(position2, parentLocalToWorldPos, Color.red);
                    //position.Value = position2;
                }
                else {
                    Debug.LogError("Bone doesn't exist at: " + bone.localPosition);
                }*/