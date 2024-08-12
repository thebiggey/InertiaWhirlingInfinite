using System;
using Godot;

[Tool]
public partial class PlanetarySystem : OrbitalNode
{
    [Export] double mass = 1000d;
    [Export] public double systemRadius = 100000d;

    PlanetaryBody planetaryBody;

    public double mu => mass * GravityManager.G;
}
