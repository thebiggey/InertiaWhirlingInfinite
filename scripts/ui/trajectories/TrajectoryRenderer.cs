using Godot;
using System;

[Tool]
public partial class TrajectoryRenderer : Node
{
    [Export] int resolution = 20;

    [Export] double size = 5;

    [Export] Trajectory trajectory;

    SectionRenderer[] renderers = new SectionRenderer[0];

    public void SetTrajectory(Trajectory trajectory)
    {
        this.trajectory = trajectory;
    }

    public void Clear()
    {
        for(int i = 0; i < renderers.Length; i++)
        {
            renderers[i].QueueFree();
        }
    }

    public void Update()
    {
        Clear();

        int n = trajectory.Length;

        renderers = new SectionRenderer[n];

        for(int i = 0; i < n; i++)
        {
            TrajectorySection section = trajectory.Get(i);

            SectionRenderer renderer = new SectionRenderer(resolution, size, section);

            section.orbit.planetarySystem.AddWorldChild(renderer);
            renderers[i] = renderer;

            renderer.Update();
        }
    }

    public void Skip()
    {
        if(renderers.Length > 0)
        {
            renderers[0].QueueFree();
        }
    }
}
