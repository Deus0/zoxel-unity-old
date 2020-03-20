using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

namespace Zoxel
{
    public static class EntityUtilities
    {
        public static void SetParent(EntityManager dstManager, Entity parent, Entity child, float3 localTranslation, quaternion localRotation, float3 localScale)
        {
            //set the child
            if (!dstManager.HasComponent<LocalToWorld>(child))
                dstManager.AddComponentData(child, new LocalToWorld { });

            if (!dstManager.HasComponent<Translation>(child))
                dstManager.AddComponentData(child, new Translation { Value = localTranslation });
            else
                dstManager.SetComponentData(child, new Translation { Value = localTranslation });

            if (!dstManager.HasComponent<Rotation>(child))
                dstManager.AddComponentData(child, new Rotation { Value = (localRotation) });
            else
                dstManager.SetComponentData(child, new Rotation { Value = (localRotation) });

            if (!dstManager.HasComponent<NonUniformScale>(child))
                dstManager.AddComponentData(child, new NonUniformScale { Value = localScale });
            else
                dstManager.SetComponentData(child, new NonUniformScale { Value = localScale });

            if (!dstManager.HasComponent<Parent>(child))
                dstManager.AddComponentData(child, new Parent { Value = parent });
            else
                dstManager.SetComponentData(child, new Parent { Value = parent });

            if (!dstManager.HasComponent<LocalToParent>(child))
                dstManager.AddComponentData(child, new LocalToParent());

            //set the parent
            if (!dstManager.HasComponent<LocalToWorld>(parent))
                dstManager.AddComponentData(parent, new LocalToWorld { });

            if (!dstManager.HasComponent<Translation>(parent))
                dstManager.AddComponentData(parent, new Translation { Value = UnityEngine.Vector3.one });

            if (!dstManager.HasComponent<Rotation>(parent))
                dstManager.AddComponentData(parent, new Rotation { Value = quaternion.identity });

            if (!dstManager.HasComponent<NonUniformScale>(parent))
                dstManager.AddComponentData(parent, new NonUniformScale { Value = UnityEngine.Vector3.one });

        }

    }
}