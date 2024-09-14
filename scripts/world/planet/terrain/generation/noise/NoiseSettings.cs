using Godot;
using Godot.Collections;
using System;

[Tool, Serializable]
public partial class NoiseSettings : Resource
{
	[Export] public Array<NoiseFilter> noiseFilters = new Array<NoiseFilter>();

    public double Evaluate(Vector3 point, Noise noise)
    {
        double elevation = 0;
        double firstLayerValue = 0;

        if(noiseFilters.Count > 0)
        {
            if(noiseFilters[0] != null && noiseFilters[0].enable)
            {
                firstLayerValue = noiseFilters[0].Evaluate(point, noise);
                elevation = firstLayerValue;
            }
        }

        for (int i = 1; i < noiseFilters.Count; i++)
        {
            if (noiseFilters[i] != null && noiseFilters[i].enable)
            {
                elevation += noiseFilters[i].Evaluate(point, noise) * (noiseFilters[i].useFirstLayerAsBase ? firstLayerValue : 1);
            }
        }

        return elevation;
    }
}
