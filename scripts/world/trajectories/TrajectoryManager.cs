using Godot;
using System;

public partial class TrajectoryManager : Node
{
    [Export] Trajectory trajectory;

    [ExportGroup("Parameters")]
    [Export] Node3D target;
    [Export] Vector3 startVelocity;
    [Export] PlanetarySystem startSystem;
    [Export] TrajectoryRenderer trajectoryRenderer;

    [ExportGroup("Controls")]
    [Export] bool update = false;

    double lastT;

    bool init = false;

    /*
        The way this is implemented architecturally is temporary. The way it's implemented now,
        is for making the basic loops easy to develop and test.
        This will be adapted at a later point to fit better with the overall architecture.
    */

    void UpdateTrajectory()
    {
        lastT = Clock.t;

        trajectory = new Trajectory();

        double startV;
        Orbit orbit = new Orbit(startSystem, new StateVector(target.Position, startVelocity), out startV);
        double endV = orbit.SphereIntersection();

        double startT = orbit.TrueAnomalyToTime(startV);
        double endT = orbit.TrueAnomalyToTime(endV);

        TrajectorySection section = new TrajectorySection(orbit, startT, endT);
        trajectory.Add(section);

        double time = Clock.t + section.duration;

        int k = 0;
        while(!double.IsNaN(endT) && !orbit.planetarySystem.isSunSystem && k < 5)
        {
            PlanetarySystem system = orbit.planetarySystem;
            PlanetarySystem parent = system.parentSystem;

            StateVector playerSV = orbit.GetStateVector(endT) + system.orbit.GetStateVector(time - system.startingT);

            orbit = new Orbit(parent, playerSV, out startV);
            endV = orbit.SphereIntersection();

            startT = orbit.TrueAnomalyToTime(startV);
            endT = orbit.TrueAnomalyToTime(endV);

            section = new TrajectorySection(orbit, startT, endT);
            trajectory.Add(section);

            time += section.duration;
            k++;
        }
    }

    void TraverseTrajectory()
    {
        double t = Clock.t - lastT;

        TrajectorySection section = trajectory.Get(0);

        while(t > section.duration)
        {
            lastT = Clock.t;
            t = 0;

            trajectory.Skip();
            section = trajectory.Get(0);

            section.orbit.planetarySystem.AddWorldChild(target);

            GD.Print("TRAJECTORY SKIP");
        }

        StateVector sv = section.Evaluate(t);

        target.Position = sv.position;
    }

    public override void _Ready()
    {
        init = false;
    }

    public override void _PhysicsProcess(double delta)
    {
        if(!init)
        {
            UpdateTrajectory();

            trajectoryRenderer.SetTrajectory(trajectory);

            trajectoryRenderer.Update();

            init = true;
        }

        if(update)
        {
            TraverseTrajectory();
        }
    }
}
