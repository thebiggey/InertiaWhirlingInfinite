using Godot;
using System;

public partial class TrajectoryRenderer : LineRenderer
{
    [Export] int resolution = 20;

    [Export] Trajectory trajectory;

    public void SetTrajectory(Trajectory trajectory)
    {
        this.trajectory = trajectory;
    }

    public override void Update()
    {
        int n = resolution * trajectory.Length;
        int k = 0;

        for(int i = 0; i < trajectory.Length; i++)
        {
            TrajectorySection section = trajectory.Get(i);

            Vector3[] points = new Vector3[n];

            if(section.isFull)
            {
                Ellipse ellipse = new Ellipse(section.orbit);

                ellipse.Sample(resolution, points, k);
            }
            else
            {
                Orbit orbit = section.orbit;

                double startV = orbit.TimeToTrueAnomaly(section.startT);
                double endV = orbit.TimeToTrueAnomaly(section.endT);

                points = new Vector3[resolution];

                for(int j = 0; j < resolution; j++)
                {
                    double r = (double)j / (double)resolution;
                    points[k] = orbit.GetStateVectorFromTruenomaly((endV - startV) * r + startV).position + orbit.planetarySystem.GlobalPosition;
                    k++;
                }
            }

            SetPoints(points);
            Construct();
        }
    }
}
