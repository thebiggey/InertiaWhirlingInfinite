using System;
using Godot;

[Tool]
public partial class TerrainSettings : Resource
{
    [Export] public double radius;
    [Export] public NoiseSettings noiseSettings;

    public Vector3 Evaluate(Vector3 pointOnSphere, Noise noise)
    {
        return pointOnSphere * (radius + noiseSettings.Evaluate(pointOnSphere, noise));
    }
}
