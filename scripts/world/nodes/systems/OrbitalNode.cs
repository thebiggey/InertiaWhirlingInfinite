using System;
using Godot;

[Tool]
public partial class OrbitalNode : WorldNode
{
    [Export] PlanetarySystem planetarySystem
    {
        get {
            return _planetarySystem;
        }
        set {
            orbit = new Orbit(value, new StateVector(Position, setVelocity));
            _planetarySystem = value;
        }
    }

    private PlanetarySystem _planetarySystem;

    [Export] Orbit orbit;
    [ExportGroup("Debug")]
    [Export] bool printStateVector = false;
    [Export] Vector3 setVelocity;
    [Export] bool updateOrbit = false;
    [ExportGroup("")]

    StateVector stateVector;

    public StateVector _StateVector {
        get {
            if(orbit == null) return StateVector.Zero;
            else return stateVector;
        }
    }

    internal override void Update(double delta)
    {
        //Debug
        if(updateOrbit)
        {
            updateOrbit = false;

            orbit = new Orbit(planetarySystem, new StateVector(Position, setVelocity));
        }

        if(orbit == null) return;

        stateVector = orbit.GetStateVector(Clock.t);
        Position = _StateVector.position;

        if(printStateVector)
            GD.Print($"{Name}: {stateVector}");
    }
}
