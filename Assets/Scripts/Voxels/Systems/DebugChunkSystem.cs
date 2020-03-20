using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Unity.Mathematics;

namespace Zoxel.Voxels
{
    [DisableAutoCreation]
    public class DebugChunkSystem : ComponentSystem
    {
        float timing = 0.01f;
        Color lineColor = Color.green;

        protected override void OnUpdate()
        {
            if (Application.isPlaying)
            {
                Entities.WithAll<Chunk>().ForEach((Entity e, ref Chunk chunk) =>
                {
                    DrawCubeLines(chunk.GetVoxelPosition().ToFloat3(), chunk.Value.voxelDimensions.ToFloat3());
                });
            }
    }

        private void DrawCubeLines(float3 position, float3 size)
        {
            DrawQuadLines(position + new float3(0, -size.y, 0), size);
            DrawQuadLines(position + new float3(0, size.y, 0), size);
            // draw 4 lines instead of these
            DrawQuadLines3(position + new float3(-size.x, 0, 0), size);
            DrawQuadLines3(position + new float3(size.x, 0, 0), size);
            DrawQuadLines2(position + new float3(0, 0, -size.z), size);
            DrawQuadLines2(position + new float3(0, 0, size.z), size);
        }

        private void DrawQuadLines(float3 position, float3 size)
        {
            DrawQuadLinesCore(position, size,
                new float3(-size.x, 0, size.z),
                new float3(size.x, 0, size.z),
                new float3(size.x, 0, -size.z),
                new float3(-size.x, 0, -size.z));
        }

        private void DrawQuadLines2(float3 position, float3 size)
        {
            DrawQuadLinesCore(position, size,
                new float3(-size.x, size.y, 0),
                new float3(size.x, size.y, 0),
                new float3(size.x, -size.y, 0),
                new float3(-size.x, -size.y, 0));
        }

        private void DrawQuadLines3(float3 position, float3 size)
        {
            DrawQuadLinesCore(position, size,
                new float3(0, -size.y, size.z),
                new float3(0, size.y, size.z),
                new float3(0, size.y, -size.z),
                new float3(0, -size.y, -size.z));
        }

        private void DrawQuadLinesCore(float3 position, float3 size, float3 cornerA, float3 cornerB,
            float3 cornerC, float3 cornerD)
        {
            float3 pointA = position + cornerA;
            float3 pointB = position + cornerB;
            float3 pointC = position + cornerC;
            float3 pointD = position + cornerD;

            // rotate the points

            Debug.DrawLine(pointA, pointB, lineColor, timing);
            Debug.DrawLine(pointB, pointC, lineColor, timing);
            Debug.DrawLine(pointC, pointD, lineColor, timing);
            Debug.DrawLine(pointD, pointA, lineColor, timing);
        }
    }
}