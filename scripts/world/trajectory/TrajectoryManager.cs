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
    [Export] bool set = false;
    [Export] bool autoSet = false;

    double lastT;

    bool init = false;

    /*
        The way this is implemented architecturally is temporary. The way it's implemented now,
        is for making the basic loops easy to develop and test.
        This will be adapted at a later point to fit better with the overall architecture.
    */

    (double, PlanetarySystem) GetIntersections(TrajectorySection section, PlanetarySystem parentSystem)
    {
        const double m = 0.5d;
        const double dt_max_r = 0.1d;
        const double epsilon = 0.001d;
        const int N = 500;

        Orbit orbit = section.orbit;
        double startT = section.startT;
        double endT = section.endT;

        double apoapsis = orbit.Apoapsis;
        double periapsis = orbit.Periapsis;

        double t = startT;
        double dt_max = dt_max_r * section.duration;

        Godot.Collections.Array<PlanetarySystem> children = new Godot.Collections.Array<PlanetarySystem>();

        // filter all systems which are easily ignorable
        foreach(PlanetarySystem child in parentSystem.childSystems)
        {
            double r = child.systemRadius;

            if(child.orbit.Apoapsis > apoapsis + r || child.orbit.Periapsis < periapsis - r)
            {
                continue;
            }

            children.Add(child);
        }

        int l = children.Count;

        if(l == 0)
            return (double.NaN, null);


        int i = 0;
        while(i < N)
        {
            if(!section.isFull && t < endT)
            {
                break;
            }

            StateVector targetSV = orbit.GetStateVector(t);

            double[] dt_arr = new double[l];

            for(int j = 0; j < l; j++)
            {
                PlanetarySystem system = children[j];

                StateVector systemSV = system.orbit.GetStateVector(t + system.startingT);

                StateVector sv = systemSV - targetSV;

                double d = sv.position.Length() - system.systemRadius;

                if(d < epsilon)
                {
                    return (t, system);
                }

                // this calculates the velocity component along the position
                // basically this measures how fast the target is moving towards the system
                double a = nMath.VectorComponent(sv.velocity, sv.position);

                if(a == 0)
                {
                    dt_arr[j] = dt_max;
                }
                else
                {
                    dt_arr[j] = Math.Abs((d - system.systemRadius) / a);
                }

            }

            double dt = nMath.Min(dt_arr);

            t += Math.Min(dt * m, dt_max);
        }

        return (double.NaN, null);
    }

    void UpdateTrajectoryAdvanced()
    {
        lastT = Clock.t;

        trajectory = new Trajectory();

        double time = Clock.t;
        StateVector sv = new StateVector(target.Position, startVelocity);

        int k = 0;
        while(k < 5)
        {
            double startV;
            Orbit orbit = new Orbit(startSystem, sv, out startV);
            double endV = orbit.SphereIntersection();

            double startT = orbit.TrueAnomalyToTime(startV);
            double endT = orbit.TrueAnomalyToTime(endV);

            PlanetarySystem system = orbit.planetarySystem;
            PlanetarySystem parent = system.parentSystem;

            TrajectorySection section = new TrajectorySection(orbit, startT, endT);

            (double t_i, PlanetarySystem child) = GetIntersections(section, system);

            // an intersection was found
            // the trajectory will enter the child system
            if(!double.IsNaN(t_i))
            {
                endT = t_i;
                sv = orbit.GetStateVector(endT) - child.orbit.GetStateVector(time - child.startingT);

                section = new TrajectorySection(orbit, startT, endT);
            }
            // there was no intersection found, but the object exits the SOI
            // the trajectory will enter the parent system
            else if(!double.IsNaN(endT))
            {
                sv = orbit.GetStateVector(endT) + system.orbit.GetStateVector(time - system.startingT);
            }
            // there was no intersection found and the object does not exit the SOI
            // this is a full orbit and the trajectory ends here
            else
            {
                break;
            }


            trajectory.Add(section);

            time += section.duration;
            k++;
        }
    }

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

            StateVector playerSV = orbit.GetStateVector(endT) + system.orbit.GetStateVector(time + system.startingT);

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

            trajectoryRenderer.Skip();

            GD.Print("TRAJECTORYMANAGER: SKIP");
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

        /*
        if(set || autoSet)
        {
            set = false;

            UpdateTrajectory();

            trajectoryRenderer.SetTrajectory(trajectory);

            trajectoryRenderer.Update();
        }*/

        if(update && !Engine.IsEditorHint())
        {
            TraverseTrajectory();
        }
    }
}
