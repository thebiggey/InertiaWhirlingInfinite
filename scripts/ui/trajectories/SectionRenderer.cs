using Godot;
using System;

[Tool]
public partial class SectionRenderer : LineRenderer
{
    [Export] int resolution = 30;

    [Export] TrajectorySection section;

    public SectionRenderer()
    {
        this.resolution = 30;
        this.section = new TrajectorySection();
    }

    public SectionRenderer(int resolution, double size, TrajectorySection section)
    {
        this.resolution = resolution;
        this.size = size;
        this.section = section;
    }

    public void SetSection(TrajectorySection section)
    {
        this.section = section;
    }

    public void SetSection(Orbit orbit)
    {
        this.section = new TrajectorySection(orbit, 0, double.NaN);
    }

    public override void Update()
    {
        Vector3[] points = new Vector3[resolution];

        Orbit orbit = section.orbit;
        double startV, endV;

        this.normal = orbit.Normal;

        if(section.isFull)
        {
            startV = 0;
            endV = nMath.tau;
        }
        else
        {
            startV = orbit.TimeToTrueAnomaly(section.startT);
            endV = orbit.TimeToTrueAnomaly(section.endT);
        }

        double v = startV;

        double dv = (endV - startV);
        if(dv < 0)
            dv += nMath.tau;

        dv /= (resolution - 1);

        for(int i = 0; i < resolution; i++)
        {
            points[i] = orbit.Sample(v);
            v += dv;
        }

        SetPoints(points);
        Construct();
    }
}
