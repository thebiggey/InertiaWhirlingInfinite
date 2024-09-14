using Godot;
using Godot.Collections;
using System;

[Tool, Serializable]
public partial class NoiseFilter : Resource
{
    [Export] double baseAmplitude;
    [Export] double persistance;
    [Export] double baseRoughness;
    [Export] double roughness;
    [Export] int layers;
    [Export] Vector3 centre;
    [Export] double minValue;
    [Export] double transitionSmooth;
    [Export] public bool enable = true;
    [Export] public bool useFirstLayerAsBase = false;

    public NoiseFilter()
    {
        baseAmplitude = 1;
        baseRoughness = 1;
        roughness = 2;
        layers = 1;
        persistance = .5d;
        centre = Vector3.Zero;
        minValue = -1;
        transitionSmooth = 1;
        enable = true;
        useFirstLayerAsBase = false;
    }

    public double Evaluate(Vector3 point, Noise noise)
    {

        double value = 0f;
        double freq = baseRoughness;
        double amp = baseAmplitude;

        for (int i = 0; i < layers; i++)
        {
            double v = noise.Evaluate(point * freq + centre);
            value += v * 0.1d * amp;

            freq *= roughness;
            amp *= persistance;
        }

        value = Math.Max(minValue, value);
        return value;
    }
}
