using Godot;
using System;

[Tool]
public partial class Ellipse : Resource
{
    [Export] Vector3 centre;
    [Export] double a;
    [Export] double b;
    [Export] Vector3 aDir;
    [Export] Vector3 bDir;

    public Ellipse()
    {
        this.centre = Vector3.Zero;
        this.a = 1;
        this.b = 1;
        this.aDir = Vector3.Forward;
        this.bDir = Vector3.Right;
    }

    public Ellipse(Vector3 centre, double a, double b, Vector3 aDir, Vector3 bDir)
    {
        this.centre = centre;
        this.a = a;
        this.b = b;
        this.aDir = aDir;
        this.bDir = bDir;
    }

    public Ellipse(Orbit orbit)
    {
        if(!orbit.isElliptical)
        {
            GD.PrintErr("Cannot convert orbit to ellipse, as it is hyperbolic!");
        }

        this.aDir = orbit.LocalToWorldSpace(-Vector2.Right).Normalized();
        this.bDir = orbit.LocalToWorldSpace(Vector2.Up).Normalized();

        this.centre = orbit.planetarySystem.GlobalPosition + (orbit.a * orbit.e * this.aDir);
        this.a = orbit.a;
        this.b = orbit.b;
    }

    public virtual Vector3 Evaluate(double theta)
    {
        return centre + a * Math.Cos(theta) * aDir + b * Math.Sin(theta) * bDir;
    }

    public Vector3[] Sample(int resolution)
    {
        Vector3[] points = new Vector3[resolution];

        Sample(resolution, points, 0);

        return points;
    }

    public void Sample(int resolution, Vector3[] arr, int k)
    {
        for(int i = 0; i < resolution; i++)
        {
            double theta = nMath.tau * (double)i / (double)(resolution - 1);

            arr[i + k] = Evaluate(theta);
        }
    }
}
