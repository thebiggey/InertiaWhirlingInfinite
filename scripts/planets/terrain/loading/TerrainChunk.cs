using System;
using Godot;

[Tool]
public partial class TerrainChunk : MeshInstance3D
{
    [Export] TerrainSettings terrainSettings;

    [Export] Vector3[] quad;

    (int, int, int) index;

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

        AddQuad(ref triangles, 0, quadIndices);

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

                AddQuad(ref triangles, k, q);
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

    private void AddQuad(ref int[] tris, int k, int[] quad)
    {
        tris[k + 0] = quad[2];
        tris[k + 1] = quad[1];
        tris[k + 2] = quad[0];

        tris[k + 3] = quad[0];
        tris[k + 4] = quad[3];
        tris[k + 5] = quad[2];
    }

    public override string ToString()
    {
        return $"Index: {index.Item1}, {index.Item2}, {index.Item3}; quad: 0: {quad[0]}, 1: {quad[1]}, 2: {quad[2]}, 3: {quad[3]}";
    }
}
