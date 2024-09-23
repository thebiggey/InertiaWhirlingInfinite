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
    [Export] IntersectionVisualiser intervis;

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
    (double, PlanetarySystem) GetIntersections(TrajectorySection section, double time)
    {
        const double m = 0.25d;
        const double dt_min = 0.000000001d;
        const double dt_max_r = 0.2d;
        const double epsilon = 0.0001d;
        const int N = 500;
        const int p = 3;

        Orbit orbit = section.orbit;
        double startT = section.startT;
        double endT = section.endT;

        double apoapsis = orbit.Apoapsis;
        double periapsis = orbit.Periapsis;

        double t = startT;
        double dt_max = dt_max_r * orbit.T;

        double glob_t = time;

        PlanetarySystem parentSystem = orbit.planetarySystem;

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

        GD.Print($"---------------------");
        GD.Print($"Starting Intersector: {parentSystem.Name}");
        GD.Print($"startT: {startT}");
        GD.Print($"---------------------");

        Godot.Collections.Array<Vector3> positions = new Godot.Collections.Array<Vector3>();

        int i = 0;
        while(i < N)
        {
            if(!section.isFull)
            {
                if(t < endT)
                    break;
            }
            else
            {
                if((t - startT) > p * orbit.T)
                    break;
            }

            StateVector targetSV = orbit.GetStateVector(t);

            positions.Add(targetSV.position);


            double[] dt_arr = new double[l];

            for(int j = 0; j < l; j++)
            {
                PlanetarySystem system = children[j];
                StateVector systemSV = system.GetStateVector(glob_t + t - startT);

                positions.Add(systemSV.position);

                StateVector sv = systemSV - targetSV;

                double d = sv.position.Length() - (system.systemRadius);

                if(Math.Abs(d) < epsilon)
                {
                    GD.Print($"Intersection found; t: {t}, d: {d}, sys: {system.Name}");
                    intervis.Visualise(parentSystem, positions);
                    return (t, system);
                }

                // this calculates the velocity component along the position
                // basically this measures how fast the target is moving towards the system
                double a = nMath.VectorComponent(sv.velocity, sv.position);

                //GD.Print($"d: {d}");
                //GD.Print($"a: {a}");
                //GD.Print($"ratio: {d / a}");

                if(a == 0)
                {
                    dt_arr[j] = dt_max;
                }
                else
                {
                    dt_arr[j] = Math.Abs(d / a);
                }

            }

            double dt = Math.Max(dt_min, nMath.Min(dt_arr)) * m;

            GD.Print($"dt: {dt}");

            GD.Print($"i: {i}, t: {t}, dt: {dt}");
            GD.Print("---------------------");

            intervis.Visualise(parentSystem, positions);

            t += Math.Min(dt, dt_max);
            i++;
        }

        GD.Print("No intersection found!");
        return (double.NaN, null);
    }

    void UpdateTrajectory()
    {
        lastT = Clock.t;

        trajectory = new Trajectory();

        double time = Clock.t;
        StateVector sv = new StateVector(target.Position, startVelocity);
        PlanetarySystem system = startSystem;

        int k = 0;
        while(k < 5)
        {
            double startV;
            Orbit orbit = new Orbit(system, sv, out startV);
            double endV = orbit.SphereIntersection();

            double startT = orbit.TrueAnomalyToTime(startV);
            double endT = orbit.TrueAnomalyToTime(endV);

            TrajectorySection section = new TrajectorySection(orbit, startT, endT);

            (double t_i, PlanetarySystem child) = GetIntersections(section, time);

            // an intersection was found
            // the trajectory will enter the child system
            if(!double.IsNaN(t_i))
            {
                endT = t_i;

                section = new TrajectorySection(orbit, startT, endT);

                sv = orbit.GetStateVector(endT) - child.GetStateVector(time + section.duration);

                system = child;
                GD.Print("");
                GD.Print("TRAJECTORY_MANAGER: Intersection");
                GD.Print("");
            }
            // there was no intersection found, but the object exits the SOI
            // the trajectory will enter the parent system
            else if(!double.IsNaN(endT))
            {
                sv = orbit.GetStateVector(endT) + system.GetStateVector(time + section.duration);

                system = orbit.planetarySystem.parentSystem;

                GD.Print("");
                GD.Print("TRAJECTORY_MANAGER: Exit");
                GD.Print("");
            }
            // there was no intersection found and the object does not exit the SOI
            // this is a full orbit and the trajectory ends here
            else
            {
                GD.Print("");
                GD.Print("TRAJECTORY_MANAGER: End");
                GD.Print("");

                trajectory.Add(section);

                return;
            }


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

        StateVector sv = section.orbit.GetStateVector(section.startT + t);

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

            GD.Print("++++++++++++++++++++++++++++++++++++++++");
            GD.Print("++++++++++++++++++++++++++++++++++++++++");
            GD.Print("++++++++++++++++++++++++++++++++++++++++");

            trajectoryRenderer.SetTrajectory(trajectory);

            trajectoryRenderer.Update();

            init = true;
        }

        if(update && !Engine.IsEditorHint())
        {
            TraverseTrajectory();
        }
    }
}
