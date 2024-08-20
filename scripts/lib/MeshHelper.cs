using System;
using System.Collections.Generic;
using Godot;

public class MeshHelper
{
    public static void AddQuad(ref int[] tris, int k, int[] quad)
    {
        tris[k + 0] = quad[2];
        tris[k + 1] = quad[1];
        tris[k + 2] = quad[0];

        tris[k + 3] = quad[0];
        tris[k + 4] = quad[3];
        tris[k + 5] = quad[2];
    }

    public static void AddQuad(List<int> tris, int[] quad)
    {
        tris.Add(quad[2]);
        tris.Add(quad[1]);
        tris.Add(quad[0]);

        tris.Add(quad[0]);
        tris.Add(quad[3]);
        tris.Add(quad[2]);
    }

    public static void PrintFull(Vector3[] vertices, int[] triangles)
    {
        GD.Print("Vertices: ");
        foreach(Vector3 v in vertices)
        {
            GD.Print(v);
        }

        GD.Print("triangles: ");
        foreach(int i in triangles)
        {
            GD.Print(i);
        }

        GD.Print("----------------------------------");
    }

    public static void PrintFull(List<Vector3> vertices, List<int> triangles)
    {
        PrintFull(vertices.ToArray(), triangles.ToArray());
    }
}
