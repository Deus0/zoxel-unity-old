using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Unity.Mathematics;

namespace Zoxel
{
    [DisableAutoCreation]
    public class DebugBoneSystem : ComponentSystem
    {

        protected override void OnUpdate()
        {
            Entities.WithAll<Bone, Translation, Rotation, Parent>().ForEach((Entity e, ref Bone bone, ref Translation position, ref Rotation rotation, ref Parent parent) =>
            {
                if (World.EntityManager.Exists(parent.Value) && World.EntityManager.HasComponent<Translation>(parent.Value))
                {
                    DebugLines.DrawCubeLines(position.Value, rotation.Value, new float3(0.1f, 0.1f, 0.1f), Color.cyan);
                    Debug.DrawLine(position.Value,
                        World.EntityManager.GetComponentData<Translation>(parent.Value).Value, Color.red);
                }
            });
        }

    }
}
