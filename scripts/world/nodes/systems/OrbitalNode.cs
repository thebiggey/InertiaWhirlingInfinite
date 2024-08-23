using System;
using Godot;

[Tool]
public partial class OrbitalNode : WorldNode
{
    [Export] Orbit orbit;

    [ExportGroup("References")]
    [Export] PlanetarySystem system;
    [Export] EllipseRenderer ellipseRenderer;

    [ExportGroup("Controls")]
    [Export] bool update = false;
    [Export] bool autoUpdate = false;
    [Export] bool setOrbit = false;
    [Export] Vector3 setVelocity = Vector3.Zero;
    [Export] bool printVelocity = false;

    public override void _PhysicsProcess(double delta)
    {
        if(setOrbit)
        {
            setOrbit = false;

            orbit = new Orbit(system, new StateVector(Position, setVelocity));

            if(ellipseRenderer == null) return;

            ellipseRenderer.SetEllipse(orbit);
            ellipseRenderer.Update();
        }

        if(update || autoUpdate)
        {
            update = false;

            if(orbit == null) return;

            orbit.SetPlanetarySystem(system);

            StateVector stateVector = orbit.GetStateVector(Clock.t);
            Position = stateVector.position;

            if(printVelocity)
            {
                GD.Print($"Velocity: {stateVector.velocity}");
                GD.Print($"Speed: {stateVector.velocity.Length()}");
            }
        }
    }
}
