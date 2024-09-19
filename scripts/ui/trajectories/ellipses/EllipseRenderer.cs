using Godot;
using System;

[Tool]
public partial class EllipseRenderer : LineRenderer
{
    [Export] int resolution = 10;
    [Export] Ellipse ellipse;

    public void SetEllipse(Ellipse ellipse)
    {
        this.ellipse = ellipse;
    }

    public void SetEllipse(Orbit orbit)
    {
        SetEllipse(new Ellipse(orbit));
    }

    public override void Update()
    {
        this.normal = ellipse.ComputeNormal();
        Vector3[] points = ellipse.Sample(resolution);

        SetPoints(points);
        Construct();
    }
}
