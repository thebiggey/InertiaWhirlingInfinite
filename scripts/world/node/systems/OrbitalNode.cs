using System;
using Godot;

[Tool]
public partial class OrbitalNode : WorldNode
{
    [ExportGroup("Parameters")]
    [Export] public Orbit orbit;
    [Export] public double startingT = 0;

    [ExportGroup("References")]
    [Export] public PlanetarySystem parentSystem;
    [Export] EllipseRenderer ellipseRenderer;

    [ExportGroup("Controls")]
    [Export] bool update = false;
    [Export] bool autoUpdate = false;
    [Export] bool setOrbit = false;
    [Export] Vector3 setVelocity = Vector3.Zero;
    [Export] bool printSV = false;

    public StateVector GetStateVector(double t)
    {
        if(orbit == null)
            return StateVector.Zero;

        return orbit.GetStateVector(t + startingT);
    }

    public StateVector GetStateVector()
    {
        return GetStateVector(Clock.t);
    }

    internal override void OnWorldLoad()
    {
        orbit.SetPlanetarySystem(parentSystem);
    }

    public override void _PhysicsProcess(double delta)
    {
        if(setOrbit)
        {
            setOrbit = false;

            orbit = new Orbit(parentSystem, new StateVector(Position, setVelocity));

            if(ellipseRenderer == null) return;

            ellipseRenderer.SetEllipse(orbit);
            ellipseRenderer.Update();
        }

        if(!Engine.IsEditorHint() || update || autoUpdate)
        {
            update = false;

            StateVector stateVector = GetStateVector();
            Position = stateVector.position;

            if(printSV)
            {
                GD.Print($"SV: {stateVector}");
                GD.Print($"Speed: {stateVector.velocity.Length()}");
                GD.Print($"--------------------");
            }
        }
    }
}
