using System;
using Godot;

[Tool]
public partial class LineRenderer : MeshInstance3D
{
    [ExportGroup("Parameters")]
    [Export] double size = 1;

    [ExportGroup("Controls")]
    [Export] bool update = false;
    [Export] bool autoUpdate = false;
    [Export] bool loop = false;
    [Export] bool global = false;

    Vector3[] points;

    public virtual void Update() { }

    public void SetPoints(Vector3[] points)
    {
        this.points = points;
    }

    public void Construct()
    {
        const double _tauOver3 = nMath.tau / 3;
        const double _2tauOver3 = (2 * nMath.tau) / 3;

        int n = points.Length;new Vector3(1, 0, -1)

        Vector3[] vertices = new Vector3[n * 3];
        int[] triangles = new int[(loop ? n : (n - 1)) * 18];

        for(int i = 0; i < n; i++)
        {
            Vector3 point = points[i] - (global ? GlobalPosition : Vector3.Zero);
            Vector3 dir = i < (n - 1) ? points[i + 1] - points[i] : points[0] - points[i];
            dir = dir.Normalized();

            Vector3 axis = dir.Cross(Vector3.Forward);
            if(axis == Vector3.Zero)
                axis = dir.Cross(Vector3.Right);
            else if(axis.Y < 0)
                axis *= -1;

            axis = axis.Normalized() * size;

            int k = i * 3;
            vertices[k] = point + axis;
            vertices[k + 1] = point + nMath.RotateVectorAround(axis, dir, _tauOver3);
            vertices[k + 2] = point + nMath.RotateVectorAround(axis, dir, _2tauOver3);
        }

        for(int i = 0; i < (n - 1); i++)
        {
            int k = i * 3;
            int j = i * 18;

            int[] quad0 = { k + 1, k + 4, k + 3, k };
            MeshHelper.AddQuad(ref triangles, j, quad0);
            j += 6;

            int[] quad1 = { k + 2, k + 5, k + 4, k + 1 };
            MeshHelper.AddQuad(ref triangles, j, quad1);
            j += 6;

            int[] quad2 = { k, k + 3, k + 5, k + 2 };
            MeshHelper.AddQuad(ref triangles, j, quad2);
        }

        if(loop)
        {
            int k = (n - 1) * 3;
            int j = (n - 1) * 18;

            int[] quad0 = { k + 1, 1, 0, k };
            MeshHelper.AddQuad(ref triangles, j, quad0);
            j += 6;

            int[] quad1 = { k + 2, 2, 1, k + 1 };
            MeshHelper.AddQuad(ref triangles, j, quad1);
            j += 6;

            int[] quad2 = { k, 0, 2, k + 2 };
            MeshHelper.AddQuad(ref triangles, j, quad2);
        }

        ArrayMesh arrayMesh = new ArrayMesh();

        Godot.Collections.Array arrays = new Godot.Collections.Array();
        arrays.Resize((int)Mesh.ArrayType.Max);

        arrays[(int)ArrayMesh.ArrayType.Vertex] = vertices;
        arrays[(int)ArrayMesh.ArrayType.Index] = triangles;

        arrayMesh.AddSurfaceFromArrays(ArrayMesh.PrimitiveType.Triangles, arrays);

        this.Mesh = arrayMesh;
    }

    public override void _PhysicsProcess(double delta)
    {
        if(update || autoUpdate)
        {
            update = false;

            Update();
            Construct();
        }
    }

}
