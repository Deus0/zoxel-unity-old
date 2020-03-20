using UnityEngine;
using Unity.Mathematics;

namespace Zoxel
{
    public static class DebugLines
    {
        public static void DrawCubeLines(float3 position, quaternion rotation, float3 size, Color color, float timing = 0)
        {
            float3 positionOffset = position;
            DrawQuadLines(positionOffset, new float3(0, -size.y, 0), rotation, size, color, timing);
            DrawQuadLines(positionOffset, new float3(0, size.y, 0),  rotation, size, color, timing);
            // draw 4 lines instead of these
            DrawQuadLines3(positionOffset, new float3(-size.x, 0, 0),  rotation, size, color, timing);
            DrawQuadLines3(positionOffset, new float3(size.x, 0, 0),  rotation, size, color, timing);
            DrawQuadLines2(positionOffset, new float3(0, 0, -size.z),  rotation, size, color, timing);
            DrawQuadLines2(positionOffset, new float3(0, 0, size.z),  rotation, size, color, timing);
        }

        public static void DrawQuadLines(float3 positionOffset, float3 position, quaternion rotation, float3 size, Color color, float timing = 0)
        {
            DrawQuadLinesCore(positionOffset, position, rotation, size,
                new float3(-size.x, 0, size.z),
                new float3(size.x, 0, size.z),
                new float3(size.x, 0, -size.z),
                new float3(-size.x, 0, -size.z), color, timing);
        }

        public static void DrawQuadLines2(float3 positionOffset, float3 position, quaternion rotation, float3 size, Color color, float timing = 0)
        {
            DrawQuadLinesCore(positionOffset, position, rotation, size,
                new float3(-size.x, size.y, 0),
                new float3(size.x, size.y, 0),
                new float3(size.x, -size.y, 0),
                new float3(-size.x, -size.y, 0), color, timing);
        }

        public static void DrawQuadLines3(float3 positionOffset, float3 position, quaternion rotation, float3 size, Color color, float timing = 0)
        {
            DrawQuadLinesCore(positionOffset, position, rotation, size,
                new float3(0, -size.y, size.z),
                new float3(0, size.y, size.z),
                new float3(0, size.y, -size.z),
                new float3(0, -size.y, -size.z), color, timing);
        }

        public static void DrawQuadLinesCore(float3 positionOffset, float3 position, quaternion rotation, float3 size, float3 cornerA, float3 cornerB,
            float3 cornerC, float3 cornerD, Color color, float timing = 0)
        {
            float3 pointA = position + cornerA;
            float3 pointB = position + cornerB;
            float3 pointC = position + cornerC;
            float3 pointD = position + cornerD;
            pointA = math.mul(rotation, pointA);
            pointB = math.mul(rotation, pointB);
            pointC = math.mul(rotation, pointC);
            pointD = math.mul(rotation, pointD);
            pointA += positionOffset;
            pointB += positionOffset;
            pointC += positionOffset;
            pointD += positionOffset;
            Debug.DrawLine(pointA, pointB, color, timing);
            Debug.DrawLine(pointB, pointC, color, timing);
            Debug.DrawLine(pointC, pointD, color, timing);
            Debug.DrawLine(pointD, pointA, color, timing);
        }
    }

}