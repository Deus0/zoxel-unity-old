using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GLTestQuads : MonoBehaviour
{
    // Draw red a rombus on the screen
    // and also draw a small cyan Quad in the left corner
    public Material mat;
    void OnPostRender()
    {
        if (!mat)
        {
            Debug.LogError("Please Assign a material on the inspector");
            return;
        }
        GL.PushMatrix();
        mat.SetPass(0);
        GL.LoadOrtho();
        GL.Begin(GL.QUADS);
        GL.Color(Color.red);
        GL.Vertex3(0, 0.5f, 0);
        GL.Vertex3(0.5f, 1, 0);
        GL.Vertex3(1, 0.5f, 0);
        GL.Vertex3(0.5f, 0, 0);

        GL.Color(Color.cyan);
        GL.Vertex3(0, 0, 0);
        GL.Vertex3(0, 0.25f, 0);
        GL.Vertex3(0.25f, 0.25f, 0);
        GL.Vertex3(0.25f, 0, 0);
        GL.End();
        GL.PopMatrix();
    }
}