using Godot;
using System;

[Tool]
public partial class TrajectorySection : Resource
{
    [Export] public Orbit orbit;
    [Export] public double startT;
    [Export] public double endT;

    public bool isFull
    {
        get {
            return double.IsNaN(endT);
        }
    }

    public double duration {
        get {
            if(isFull) return double.PositiveInfinity;

            double d = endT - startT;
            if(d < 0) d += orbit.T;

            return d;
        }
    }

    public TrajectorySection()
    {
        this.orbit = new Orbit();
        this.startT = 0;
        this.endT = double.NaN;
    }

    public TrajectorySection(Orbit orbit, double startT, double endT)
    {
        this.orbit = orbit;
        this.startT = startT;
        this.endT = endT;
    }

    public StateVector Evaluate(double t)
    {
        return orbit.GetStateVector((startT + t) % orbit.T);
    }
}
