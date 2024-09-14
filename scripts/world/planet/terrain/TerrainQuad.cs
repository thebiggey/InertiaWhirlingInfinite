using System;
using System.Collections.Generic;
using Godot;

public struct TerrainQuad
{
    int[] quad;
    TerrainQuad[] children;

    TerrainChunk chunk;

    public TerrainQuad(int[] quad, TerrainChunk chunk)
    {
        this.quad = quad;
        this.chunk = chunk;
        this.children = new TerrainQuad[0];
    }

    public void GenerateChildren(int depth, int maxDepth, double splitDistance, Vector3 target, List<Vector3> vertices, List<int> triangles)
    {
        Vector3[] points = {
            vertices[quad[0]],
            vertices[quad[1]],
            vertices[quad[2]],
            vertices[quad[3]],
        };

        double sideLength = (points[1] - points[0]).Length();
        double radius = chunk.terrainSettings.radius;

        Vector3 centre = (points[0] + points[2]) * 0.5d;

        if(depth > maxDepth || target.DistanceTo(centre.Normalized() * radius) > splitDistance * sideLength * radius)
        {
            MeshHelper.AddQuad(triangles, quad);
            return;
        }

        Vector3[] newPoints = {
            (points[0] + points[1]) * 0.5d,
            (points[1] + points[2]) * 0.5d,
            (points[2] + points[3]) * 0.5d,
            (points[3] + points[0]) * 0.5d,
        };

        int k = vertices.Count;

        vertices.AddRange(newPoints);
        vertices.Add(centre);

        int[] quad0 = { quad[0], k, k + 4, k + 3 };
        int[] quad1 = { k, quad[1], k + 1, k + 4 };
        int[] quad2 = { k + 4, k + 1, quad[2], k + 2 };
        int[] quad3 = { k + 3, k + 4, k + 2, quad[3] };

        children = new TerrainQuad[4];
        children[0] = new TerrainQuad(quad0, chunk);
        children[1] = new TerrainQuad(quad1, chunk);
        children[2] = new TerrainQuad(quad2, chunk);
        children[3] = new TerrainQuad(quad3, chunk);

        foreach(TerrainQuad child in children)
        {
            child.GenerateChildren(depth + 1, maxDepth, splitDistance, target, vertices, triangles);
        }
    }
}
