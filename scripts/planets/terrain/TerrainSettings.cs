using System;
using Godot;

[Tool]
public partial class TerrainSettings : Resource
{
    [Export] public double radius;
    [Export] public NoiseSettings noiseSettings;
}
