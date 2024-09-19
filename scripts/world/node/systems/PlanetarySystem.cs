using System;
using Godot;

[Tool]
public partial class PlanetarySystem : OrbitalNode
{
    [ExportGroup("Parameters")]
    [Export] double mass = 1000d;
    [Export] public double systemRadius = 100000d;

    [ExportGroup("References")]
    [Export] PlanetaryBody planetaryBody;
    [Export] public Godot.Collections.Array<PlanetarySystem> childSystems;

    Node3D childrenContainer;

    public double mu => mass * GravityManager.G;

    public bool isSunSystem => parentSystem == null;

    internal override void OnWorldLoad()
    {
        if(orbit != null)
            orbit.SetPlanetarySystem(parentSystem);

        childrenContainer = GetChild<Node3D>(1);
    }

    public void AddWorldChild(Node node)
    {
        NodeHelper.SetParent(node, this);
    }
}
