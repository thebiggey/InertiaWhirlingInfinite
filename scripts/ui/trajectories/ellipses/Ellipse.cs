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
        this.aDir = orbit.LocalToWorldSpace(-Vector2.Right);
        this.bDir = orbit.LocalToWorldSpace(Vector2.Up);

        this.centre = orbit.planetarySystem.GlobalPosition + (orbit.a * orbit.e * this.aDir);
        this.a = orbit.a;
        this.b = orbit.b;
    }

    public Vector3 Evaluate(double theta)
    {
        return centre + a * Math.Cos(theta) * aDir + b * Math.Sin(theta) * bDir;
    }
}
