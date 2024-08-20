using System;
using System.Collections.Generic;
using Godot;

// Note:
// this sadly can't be done in C# using structs, as structs that aren't directly referenced by the root aren't saved in memory.
// This should be implemetable using structs in C++
public class DynamicQuad
{
    Vector3[] quad;
    int depth;
    TerrainChunk chunk;
    DynamicQuad parent;

    DynamicQuad[] children;

    public bool isLeaf => children.Length == 0;

    public DynamicQuad(Vector3[] quad, int depth, TerrainChunk chunk, DynamicQuad parent)
    {
        this.quad = quad;
        this.depth = depth;
        this.chunk = chunk;
        this.parent = parent;

        this.children = new DynamicQuad[0];
    }

    public int Generate(int maxDepth, double splitDistance, Vector3 target)
    {
        double sideLength = (quad[1] - quad[0]).Length();
        double radius = chunk.terrainSettings.radius;

        Vector3 centre = (quad[0] + quad[2]) * 0.5d;

        if(depth > maxDepth || target.DistanceTo(centre.Normalized() * radius) > splitDistance * sideLength * radius)
        {
            return 1;
        }

        Vector3[] newPoints = {
            (quad[0] + quad[1]) * 0.5d,
            (quad[1] + quad[2]) * 0.5d,
            (quad[2] + quad[3]) * 0.5d,
            (quad[3] + quad[0]) * 0.5d,
        };

        Vector3[] quad0 = { quad[0], newPoints[0], centre, newPoints[3] };
        Vector3[] quad1 = { newPoints[0], quad[1], newPoints[1], centre };
        Vector3[] quad2 = { centre, newPoints[1], quad[2], newPoints[2] };
        Vector3[] quad3 = { newPoints[3], centre, newPoints[2], quad[3] };

        this.children = new DynamicQuad[4];
        this.children[0] = new DynamicQuad(quad0, depth + 1, chunk, this);
        this.children[1] = new DynamicQuad(quad1, depth + 1, chunk, this);
        this.children[2] = new DynamicQuad(quad2, depth + 1, chunk, this);
        this.children[3] = new DynamicQuad(quad3, depth + 1, chunk, this);

        int c = 0;
        foreach(DynamicQuad child in children)
        {
            c += child.Generate(maxDepth, splitDistance, target);
        }

        return c;
    }

    public int Construct(int[] indices, List<Vector3> vertices, List<int> triangles)
    {
        //GD.Print(depth);
        //GD.Print(isLeaf);
        //GD.Print("+++");

        if(isLeaf)
        {
            MeshHelper.AddQuad(triangles, indices);
            return 1;
        }

        Vector3[] newPoints = {
            (quad[0] + quad[1]) * 0.5d,
            (quad[1] + quad[2]) * 0.5d,
            (quad[2] + quad[3]) * 0.5d,
            (quad[3] + quad[0]) * 0.5d,
        };

        Vector3 centre = (quad[0] + quad[2]) * 0.5d;

        int k = vertices.Count;

        vertices.AddRange(newPoints);
        vertices.Add(centre);

        int[] quad0 = { indices[0], k, k + 4, k + 3 };
        int[] quad1 = { k, indices[1], k + 1, k + 4 };
        int[] quad2 = { k + 4, k + 1, indices[2], k + 2 };
        int[] quad3 = { k + 3, k + 4, k + 2, indices[3] };

        int c = 0;

        c += children[0].Construct(quad0, vertices, triangles);
        c += children[1].Construct(quad1, vertices, triangles);
        c += children[2].Construct(quad2, vertices, triangles);
        c += children[3].Construct(quad3, vertices, triangles);

        return c;
    }


    public void Update(int maxDepth, double splitDistance, Vector3 target)
    {
        if(isLeaf)
        {
            Generate(maxDepth, splitDistance, target);
            return;
        }

        if(children[0].isLeaf && children[1].isLeaf && children[2].isLeaf && children[3].isLeaf)
        {
            double sideLength = (quad[1] - quad[0]).Length();
            double radius = chunk.terrainSettings.radius;
            Vector3 centre = (quad[0] + quad[2]) * 0.5d;

            if(target.DistanceTo(centre.Normalized() * radius) > splitDistance * sideLength * radius)
            {
                this.children = new DynamicQuad[0];

                if(parent != null)
                {
                    parent.Update(maxDepth, splitDistance, target);
                }
            }
        }

        foreach(DynamicQuad child in children)
        {
            child.Update(maxDepth, splitDistance, target);
        }

        /*
        A way to optimise this would be by recording whether the chunk actually gets updated at all.
        If not, we can just preserve the mesh. Since on a planet scale this should apply to most chunks on most frames,
        this should vastly improve performance.
        */
    }

    public override string ToString()
    {
        return $"quad: {quad[0]}, {quad[1]}, {quad[2]}, {quad[3]}; depth: {depth}";
    }
}
