using Unity.Mathematics;
using UnityEngine;

namespace Zoxel
{
    public static class MeshUtilities
    {

        public static Mesh CreateQuadMesh(float2 size)
        {
            float healthBackBuffer = 0;
            Mesh mesh = new Mesh();
            Vector3[] newVerts = new Vector3[4];
            newVerts[0] = new Vector3(-0.5f * size.x - healthBackBuffer, -0.5f * size.y - healthBackBuffer, 0);
            newVerts[1] = new Vector3(0.5f * size.x + healthBackBuffer, -0.5f * size.y - healthBackBuffer, 0);
            newVerts[2] = new Vector3(0.5f * size.x + healthBackBuffer, 0.5f * size.y + healthBackBuffer, 0);
            newVerts[3] = new Vector3(-0.5f * size.x - healthBackBuffer, 0.5f * size.y + healthBackBuffer, 0);
            mesh.vertices = newVerts;
            int[] indices = new int[6];

            indices[0] = 2;
            indices[1] = 1;
            indices[2] = 0;
            indices[3] = 3;
            indices[4] = 2;
            indices[5] = 0;

            /*indices[0] = 0;
            indices[1] = 1;
            indices[2] = 2;
            indices[3] = 0;
            indices[4] = 2;
            indices[5] = 3;*/

            mesh.SetIndices(indices, MeshTopology.Triangles, 0);
            Vector2[] uvs = new Vector2[4];
            uvs[0] = new Vector2(0, 0);
            uvs[1] = new Vector2(1, 0);
            uvs[2] = new Vector2(1, 1);
            uvs[3] = new Vector2(0, 1);
            mesh.uv = uvs;
            mesh.colors = new Color[] { Color.white, Color.white, Color.white, Color.white };
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            return mesh;
        }
        public static Mesh CreateReverseQuadMesh(float2 size, float zbuffer)
        {
            float healthBackBuffer = 0;
            //float zbuffer = 0.1f;
            Mesh mesh = new Mesh();
            Vector3[] newVerts = new Vector3[4];
            newVerts[0] = new Vector3(-0.5f * size.x - healthBackBuffer, -0.5f * size.y - healthBackBuffer, zbuffer);
            newVerts[1] = new Vector3(0.5f * size.x + healthBackBuffer, -0.5f * size.y - healthBackBuffer, zbuffer);
            newVerts[2] = new Vector3(0.5f * size.x + healthBackBuffer, 0.5f * size.y + healthBackBuffer, zbuffer);
            newVerts[3] = new Vector3(-0.5f * size.x - healthBackBuffer, 0.5f * size.y + healthBackBuffer, zbuffer);
            mesh.vertices = newVerts;
            int[] indices = new int[6];
            indices[0] = 0;
            indices[1] = 1;
            indices[2] = 2;
            indices[3] = 0;
            indices[4] = 2;
            indices[5] = 3;
            mesh.SetIndices(indices, MeshTopology.Triangles, 0);
            Vector2[] uvs = new Vector2[4];
            uvs[0] = new Vector2(0, 0);
            uvs[1] = new Vector2(1, 0);
            uvs[2] = new Vector2(1, 1);
            uvs[3] = new Vector2(0, 1);
            mesh.uv = uvs;
            mesh.colors = new Color[] { Color.white, Color.white, Color.white, Color.white };
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            return mesh;
        }
    }
}