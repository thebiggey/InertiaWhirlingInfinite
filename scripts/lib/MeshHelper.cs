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
}
