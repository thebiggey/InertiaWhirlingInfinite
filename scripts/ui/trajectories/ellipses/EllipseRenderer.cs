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
        Vector3[] points = new Vector3[resolution];

        for(int i = 0; i < resolution; i++)
        {
            double theta = nMath.tau * (double)i / (double)(resolution - 1);

            points[i] = ellipse.Evaluate(theta);
        }

        SetPoints(points);
        Construct();
    }
}
