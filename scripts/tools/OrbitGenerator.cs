using Godot;
using System;

[Tool]
public partial class OrbitGenerator : Node3D
{
	[Export] PlanetarySystem PlanetarySystem;
	[Export] bool setOrbit = false;
	[Export] Vector3 velocity;

	[Export] Orbit orbit;

	public override void _Ready()
	{

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(setOrbit)
		{
			setOrbit = false;

			orbit = new Orbit(PlanetarySystem, new StateVector(GlobalPosition, velocity) - PlanetarySystem._StateVector);

			GD.Print($"{orbit}");
		}
	}
}
