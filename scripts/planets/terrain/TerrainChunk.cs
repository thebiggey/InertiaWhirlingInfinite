using System;
using System.Collections.Generic;
using Godot;

[Tool]
public partial class TerrainChunk : MeshInstance3D
{
    (int, int, int) index;
    [Export] public TerrainSettings terrainSettings;

    [Export] Vector3[] quad;

    DynamicQuad rootQuad;

    Noise noise;

    public void Initialise(TerrainSettings terrainSettings, (int, int, int) index, Vector3[] quad, Noise noise)
    {
        this.terrainSettings = terrainSettings;
        this.index = index;
        this.quad = quad;
        this.noise = noise;
    }

    private Vector3 GetPoint(double tx, double ty)
    {
        return quad[0] + (quad[1] - quad[0]) * tx + (quad[2] - quad[1]) * ty;
    }

    public void ConstructDebug()
    {
        ArrayMesh arrayMesh = new ArrayMesh();

        Godot.Collections.Array arrays = new Godot.Collections.Array();
        arrays.Resize((int)Mesh.ArrayType.Max);

        int[] triangles = new int[6];

        int[] quadIndices = { 0, 1, 2, 3 };

        MeshHelper.AddQuad(ref triangles, 0, quadIndices);

        arrays[(int)ArrayMesh.ArrayType.Vertex] = quad;
        arrays[(int)ArrayMesh.ArrayType.Index] = triangles;

        arrayMesh.AddSurfaceFromArrays(ArrayMesh.PrimitiveType.Triangles, arrays);

        this.Mesh = arrayMesh;
    }

    public void ConstructUniform(int resolution)
    {
        if(resolution <= 0) return;

        Vector3[] vertices = new Vector3[(resolution + 1) * (resolution + 1)];
        int[] triangles = new int[resolution * resolution * 6];

        int k = 0;

        for(int i = 0; i < resolution + 1; i++)
        {
        Vector3 centre = (quad[0] + quad[2]) * 0.5d;

            double tx = (double)i / (double)resolution;

            for(int j = 0; j < resolution + 1; j++)
            {
                double ty = (double)j / (double)resolution;

                Vector3 point = GetPoint(tx, ty);
                Vector3 pointOnSphere = point.Normalized();

                vertices[k] = pointOnSphere * (terrainSettings.radius + terrainSettings.noiseSettings.Evaluate(pointOnSphere, noise));

                k++;
            }
        }

        k = 0;

        for(int i = 0; i < resolution; i++)
        {
            for(int j = 0; j < resolution; j++)
            {
                int l = i * (resolution + 1) + j;
                int[] q = {
                    l,
                    l + resolution + 1,
                    l + resolution + 2,
                    l + 1
                };

                MeshHelper.AddQuad(ref triangles, k, q);
                k += 6;
            }
        }

        ArrayMesh arrayMesh = new ArrayMesh();

        Godot.Collections.Array arrays = new Godot.Collections.Array();
        arrays.Resize((int)Mesh.ArrayType.Max);

        arrays[(int)ArrayMesh.ArrayType.Vertex] = vertices;
        arrays[(int)ArrayMesh.ArrayType.Index] = triangles;

        arrayMesh.AddSurfaceFromArrays(ArrayMesh.PrimitiveType.Triangles, arrays);

        this.Mesh = arrayMesh;
    }

    public void ConstructDynamic(Vector3 target, int maxDepth, double splitDistance, double cullingAngle)
    {
        Vector3 centre = (quad[0] + quad[2]) * 0.5d;

        // This is a basic way to cull the chunk if it isn't visible by the player
        if(nMath.CosineTheoremAngle(target.Length(), centre.Length(), (target - centre).Length()) > cullingAngle)
        {
            this.Mesh = null;
            return;
        }

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        vertices.AddRange(quad);

        int[] rootquad = { 0, 1, 2, 3 };
        TerrainQuad root = new TerrainQuad(rootquad, this);

        root.GenerateChildren(0, maxDepth, splitDistance, target, vertices, triangles);

        for(int i = 0; i < vertices.Count; i++)
        {
            vertices[i] = vertices[i].Normalized() * terrainSettings.radius;
        }

        ArrayMesh arrayMesh = new ArrayMesh();

        Godot.Collections.Array arrays = new Godot.Collections.Array();
        arrays.Resize((int)Mesh.ArrayType.Max);

        arrays[(int)ArrayMesh.ArrayType.Vertex] = vertices.ToArray();
        arrays[(int)ArrayMesh.ArrayType.Index] = triangles.ToArray();

        arrayMesh.AddSurfaceFromArrays(ArrayMesh.PrimitiveType.Triangles, arrays);

        this.Mesh = arrayMesh;
    }

    public void GenerateTree(Vector3 target, int maxDepth, double splitDistance)
    {
        rootQuad = new DynamicQuad(quad, 0, this, null);

        rootQuad.Generate(maxDepth, splitDistance, target);
    }

    public void UpdateTree(int maxDepth, Vector3 target, double splitDistance)
    {
        rootQuad.Update(maxDepth, splitDistance, target);
    }

    public void ConstructMesh(Vector3 target, double cullingAngle)
    {
        Vector3 centre = (quad[0] + quad[2]) * 0.5d;

        // This is a basic way to cull the chunk if it isn't visible by the player
        if(nMath.CosineTheoremAngle(target.Length(), centre.Length(), (target - centre).Length()) > cullingAngle)
        {
            this.Mesh = null;
            return;
        }

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        vertices.AddRange(quad);

        int[] rootIndices = { 0, 1, 2, 3 };

        rootQuad.Construct(rootIndices, vertices, triangles);

        for(int i = 0; i < vertices.Count; i++)
        {
            vertices[i] = vertices[i].Normalized() * terrainSettings.radius;
        }

        ArrayMesh arrayMesh = new ArrayMesh();
        Godot.Collections.Array arrays = new Godot.Collections.Array();
        arrays.Resize((int)Mesh.ArrayType.Max);

        arrays[(int)ArrayMesh.ArrayType.Vertex] = vertices.ToArray();
        arrays[(int)ArrayMesh.ArrayType.Index] = triangles.ToArray();

        arrayMesh.AddSurfaceFromArrays(ArrayMesh.PrimitiveType.Triangles, arrays);

        this.Mesh = arrayMesh;
    }

    public override string ToString()
    {
        return $"Index: {index.Item1}, {index.Item2}, {index.Item3}; quad: 0: {quad[0]}, 1: {quad[1]}, 2: {quad[2]}, 3: {quad[3]}";
    }
}
