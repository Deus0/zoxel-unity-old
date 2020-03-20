using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Unity.Mathematics;

namespace Zoxel
{

    [DisableAutoCreation]
    public class DebugColliderSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities.WithAll<Body, Translation, Rotation>().ForEach((Entity e, ref Body body, ref Translation position, ref Rotation rotation) =>
            {
                DebugLines.DrawCubeLines(position.Value, rotation.Value, body.size, Color.black);
            });
        }

    }
}